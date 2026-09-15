using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LibraryExamAPI.Data;
using LibraryExamAPI.Hubs;
using LibraryExamAPI.Models;
using LibraryExamAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LibraryExamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibraryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RecommendationService _recommendationService;
    private readonly IHubContext<NotificationHub> _hub;

    public LibraryController(AppDbContext db, RecommendationService recommendationService, IHubContext<NotificationHub> hub)
    {
        _db = db;
        _recommendationService = recommendationService;
        _hub = hub;
    }

    [HttpGet("books")]
    [Authorize]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? q = null,
        [FromQuery] string? genre = null,
        [FromQuery] string sortBy = "title",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Books
            .Include(b => b.IssueRecords)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim();
            query = query.Where(b =>
                b.Title.Contains(search) ||
                b.Author.Contains(search) ||
                b.Isbn.Contains(search) ||
                b.Genre.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(genre) && !genre.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.Genre == genre);
        }

        query = sortBy.ToLowerInvariant() switch
        {
            "author" => query.OrderBy(b => b.Author).ThenBy(b => b.Title),
            "isbn" => query.OrderBy(b => b.Isbn),
            "copies" => query.OrderByDescending(b => b.CopiesAvailable),
            _ => query.OrderBy(b => b.Title)
        };

        var total = await query.CountAsync();

        var books = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new
            {
                bookId = b.BookId,
                title = b.Title,
                author = b.Author,
                isbn = b.Isbn,
                genre = b.Genre,
                copiesAvailable = b.CopiesAvailable,
                issueCount = b.IssueRecords.Count(ir => ir.ReturnDate == null),
                currentBorrowers = b.IssueRecords
                    .Where(ir => ir.ReturnDate == null)
                    .Select(ir => new
                    {
                        issueId = ir.IssueId,
                        studentId = ir.StudentId,
                        studentName = ir.Student.Name,
                        dueDate = ir.DueDate,
                        fineAmount = ir.FineAmount
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items = books
        });
    }

    [HttpGet("members")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetMembers()
    {
        var members = await _db.Students
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                studentId = s.StudentId,
                name = s.Name,
                rollNo = s.RollNo,
                dept = s.Dept,
                semester = s.Semester,
                email = s.Contact,
                role = s.Role,
                activeIssueCount = s.IssueRecords.Count(i => i.ReturnDate == null)
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpGet("issues")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetIssues()
    {
        var issues = await _db.IssueRecords
            .Include(i => i.Book)
            .Include(i => i.Student)
            .OrderByDescending(i => i.IssueDate)
            .Select(i => new
            {
                issueId = i.IssueId,
                studentId = i.StudentId,
                studentName = i.Student.Name,
                bookId = i.BookId,
                bookTitle = i.Book.Title,
                issueDate = i.IssueDate,
                dueDate = i.DueDate,
                returnDate = i.ReturnDate,
                fineAmount = i.FineAmount,
                isOverdue = i.ReturnDate == null && i.DueDate < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(issues);
    }

    [HttpGet("reports")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetReports()
    {
        var books = await _db.Books.Include(b => b.IssueRecords).ToListAsync();
        var activeIssues = await _db.IssueRecords
            .Include(i => i.Student)
            .Where(i => i.ReturnDate == null)
            .ToListAsync();

        var totalFine = activeIssues.Sum(i => i.FineAmount);
        var overdueCount = activeIssues.Count(i => i.DueDate < DateTime.UtcNow);

        var report = new
        {
            totalBooks = books.Count,
            totalAvailableCopies = books.Sum(b => b.CopiesAvailable),
            activeIssueCount = activeIssues.Count,
            overdueCount,
            totalFine,
            topBooks = books
                .OrderByDescending(b => b.IssueRecords.Count(i => i.ReturnDate == null))
                .Take(5)
                .Select(b => new { b.BookId, b.Title, activeIssueCount = b.IssueRecords.Count(i => i.ReturnDate == null) })
                .ToList()
        };

        return Ok(report);
    }

    [HttpGet("my-issues")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyIssues()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { message = "User email is not available in the token." });
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Contact == email);
        if (student == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var issues = await _db.IssueRecords
            .Include(i => i.Book)
            .Where(i => i.StudentId == student.StudentId)
            .OrderByDescending(i => i.IssueDate)
            .Select(i => new
            {
                issueId = i.IssueId,
                bookId = i.BookId,
                bookTitle = i.Book.Title,
                author = i.Book.Author,
                issueDate = i.IssueDate,
                dueDate = i.DueDate,
                returnDate = i.ReturnDate,
                fineAmount = i.FineAmount,
                isOverdue = i.ReturnDate == null && i.DueDate < DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(issues);
    }

    [HttpPost("books")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var duplicate = await _db.Books.AnyAsync(b => b.Isbn == request.Isbn.Trim());
        if (duplicate)
        {
            return BadRequest(new { message = "A book with this ISBN already exists." });
        }

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = request.Isbn.Trim(),
            Genre = request.Genre.Trim(),
            CopiesAvailable = request.CopiesAvailable,
            CoverImageUrl = request.CoverImageUrl,
            Description = request.Description,
            Publisher = request.Publisher,
            PublishedYear = request.PublishedYear
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Book added successfully.", bookId = book.BookId });
    }

    [HttpPut("books/{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null)
        {
            return NotFound(new { message = "Book not found." });
        }

        var isDuplicateIsbn = await _db.Books.AnyAsync(b => b.BookId != id && b.Isbn == request.Isbn.Trim());
        if (isDuplicateIsbn)
        {
            return BadRequest(new { message = "Another book already uses this ISBN." });
        }

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Isbn = request.Isbn.Trim();
        book.Genre = request.Genre.Trim();
        book.CopiesAvailable = request.CopiesAvailable;
        book.CoverImageUrl = request.CoverImageUrl;
        book.Description = request.Description;
        book.Publisher = request.Publisher;
        book.PublishedYear = request.PublishedYear;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Book updated successfully." });
    }

    [HttpDelete("books/{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _db.Books
            .Include(b => b.IssueRecords)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null)
        {
            return NotFound(new { message = "Book not found." });
        }

        if (book.IssueRecords.Any(i => i.ReturnDate == null))
        {
            return BadRequest(new { message = "Cannot delete a book that is currently issued." });
        }

        _db.Books.Remove(book);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Book deleted successfully." });
    }

    [HttpGet("recommendations")]
    [Authorize]
    public async Task<IActionResult> GetRecommendations()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        var student = !string.IsNullOrWhiteSpace(email)
            ? await _db.Students.FirstOrDefaultAsync(s => s.Contact == email)
            : null;

        var recommendationResult = await _recommendationService.GetRecommendationsAsync(_db, student?.StudentId);

        return Ok(new
        {
            recommendations = recommendationResult.Recommendations,
            basedOn = recommendationResult.BasedOn
        });
    }

    [HttpGet("books/{id:int}/details")]
    [Authorize]
    public async Task<IActionResult> GetBookDetails(int id)
    {
        var book = await _db.Books
            .Include(b => b.Comments)
            .ThenInclude(c => c.Student)
            .Include(b => b.Reactions)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null)
        {
            return NotFound(new { message = "Book not found." });
        }

        var comments = book.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                commentId = c.CommentId,
                studentName = c.Student.Name,
                studentRole = c.Student.Role,
                commentText = c.CommentText,
                createdAt = c.CreatedAt
            })
            .ToList();

        var reactionSummary = new
        {
            total = book.Reactions.Count,
            likes = book.Reactions.Count(r => r.Type.Equals("like", StringComparison.OrdinalIgnoreCase)),
            dislikes = book.Reactions.Count(r => r.Type.Equals("dislike", StringComparison.OrdinalIgnoreCase))
        };

        return Ok(new
        {
            bookId = book.BookId,
            title = book.Title,
            author = book.Author,
            isbn = book.Isbn,
            genre = book.Genre,
            description = book.Description,
            coverImageUrl = book.CoverImageUrl,
            copiesAvailable = book.CopiesAvailable,
            comments,
            reactions = reactionSummary
        });
    }

    [HttpPost("books/{id:int}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null)
        {
            return NotFound(new { message = "Book not found." });
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { message = "User email is not available in the token." });
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Contact == email);
        if (student == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var comment = new Comment
        {
            BookId = id,
            StudentId = student.StudentId,
            CommentText = request.CommentText.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("ReceiveNotification", new
        {
            type = "comment",
            bookId = id,
            message = $"{student.Name} commented on '{book.Title}'.",
            createdAt = DateTime.UtcNow
        });

        return Ok(new { message = "Comment added successfully.", commentId = comment.CommentId });
    }

    [HttpPost("books/{id:int}/reactions")]
    [Authorize]
    public async Task<IActionResult> AddReaction(int id, [FromBody] CreateReactionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null)
        {
            return NotFound(new { message = "Book not found." });
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { message = "User email is not available in the token." });
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Contact == email);
        if (student == null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var existing = await _db.Reactions.FirstOrDefaultAsync(r => r.BookId == id && r.StudentId == student.StudentId);
        if (existing != null)
        {
            if (existing.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase))
            {
                _db.Reactions.Remove(existing);
                await _db.SaveChangesAsync();
                return Ok(new { message = "Reaction removed." });
            }

            existing.Type = request.Type.Trim();
            await _db.SaveChangesAsync();
            return Ok(new { message = "Reaction updated." });
        }

        var reaction = new Reaction
        {
            BookId = id,
            StudentId = student.StudentId,
            Type = request.Type.Trim()
        };

        _db.Reactions.Add(reaction);
        await _db.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("ReceiveNotification", new
        {
            type = "reaction",
            bookId = id,
            message = $"{student.Name} {request.Type.Trim()}d '{book.Title}'.",
            createdAt = DateTime.UtcNow
        });

        return Ok(new { message = "Reaction added successfully." });
    }

    [HttpPost("issues")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> IssueBook([FromBody] IssueBookRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == request.StudentId);
        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        var book = await _db.Books
            .Include(b => b.IssueRecords)
            .FirstOrDefaultAsync(b => b.BookId == request.BookId);

        if (book == null)
        {
            return NotFound(new { message = "Book not found." });
        }

        var existingActiveIssue = await _db.IssueRecords.AnyAsync(i =>
            i.BookId == request.BookId &&
            i.StudentId == request.StudentId &&
            i.ReturnDate == null);

        if (existingActiveIssue)
        {
            return BadRequest(new { message = "This student already has this book on issue." });
        }

        if (book.CopiesAvailable <= 0)
        {
            return BadRequest(new { message = "No copies available for this book." });
        }

        var dueDate = DateTime.UtcNow.AddDays(request.DueDays > 0 ? request.DueDays : 14);

        var issue = new IssueRecord
        {
            BookId = request.BookId,
            StudentId = request.StudentId,
            IssueDate = DateTime.UtcNow,
            DueDate = dueDate,
            FineAmount = 0m
        };

        _db.IssueRecords.Add(issue);
        book.CopiesAvailable -= 1;

        await _db.AuditLogs.AddAsync(new AuditLog
        {
            Action = "IssueBook",
            EntityType = "IssueRecord",
            EntityId = 0,
            PerformedBy = User.Identity?.Name ?? "system",
            Details = $"Issued '{book.Title}' to student id {request.StudentId}."
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Book issued successfully.", issueId = issue.IssueId });
    }

    [HttpPost("issues/{id:int}/return")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var issue = await _db.IssueRecords
            .Include(i => i.Book)
            .FirstOrDefaultAsync(i => i.IssueId == id);

        if (issue == null)
        {
            return NotFound(new { message = "Issue record not found." });
        }

        if (issue.ReturnDate != null)
        {
            return BadRequest(new { message = "This book has already been returned." });
        }

        issue.ReturnDate = DateTime.UtcNow;
        issue.FineAmount = CalculateFine(issue);
        issue.Book.CopiesAvailable += 1;

        await _db.AuditLogs.AddAsync(new AuditLog
        {
            Action = "ReturnBook",
            EntityType = "IssueRecord",
            EntityId = issue.IssueId,
            PerformedBy = User.Identity?.Name ?? "system",
            Details = $"Returned book '{issue.Book.Title}' and applied fine {issue.FineAmount}."
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Book returned successfully.", fineAmount = issue.FineAmount });
    }

    private static decimal CalculateFine(IssueRecord issue)
    {
        if (issue.ReturnDate == null)
        {
            return 0m;
        }

        var overdueDays = Math.Max(0, (issue.ReturnDate.Value.Date - issue.DueDate.Date).Days);
        return overdueDays * 25m;
    }
}

public class CreateBookRequest
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Author { get; set; } = string.Empty;

    [Required, MinLength(5)]
    public string Isbn { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Genre { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int CopiesAvailable { get; set; } = 1;

    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public int PublishedYear { get; set; } = DateTime.UtcNow.Year;
}

public class UpdateBookRequest
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Author { get; set; } = string.Empty;

    [Required, MinLength(5)]
    public string Isbn { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Genre { get; set; } = string.Empty;

    [Range(0, 1000)]
    public int CopiesAvailable { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Publisher { get; set; }
    public int PublishedYear { get; set; } = DateTime.UtcNow.Year;
}

public class IssueBookRequest
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Range(1, 90)]
    public int DueDays { get; set; } = 14;
}

public class CreateCommentRequest
{
    [Required, MinLength(2), MaxLength(2000)]
    public string CommentText { get; set; } = string.Empty;
}

public class CreateReactionRequest
{
    [Required, MinLength(2), MaxLength(20)]
    public string Type { get; set; } = "like";
}

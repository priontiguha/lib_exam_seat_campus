using LibraryExamAPI.Data;
using LibraryExamAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryExamAPI.Services;

public class RecommendationService
{
    public async Task<RecommendationResult> GetRecommendationsAsync(AppDbContext db, int? studentId)
    {
        var preferredGenres = new List<string>();

        if (studentId.HasValue)
        {
            preferredGenres = await db.IssueRecords
                .Where(i => i.StudentId == studentId.Value && i.ReturnDate != null)
                .Join(db.Books,
                    issue => issue.BookId,
                    book => book.BookId,
                    (issue, book) => book.Genre)
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => g.Key)
                .ToListAsync();
        }

        var query = db.Books.AsQueryable();
        if (preferredGenres.Count > 0)
        {
            query = query.Where(b => preferredGenres.Contains(b.Genre));
        }

        var recommendations = await query
            .OrderByDescending(b => b.CopiesAvailable)
            .ThenBy(b => b.Title)
            .Take(5)
            .Select(b => new
            {
                bookId = b.BookId,
                title = b.Title,
                author = b.Author,
                genre = b.Genre,
                copiesAvailable = b.CopiesAvailable
            })
            .ToListAsync();

        if (recommendations.Count == 0)
        {
            recommendations = await db.Books
                .OrderBy(b => b.Title)
                .Take(5)
                .Select(b => new
                {
                    bookId = b.BookId,
                    title = b.Title,
                    author = b.Author,
                    genre = b.Genre,
                    copiesAvailable = b.CopiesAvailable
                })
                .ToListAsync();
        }

        return new RecommendationResult
        {
            Recommendations = recommendations,
            BasedOn = preferredGenres.Count > 0 ? preferredGenres.Cast<object>().ToList() : new List<object> { "general circulation" }
        };
    }
}

public class RecommendationResult
{
    public object Recommendations { get; set; } = new();
    public List<object> BasedOn { get; set; } = new();
}

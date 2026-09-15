using LibraryExamAPI.Data;
using LibraryExamAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryExamAPI;

public class QueryType
{
    public async Task<List<Book>> GetBooksAsync(AppDbContext db, string? genre = null)
    {
        var query = db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(b => b.Genre == genre);
        }

        return await query.OrderBy(b => b.Title).Take(20).ToListAsync();
    }

    public async Task<List<Student>> GetStudentsAsync(AppDbContext db)
    {
        return await db.Students.OrderBy(s => s.Name).Take(20).ToListAsync();
    }

    public async Task<List<SeatAllocation>> GetSeatAllocationsAsync(AppDbContext db, int? examId = null)
    {
        var query = db.SeatAllocations.AsQueryable();

        if (examId.HasValue)
        {
            query = query.Where(s => s.ExamId == examId.Value);
        }

        return await query.OrderBy(s => s.BenchNo).ThenBy(s => s.SeatNo).Take(50).ToListAsync();
    }

    public async Task<List<Book>> GetRecommendedBooksAsync(AppDbContext db)
    {
        return await db.Books
            .OrderByDescending(b => b.CopiesAvailable)
            .Take(5)
            .ToListAsync();
    }
}

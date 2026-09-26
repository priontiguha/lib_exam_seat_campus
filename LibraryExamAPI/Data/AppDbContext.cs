using LibraryExamAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryExamAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<IssueRecord> IssueRecords { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<SeatAllocation> SeatAllocations { get; set; }
    public DbSet<Invigilator> Invigilators { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().ToTable("Students");
        modelBuilder.Entity<Book>().ToTable("Books");
        modelBuilder.Entity<IssueRecord>().ToTable("IssueRecords");
        modelBuilder.Entity<Comment>().ToTable("Comments");
        modelBuilder.Entity<Reaction>().ToTable("Reactions");
        modelBuilder.Entity<Exam>().ToTable("Exams");
        modelBuilder.Entity<Room>().ToTable("Rooms");
        modelBuilder.Entity<SeatAllocation>().ToTable("SeatAllocations");
        modelBuilder.Entity<Invigilator>().ToTable("Invigilators");
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");

        modelBuilder.Entity<IssueRecord>()
            .HasOne(ir => ir.Book)
            .WithMany(b => b.IssueRecords)
            .HasForeignKey(ir => ir.BookId);

        modelBuilder.Entity<IssueRecord>()
            .HasOne(ir => ir.Student)
            .WithMany(s => s.IssueRecords)
            .HasForeignKey(ir => ir.StudentId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Book)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BookId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Student)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.StudentId);

        modelBuilder.Entity<Reaction>()
            .HasOne(r => r.Book)
            .WithMany(b => b.Reactions)
            .HasForeignKey(r => r.BookId);

        modelBuilder.Entity<Reaction>()
            .HasOne(r => r.Student)
            .WithMany(s => s.Reactions)
            .HasForeignKey(r => r.StudentId);

        modelBuilder.Entity<SeatAllocation>()
            .HasOne(sa => sa.Exam)
            .WithMany(e => e.SeatAllocations)
            .HasForeignKey(sa => sa.ExamId);

        modelBuilder.Entity<SeatAllocation>()
            .HasOne(sa => sa.Room)
            .WithMany(r => r.SeatAllocations)
            .HasForeignKey(sa => sa.RoomId);

        modelBuilder.Entity<SeatAllocation>()
            .HasOne(sa => sa.Student)
            .WithMany(s => s.SeatAllocations)
            .HasForeignKey(sa => sa.StudentId);

        modelBuilder.Entity<SeatAllocation>()
            .HasOne(sa => sa.Invigilator)
            .WithMany(i => i.SeatAllocations)
            .HasForeignKey(sa => sa.InvigilatorId);

        base.OnModelCreating(modelBuilder);
    }
}

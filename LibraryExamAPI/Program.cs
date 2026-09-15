using System.Text;
using HotChocolate;
using LibraryExamAPI;
using LibraryExamAPI.Data;
using LibraryExamAPI.Hubs;
using LibraryExamAPI.Models;
using LibraryExamAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LibraryExamAPI",
        Version = "v1",
        Description = "Library and exam portal API"
    });
});
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["JwtSettings:Key"] ?? "P@ssw0rd!SuperSecretKeyForDevelopmentOnly123456";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "LibraryExamAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "LibraryExamClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("LibrarianOrAdmin", policy => policy.RequireRole("Admin", "Librarian"));
    options.AddPolicy("ExamCoordinatorOrAdmin", policy => policy.RequireRole("Admin", "Exam Coordinator"));
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<IOtpEmailService, OtpEmailService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<QueryType>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryExamAPI v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowClientApp");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapGraphQL();
app.MapControllers();

app.MapGet("/api/ping", () => new { status = "ok", message = "LibraryExamAPI is running." })
   .WithName("Ping");

app.MapGet("/api/health", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return new { status = canConnect ? "Database connected" : "Database unavailable" };
})
.WithName("HealthCheck");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var providerName = db.Database.ProviderName ?? string.Empty;

    if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        await db.Database.EnsureCreatedAsync();
    }
    else if (db.Database.IsRelational())
    {
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: database migration failed: {ex.Message}");
            // Continue startup even if migrations fail (useful for local/dev setups)
        }
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    try
    {
        var defaultAdminEmail = builder.Configuration["DefaultAdmin:Email"] ?? "admin@library.edu";
        var defaultAdminPassword = builder.Configuration["DefaultAdmin:Password"] ?? "Admin@123";

        if (!await db.Students.AnyAsync(s => s.Role == "Admin" || s.Contact == defaultAdminEmail))
        {
            db.Students.Add(new Student
            {
                Name = "System Administrator",
                RollNo = "ADMIN-001",
                Dept = "Administration",
                Semester = 1,
                Contact = defaultAdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword),
                Role = "Admin",
                IsVerified = true
            });
        }

        if (!await db.Books.AnyAsync())
        {
            db.Books.AddRange(
                new Book
                {
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Isbn = "9780132350884",
                    Genre = "Software Engineering",
                    CopiesAvailable = 5,
                    Description = "A handbook of agile software craftsmanship.",
                    Publisher = "Prentice Hall",
                    PublishedYear = 2008
                },
                new Book
                {
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt",
                    Isbn = "9780201616224",
                    Genre = "Programming",
                    CopiesAvailable = 4,
                    Description = "Practical advice for modern software developers.",
                    Publisher = "Addison-Wesley",
                    PublishedYear = 1999
                },
                new Book
                {
                    Title = "Database System Concepts",
                    Author = "Abraham Silberschatz",
                    Isbn = "9780078022159",
                    Genre = "Database",
                    CopiesAvailable = 3,
                    Description = "Foundational concepts in database design and management.",
                    Publisher = "McGraw-Hill",
                    PublishedYear = 2010
                }
            );
        }

        if (!await db.Rooms.AnyAsync())
        {
            db.Rooms.AddRange(
                new Room { RoomNo = "A-101", Capacity = 40, BenchLayout = "4x10" },
                new Room { RoomNo = "B-204", Capacity = 60, BenchLayout = "5x12" },
                new Room { RoomNo = "C-301", Capacity = 50, BenchLayout = "5x10" }
            );
        }

        if (!await db.Invigilators.AnyAsync())
        {
            db.Invigilators.AddRange(
                new Invigilator { Name = "Nadia Rahman", Dept = "Computer Science" },
                new Invigilator { Name = "Imran Hossain", Dept = "Mathematics" },
                new Invigilator { Name = "Sadia Akter", Dept = "Physics" }
            );
        }

        if (!await db.Exams.AnyAsync())
        {
            db.Exams.AddRange(
                new Exam { Course = "CSE 201", Semester = 2, ExamDate = DateTime.UtcNow.AddDays(7), TimeSlot = "09:00-11:00" },
                new Exam { Course = "MATH 250", Semester = 2, ExamDate = DateTime.UtcNow.AddDays(9), TimeSlot = "13:00-15:00" }
            );
        }

        if (!await db.Students.AnyAsync(s => s.Role == "Student"))
        {
            db.Students.AddRange(
                new Student
                {
                    Name = "Student One",
                    RollNo = "CS-2024-001",
                    Dept = "Computer Science",
                    Semester = 2,
                    Contact = "student1@library.edu",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "Student",
                    IsVerified = true
                },
                new Student
                {
                    Name = "Student Two",
                    RollNo = "CS-2024-002",
                    Dept = "Computer Science",
                    Semester = 2,
                    Contact = "student2@library.edu",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                    Role = "Student",
                    IsVerified = true
                }
            );
        }

        var librarianEmail = "librarian1@library.edu";
        var librarian = await db.Students.FirstOrDefaultAsync(s => s.Contact == librarianEmail);
        if (librarian == null)
        {
            db.Students.Add(new Student
            {
                Name = "Library Librarian",
                RollNo = "LIB-001",
                Dept = "Library Services",
                Semester = 1,
                Contact = librarianEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lib@1234"),
                Role = "Librarian",
                IsVerified = true
            });
        }
        else
        {
            librarian.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lib@1234");
            librarian.Role = "Librarian";
            librarian.IsVerified = true;
        }

        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: database seeding failed: {ex.Message}");
    }
}

app.Run();

public partial class Program { }

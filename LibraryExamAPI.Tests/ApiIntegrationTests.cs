using System.Net;
using System.Net.Http.Json;
using LibraryExamAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LibraryExamAPI.Tests;

public class ApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PingEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PingResponse>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload!.status);
    }

    [Fact]
    public async Task RegisterVerifyAndLogin_ShouldSucceed()
    {
        var registerRequest = new
        {
            name = "Integration Tester",
            rollNo = "INT-9001",
            dept = "Computer Science",
            semester = 3,
            email = "integration.test@example.com",
            password = "Test@1234",
            role = "Student"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerPayload = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registerPayload);
        Assert.False(string.IsNullOrWhiteSpace(registerPayload!.otpCode));

        var verifyResponse = await _client.PostAsJsonAsync("/api/Auth/verify-otp", new
        {
            email = registerRequest.email,
            otpCode = registerPayload.otpCode
        });

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = registerRequest.email,
            password = registerRequest.password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);
        Assert.False(string.IsNullOrWhiteSpace(loginPayload!.token));
    }

    [Fact]
    public async Task StudentCanFetchTheirExamRoutine()
    {
        var email = "routine.student@example.com";
        var password = "Routine@123";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LibraryExamAPI.Data.AppDbContext>();
            db.Students.RemoveRange(db.Students.Where(s => s.Contact == email));
            db.Students.Add(new LibraryExamAPI.Models.Student
            {
                Name = "Routine Student",
                RollNo = "INT-9002",
                Dept = "Computer Science",
                Semester = 3,
                Contact = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Student",
                IsVerified = true
            });
            db.Exams.RemoveRange(db.Exams.Where(e => e.Course == "Algorithms"));
            db.Exams.Add(new LibraryExamAPI.Models.Exam
            {
                Course = "Algorithms",
                Semester = 3,
                ExamDate = new DateTime(2026, 10, 12),
                TimeSlot = "09:00-11:00"
            });
            await db.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginPayload!.token);

        var routineResponse = await _client.GetAsync("/api/Exam/my-routine");
        Assert.Equal(HttpStatusCode.OK, routineResponse.StatusCode);

        var routine = await routineResponse.Content.ReadFromJsonAsync<List<ExamRoutineItem>>();
        Assert.NotNull(routine);
        Assert.Contains(routine!, item => item.course == "Algorithms" && item.semester == 3);
    }

    private sealed class PingResponse
    {
        public string status { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }

    private sealed class RegisterResponse
    {
        public string message { get; set; } = string.Empty;
        public string otpCode { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public string token { get; set; } = string.Empty;
    }

    private sealed class ExamRoutineItem
    {
        public int examId { get; set; }
        public string course { get; set; } = string.Empty;
        public int semester { get; set; }
        public DateTime examDate { get; set; }
        public string timeSlot { get; set; } = string.Empty;
        public string roomNo { get; set; } = string.Empty;
        public int benchNo { get; set; }
        public int seatNo { get; set; }
    }
}

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

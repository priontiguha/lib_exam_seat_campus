using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LibraryExamAPI.Data;
using LibraryExamAPI.Models;
using LibraryExamAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryExamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly string[] AllowedRoles =
    [
        "Student",
        "Librarian",
        "Exam Coordinator",
        "Admin"
    ];

    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IOtpEmailService _otpEmailService;
    private readonly IWebHostEnvironment _env;

    public AuthController(AppDbContext db, JwtTokenService jwtTokenService, IOtpEmailService otpEmailService, IWebHostEnvironment env)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _otpEmailService = otpEmailService;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (await _db.Students.AnyAsync(s => s.RollNo == request.RollNo || s.Contact == request.Email))
        {
            return BadRequest(new { message = "A student with this roll number or email already exists." });
        }

        var otpCode = LibraryRules.GenerateOtpCode();
        var student = new Student
        {
            Name = request.Name,
            RollNo = request.RollNo,
            Dept = request.Dept,
            Semester = request.Semester,
            Contact = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsVerified = false
        };

        OtpStore[request.Email.Trim()] = otpCode;
        await _otpEmailService.SendOtpAsync(request.Email.Trim(), otpCode);

        await _db.AuditLogs.AddAsync(new AuditLog
        {
            Action = "RegisterUser",
            EntityType = "Student",
            EntityId = 0,
            PerformedBy = request.Email,
            Details = $"Registration initiated. OTP: {otpCode}."
        });

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Registration successful. Please verify your OTP.", studentId = student.StudentId, otpCode });
    }

    private static readonly Dictionary<string, string> OtpStore = new(StringComparer.OrdinalIgnoreCase);

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Contact == request.Email);
        if (student == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var otpCode = LibraryRules.GenerateOtpCode();
        OtpStore[request.Email.Trim()] = otpCode;
        await _otpEmailService.SendOtpAsync(request.Email.Trim(), otpCode);

        return Ok(new { message = "OTP sent successfully.", otpCode });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Contact == request.Email);
        if (student == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (student.IsVerified)
        {
            return Ok(new { message = "User is already verified." });
        }

        if (!OtpStore.TryGetValue(request.Email.Trim(), out var validOtp) || !validOtp.Equals(request.OtpCode.Trim(), StringComparison.Ordinal))
        {
            return BadRequest(new { message = "Invalid OTP code." });
        }

        student.IsVerified = true;
        OtpStore.Remove(request.Email.Trim());
        await _db.SaveChangesAsync();

        return Ok(new { message = "OTP verified successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.Contact == request.Email);

        if (student == null || string.IsNullOrWhiteSpace(student.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwtTokenService.GenerateToken(student.Contact, student.Role);

        return Ok(new
        {
            token,
            message = student.IsVerified ? "Login successful." : "Login successful. Please verify your OTP for full account activation.",
            user = new
            {
                studentId = student.StudentId,
                name = student.Name,
                rollNo = student.RollNo,
                dept = student.Dept,
                semester = student.Semester,
                email = student.Contact,
                role = student.Role,
                isVerified = student.IsVerified
            }
        });
    }

    [HttpPost("admin/create-user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUserByAdmin([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!AllowedRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Role must be one of: Student, Librarian, Exam Coordinator, Admin." });
        }

        if (await _db.Students.AnyAsync(s => s.RollNo == request.RollNo || s.Contact == request.Email))
        {
            return BadRequest(new { message = "A user with this roll number or email already exists." });
        }

        var user = new Student
        {
            Name = request.Name,
            RollNo = request.RollNo,
            Dept = request.Dept,
            Semester = request.Semester,
            Contact = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsVerified = request.Role == "Admin"
        };

        _db.Students.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"{request.Role} account created successfully.", userId = user.StudentId });
    }

    // Development-only: seed default users (Admin, Student, Librarian)
    [HttpPost("admin/seed-defaults")]
    public async Task<IActionResult> SeedDefaults()
    {
        if (!_env.IsDevelopment())
        {
            return Forbid();
        }

        var defaults = new[]
        {
            new { Name = "System Administrator", RollNo = "ADMIN-001", Dept = "Administration", Semester = 1, Email = "admin@library.edu", Password = "Admin@123", Role = "Admin", IsVerified = true },
            new { Name = "Student One", RollNo = "CS-2024-001", Dept = "Computer Science", Semester = 2, Email = "student1@library.edu", Password = "Student@123", Role = "Student", IsVerified = true },
            new { Name = "Library Librarian", RollNo = "LIB-001", Dept = "Library Services", Semester = 1, Email = "librarian1@library.edu", Password = "Lib@1234", Role = "Librarian", IsVerified = true }
        };

        foreach (var d in defaults)
        {
            var existing = await _db.Students.FirstOrDefaultAsync(s => s.Contact == d.Email);
            if (existing == null)
            {
                _db.Students.Add(new Student
                {
                    Name = d.Name,
                    RollNo = d.RollNo,
                    Dept = d.Dept,
                    Semester = d.Semester,
                    Contact = d.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(d.Password),
                    Role = d.Role,
                    IsVerified = d.IsVerified
                });
            }
            else
            {
                existing.Name = d.Name;
                existing.RollNo = d.RollNo;
                existing.Dept = d.Dept;
                existing.Semester = d.Semester;
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(d.Password);
                existing.Role = d.Role;
                existing.IsVerified = d.IsVerified;
            }
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Seeding failed.", detail = ex.Message });
        }

        return Ok(new { message = "Default users seeded/updated." });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Students
            .OrderBy(s => s.Role)
            .ThenBy(s => s.Name)
            .Select(s => new
            {
                studentId = s.StudentId,
                name = s.Name,
                rollNo = s.RollNo,
                dept = s.Dept,
                semester = s.Semester,
                email = s.Contact,
                role = s.Role,
                isVerified = s.IsVerified
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var user = await _db.Students
            .Where(s => s.Contact == email)
            .Select(s => new
            {
                studentId = s.StudentId,
                name = s.Name,
                rollNo = s.RollNo,
                dept = s.Dept,
                semester = s.Semester,
                email = s.Contact,
                role = s.Role,
                isVerified = s.IsVerified
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("admin/users/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserByAdmin(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!AllowedRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Role must be one of: Student, Librarian, Exam Coordinator, Admin." });
        }

        var user = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var duplicateExists = await _db.Students
            .AnyAsync(s => s.StudentId != id && (s.RollNo == request.RollNo || s.Contact == request.Email));
        if (duplicateExists)
        {
            return BadRequest(new { message = "A user with this roll number or email already exists." });
        }

        user.Name = request.Name;
        user.RollNo = request.RollNo;
        user.Dept = request.Dept;
        user.Semester = request.Semester;
        user.Contact = request.Email;
        user.Role = request.Role;
        user.IsVerified = request.IsVerified;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("admin/users/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUserByAdmin(int id)
    {
        var user = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        _db.Students.Remove(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User deleted successfully." });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logout successful." });
    }

    [HttpPost("reset-password")]
    [Authorize]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var student = await _db.Students.FirstOrDefaultAsync(s => s.Contact == email);
        if (student == null)
        {
            return NotFound();
        }

        student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password reset successful." });
    }

    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromQuery] string email = "", [FromQuery] string username = "")
    {
        var emailTaken = !string.IsNullOrWhiteSpace(email) && await _db.Students.AnyAsync(s => s.Contact == email);
        var usernameTaken = !string.IsNullOrWhiteSpace(username) && await _db.Students.AnyAsync(s => s.RollNo == username);

        return Ok(new { emailTaken, usernameTaken });
    }
}
public class SendOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(6)]
    public string OtpCode { get; set; } = string.Empty;
}
public class RegisterRequest
{
    [Required, MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string RollNo { get; set; } = string.Empty;

    [Required]
    public string Dept { get; set; } = string.Empty;

    [Required]
    public int Semester { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    [Required, MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string RollNo { get; set; } = string.Empty;

    [Required]
    public string Dept { get; set; } = string.Empty;

    [Required]
    public int Semester { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";
}

public class UpdateUserRequest
{
    [Required, MinLength(3)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string RollNo { get; set; } = string.Empty;

    [Required]
    public string Dept { get; set; } = string.Empty;

    [Required]
    public int Semester { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    [Required]
    public string Role { get; set; } = "Student";

    public bool IsVerified { get; set; }
}

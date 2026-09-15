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
public class ExamController : ControllerBase
{
    private readonly AppDbContext _db;

    public ExamController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("rooms")]
    [Authorize(Roles = "Admin,Exam Coordinator,Librarian")]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _db.Rooms
            .OrderBy(r => r.RoomNo)
            .Select(r => new
            {
                roomId = r.RoomId,
                roomNo = r.RoomNo,
                capacity = r.Capacity,
                benchLayout = r.BenchLayout,
                occupiedSeats = r.SeatAllocations.Count()
            })
            .ToListAsync();

        return Ok(rooms);
    }

    [HttpPost("rooms")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var room = new Room
        {
            RoomNo = request.RoomNo.Trim(),
            Capacity = request.Capacity,
            BenchLayout = request.BenchLayout.Trim()
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Room created successfully.", roomId = room.RoomId });
    }

    [HttpPut("rooms/{id:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] CreateRoomRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound(new { message = "Room not found." });
        room.RoomNo = request.RoomNo.Trim();
        room.Capacity = request.Capacity;
        room.BenchLayout = request.BenchLayout.Trim();
        await _db.SaveChangesAsync();
        return Ok(new { message = "Room updated successfully." });
    }

    [HttpDelete("rooms/{id:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _db.Rooms.Include(r => r.SeatAllocations).FirstOrDefaultAsync(r => r.RoomId == id);
        if (room == null) return NotFound(new { message = "Room not found." });
        if (room.SeatAllocations.Count > 0) return BadRequest(new { message = "Cannot delete a room with seat allocations." });
        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Room deleted successfully." });
    }

    [HttpGet("invigilators")]
    [Authorize(Roles = "Admin,Exam Coordinator,Librarian")]
    public async Task<IActionResult> GetInvigilators()
    {
        var invigilators = await _db.Invigilators
            .OrderBy(i => i.Name)
            .Select(i => new
            {
                staffId = i.StaffId,
                name = i.Name,
                dept = i.Dept,
                assignedCount = i.SeatAllocations.Count()
            })
            .ToListAsync();

        return Ok(invigilators);
    }

    [HttpPost("invigilators")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> CreateInvigilator([FromBody] CreateInvigilatorRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var invigilator = new Invigilator
        {
            Name = request.Name.Trim(),
            Dept = request.Dept.Trim()
        };

        _db.Invigilators.Add(invigilator);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Invigilator added successfully.", staffId = invigilator.StaffId });
    }

    [HttpPut("invigilators/{id:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> UpdateInvigilator(int id, [FromBody] CreateInvigilatorRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var invigilator = await _db.Invigilators.FindAsync(id);
        if (invigilator == null) return NotFound(new { message = "Invigilator not found." });
        invigilator.Name = request.Name.Trim();
        invigilator.Dept = request.Dept.Trim();
        await _db.SaveChangesAsync();
        return Ok(new { message = "Invigilator updated successfully." });
    }

    [HttpDelete("invigilators/{id:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> DeleteInvigilator(int id)
    {
        var invigilator = await _db.Invigilators.Include(i => i.SeatAllocations).FirstOrDefaultAsync(i => i.StaffId == id);
        if (invigilator == null) return NotFound(new { message = "Invigilator not found." });
        if (invigilator.SeatAllocations.Count > 0) return BadRequest(new { message = "Cannot delete an invigilator with seat assignments." });
        _db.Invigilators.Remove(invigilator);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Invigilator deleted successfully." });
    }

    [HttpGet("exams")]
    [Authorize(Roles = "Admin,Exam Coordinator,Librarian")]
    public async Task<IActionResult> GetExams()
    {
        var exams = await _db.Exams
            .Include(e => e.SeatAllocations)
            .OrderBy(e => e.ExamDate)
            .Select(e => new
            {
                examId = e.ExamId,
                course = e.Course,
                semester = e.Semester,
                examDate = e.ExamDate,
                timeSlot = e.TimeSlot,
                seatCount = e.SeatAllocations.Count
            })
            .ToListAsync();

        return Ok(exams);
    }

    [HttpPost("exams")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var exam = new Exam
        {
            Course = request.Course.Trim(),
            Semester = request.Semester,
            ExamDate = request.ExamDate,
            TimeSlot = request.TimeSlot.Trim()
        };

        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Exam created successfully.", examId = exam.ExamId });
    }

    [HttpPut("exams/{id:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> UpdateExam(int id, [FromBody] CreateExamRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound(new { message = "Exam not found." });
        exam.Course = request.Course.Trim();
        exam.Semester = request.Semester;
        exam.ExamDate = request.ExamDate;
        exam.TimeSlot = request.TimeSlot.Trim();
        await _db.SaveChangesAsync();
        return Ok(new { message = "Exam updated successfully." });
    }

    [HttpDelete("exams/{id:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> DeleteExam(int id)
    {
        var exam = await _db.Exams.Include(e => e.SeatAllocations).FirstOrDefaultAsync(e => e.ExamId == id);
        if (exam == null) return NotFound(new { message = "Exam not found." });
        if (exam.SeatAllocations.Count > 0) return BadRequest(new { message = "Cannot delete an exam with seat allocations." });
        _db.Exams.Remove(exam);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Exam deleted successfully." });
    }

    [HttpGet("allocations")]
    [Authorize(Roles = "Admin,Exam Coordinator,Librarian")]
    public async Task<IActionResult> GetAllocations()
    {
        var allocations = await _db.SeatAllocations
            .Include(sa => sa.Exam)
            .Include(sa => sa.Room)
            .Include(sa => sa.Student)
            .OrderBy(sa => sa.ExamId)
            .ThenBy(sa => sa.BenchNo)
            .ThenBy(sa => sa.SeatNo)
            .Select(sa => new
            {
                seatId = sa.SeatId,
                examId = sa.ExamId,
                course = sa.Exam.Course,
                roomNo = sa.Room.RoomNo,
                studentId = sa.StudentId,
                studentName = sa.Student.Name,
                rollNo = sa.Student.RollNo,
                benchNo = sa.BenchNo,
                seatNo = sa.SeatNo,
                invigilatorId = sa.InvigilatorId
            })
            .ToListAsync();

        return Ok(allocations);
    }

    [HttpGet("seat-charts")]
    [Authorize(Roles = "Admin,Exam Coordinator,Librarian")]
    public async Task<IActionResult> GetSeatCharts([FromQuery] int? examId = null)
    {
        var rooms = await _db.Rooms
            .Include(r => r.SeatAllocations)
            .ThenInclude(sa => sa.Student)
            .OrderBy(r => r.RoomNo)
            .ToListAsync();

        var filteredRooms = rooms
            .Select(r => new
            {
                roomId = r.RoomId,
                roomNo = r.RoomNo,
                capacity = r.Capacity,
                benchLayout = r.BenchLayout,
                students = r.SeatAllocations
                    .Where(sa => !examId.HasValue || sa.ExamId == examId.Value)
                    .OrderBy(sa => sa.BenchNo)
                    .ThenBy(sa => sa.SeatNo)
                    .Select(sa => new
                    {
                        studentId = sa.StudentId,
                        studentName = sa.Student.Name,
                        rollNo = sa.Student.RollNo,
                        benchNo = sa.BenchNo,
                        seatNo = sa.SeatNo
                    })
                    .ToList()
            })
            .ToList();

        return Ok(filteredRooms);
    }

    [HttpGet("my-allocations")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyAllocations()
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

        var allocations = await _db.SeatAllocations
            .Include(sa => sa.Exam)
            .Include(sa => sa.Room)
            .Where(sa => sa.StudentId == student.StudentId)
            .OrderBy(sa => sa.Exam.ExamDate)
            .Select(sa => new
            {
                seatId = sa.SeatId,
                examId = sa.ExamId,
                course = sa.Exam.Course,
                semester = sa.Exam.Semester,
                examDate = sa.Exam.ExamDate,
                timeSlot = sa.Exam.TimeSlot,
                roomNo = sa.Room.RoomNo,
                benchNo = sa.BenchNo,
                seatNo = sa.SeatNo
            })
            .ToListAsync();

        return Ok(allocations);
    }

    [HttpGet("seat-charts/{examId:int}/pdf")]
    [Authorize(Roles = "Admin,Exam Coordinator,Librarian")]
    public async Task<IActionResult> ExportSeatChartPdf(int examId)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.ExamId == examId);
        if (exam == null)
        {
            return NotFound(new { message = "Exam not found." });
        }

        var rooms = await _db.Rooms
            .Include(r => r.SeatAllocations)
            .ThenInclude(sa => sa.Student)
            .Where(r => r.SeatAllocations.Any(sa => sa.ExamId == examId))
            .OrderBy(r => r.RoomNo)
            .ToListAsync();

        if (rooms.Count == 0)
        {
            return NotFound(new { message = "No seat chart available for this exam." });
        }

        var pdfBytes = rooms.SelectMany(room => room.SeatAllocations
            .Where(sa => sa.ExamId == examId)
            .OrderBy(sa => sa.BenchNo)
            .ThenBy(sa => sa.SeatNo)
            .Select(sa => new
            {
                studentName = sa.Student.Name,
                benchNo = sa.BenchNo,
                seatNo = sa.SeatNo
            }))
            .ToList();

        var roomName = string.Join(", ", rooms.Select(r => r.RoomNo));
        var bytes = PdfExportService.GenerateSeatChartPdf(roomName, pdfBytes);
        return File(bytes, "application/pdf", $"seat-chart-{examId}.pdf");
    }

    [HttpPut("seat-allocations/{seatId:int}")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> OverrideSeatAssignment(int seatId, [FromBody] OverrideSeatRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var allocation = await _db.SeatAllocations
            .Include(sa => sa.Student)
            .Include(sa => sa.Room)
            .FirstOrDefaultAsync(sa => sa.SeatId == seatId);

        if (allocation == null)
        {
            return NotFound(new { message = "Seat allocation not found." });
        }

        var roomExists = await _db.Rooms.AnyAsync(r => r.RoomId == request.RoomId);
        if (!roomExists)
        {
            return BadRequest(new { message = "Selected room does not exist." });
        }

        allocation.RoomId = request.RoomId;
        allocation.BenchNo = request.BenchNo;
        allocation.SeatNo = request.SeatNo;
        allocation.InvigilatorId = request.InvigilatorId;

        await _db.AuditLogs.AddAsync(new AuditLog
        {
            Action = "OverrideSeatAssignment",
            EntityType = "SeatAllocation",
            EntityId = allocation.SeatId,
            PerformedBy = User.Identity?.Name ?? "system",
            Details = $"Updated allocation for {allocation.Student.Name} in {allocation.Room.RoomNo} to bench {request.BenchNo}, seat {request.SeatNo}."
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Seat assignment overridden successfully." });
    }

    [HttpPost("allocate")]
    [Authorize(Roles = "Admin,Exam Coordinator")]
    public async Task<IActionResult> AllocateSeats([FromBody] AllocateSeatsRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var exam = await _db.Exams.FirstOrDefaultAsync(e => e.ExamId == request.ExamId);
        if (exam == null)
        {
            return NotFound(new { message = "Exam not found." });
        }

        var roomIds = request.RoomIds?.Where(id => id > 0).Distinct().ToList();
        if ((roomIds == null || roomIds.Count == 0) && request.RoomId > 0)
        {
            roomIds = new List<int> { request.RoomId };
        }

        if (roomIds == null || roomIds.Count == 0)
        {
            return BadRequest(new { message = "A room selection is required for seat allocation." });
        }

        var rooms = await _db.Rooms
            .Where(r => roomIds.Contains(r.RoomId))
            .OrderBy(r => r.RoomNo)
            .ToListAsync();

        if (rooms.Count == 0)
        {
            return NotFound(new { message = "One or more rooms were not found." });
        }

        var studentQuery = _db.Students.AsQueryable();

        if (request.StudentIds is { Count: > 0 })
        {
            studentQuery = studentQuery.Where(s => request.StudentIds.Contains(s.StudentId));
        }
        else if (!string.IsNullOrWhiteSpace(request.Dept))
        {
            studentQuery = studentQuery.Where(s => s.Dept == request.Dept);
        }
        else
        {
            studentQuery = studentQuery.Where(s => s.Semester == request.Semester || request.Semester <= 0);
        }

        var students = await studentQuery
            .OrderBy(s => s.RollNo)
            .Select(s => s.StudentId)
            .ToListAsync();

        if (students.Count == 0)
        {
            return BadRequest(new { message = "No eligible students found for allocation." });
        }

        var totalCapacity = rooms.Sum(r => r.Capacity);
        var selectedIds = students.Take(totalCapacity).ToList();

        var existing = _db.SeatAllocations.Where(sa => sa.ExamId == request.ExamId);
        _db.SeatAllocations.RemoveRange(existing);

        var roomAssignments = new List<(int RoomId, List<int> Students)>();
        var roomIndex = 0;
        foreach (var studentId in selectedIds)
        {
            if (roomAssignments.Count <= roomIndex || roomAssignments[roomIndex].RoomId != rooms[roomIndex % rooms.Count].RoomId)
            {
                var roomId = rooms[roomIndex % rooms.Count].RoomId;
                roomAssignments.Add((roomId, new List<int>()));
            }

            var assignmentIndex = roomAssignments.Count - 1;
            roomAssignments[assignmentIndex].Students.Add(studentId);
            roomIndex = (roomIndex + 1) % rooms.Count;
        }

        var allocations = new List<SeatAllocation>();
        foreach (var assignment in roomAssignments)
        {
            if (assignment.Students.Count == 0)
            {
                continue;
            }

            var room = rooms.First(r => r.RoomId == assignment.RoomId);
            var seatPlan = LibraryRules.GenerateSeatPlan(assignment.Students.Count, 10);

            for (var i = 0; i < assignment.Students.Count; i++)
            {
                var seat = seatPlan[i];
                allocations.Add(new SeatAllocation
                {
                    ExamId = request.ExamId,
                    RoomId = room.RoomId,
                    StudentId = assignment.Students[i],
                    BenchNo = seat.BenchNo,
                    SeatNo = seat.SeatNo,
                    InvigilatorId = request.InvigilatorId
                });
            }
        }

        _db.SeatAllocations.AddRange(allocations);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Seat allocation generated successfully.",
            allocatedCount = allocations.Count,
            rooms = rooms.Select(r => r.RoomNo).ToList(),
            examId = request.ExamId
        });
    }
}

public class CreateRoomRequest
{
    [Required, MinLength(2)]
    public string RoomNo { get; set; } = string.Empty;

    [Range(1, 500)]
    public int Capacity { get; set; } = 30;

    [Required, MinLength(2)]
    public string BenchLayout { get; set; } = "10-per-bench";
}

public class CreateInvigilatorRequest
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Dept { get; set; } = string.Empty;
}

public class CreateExamRequest
{
    [Required, MinLength(2)]
    public string Course { get; set; } = string.Empty;

    [Range(1, 12)]
    public int Semester { get; set; } = 1;

    public DateTime ExamDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Required, MinLength(2)]
    public string TimeSlot { get; set; } = "09:00 AM - 11:00 AM";
}

public class AllocateSeatsRequest
{
    [Required]
    public int ExamId { get; set; }

    public int RoomId { get; set; }

    public List<int>? RoomIds { get; set; }

    public int Semester { get; set; }

    public string? Dept { get; set; }

    public List<int>? StudentIds { get; set; }

    public int? InvigilatorId { get; set; }
}

public class OverrideSeatRequest
{
    [Required]
    public int RoomId { get; set; }

    [Range(1, 500)]
    public int BenchNo { get; set; }

    [Range(1, 500)]
    public int SeatNo { get; set; }

    public int? InvigilatorId { get; set; }
}

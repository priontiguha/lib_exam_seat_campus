using LibraryExamAPI.Data;
using LibraryExamAPI.Models;
using LibraryExamAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryExamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevSeedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DevSeedController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("testdata")]
    public async Task<IActionResult> SeedTestData()
    {
        if (!_env.IsDevelopment())
        {
            return Forbid();
        }

        // Clear existing test allocations and test data (non-destructive to other tables)
        var existingAllocations = _db.SeatAllocations.ToList();
        if (existingAllocations.Any())
        {
            _db.SeatAllocations.RemoveRange(existingAllocations);
        }

        // Create rooms
        var rooms = new List<Room>
        {
            new Room { RoomNo = "R101", Capacity = 30, BenchLayout = "10-per-bench" },
            new Room { RoomNo = "R102", Capacity = 30, BenchLayout = "10-per-bench" },
            new Room { RoomNo = "R103", Capacity = 20, BenchLayout = "10-per-bench" }
        };

        // Remove existing rooms with same roomNo to keep idempotent
        var existingRooms = _db.Rooms.Where(r => rooms.Select(x => x.RoomNo).Contains(r.RoomNo)).ToList();
        if (existingRooms.Any()) _db.Rooms.RemoveRange(existingRooms);

        await _db.Rooms.AddRangeAsync(rooms);

        // Create invigilators
        var invs = new List<Invigilator>
        {
            new Invigilator { Name = "Dr. Alice", Dept = "CS" },
            new Invigilator { Name = "Mr. Bob", Dept = "Math" },
            new Invigilator { Name = "Ms. Carol", Dept = "Physics" }
        };

        var existingInvs = _db.Invigilators.Where(i => invs.Select(x => x.Name).Contains(i.Name)).ToList();
        if (existingInvs.Any()) _db.Invigilators.RemoveRange(existingInvs);
        await _db.Invigilators.AddRangeAsync(invs);

        // Create students (deterministic roll numbers across two sections)
        var students = new List<Student>();
        for (var i = 1; i <= 80; i++)
        {
            var section = (i % 2 == 0) ? "A" : "B";
            var roll = $"2026{section}{i:D3}";
            students.Add(new Student
            {
                Name = $"Student {i}",
                RollNo = roll,
                Dept = "CS",
                Semester = 1,
                Contact = $"student{i}@example.com",
                PasswordHash = string.Empty,
                Role = "Student",
                IsVerified = true
            });
        }

        // Remove any prior seeded students for idempotence (by contact)
        var contacts = students.Select(s => s.Contact).ToList();
        var existingStudents = _db.Students.Where(s => contacts.Contains(s.Contact)).ToList();
        if (existingStudents.Any()) _db.Students.RemoveRange(existingStudents);
        await _db.Students.AddRangeAsync(students);

        // Create an exam
        var exam = new Exam
        {
            Course = "Math 101",
            Semester = 1,
            ExamDate = DateTime.UtcNow.AddDays(7),
            TimeSlot = "09:00 AM - 11:00 AM"
        };

        // Remove any similar exam to keep idempotent
        var similar = _db.Exams.FirstOrDefault(e => e.Course == exam.Course && e.ExamDate.Date == exam.ExamDate.Date);
        if (similar != null) _db.Exams.Remove(similar);
        await _db.Exams.AddAsync(exam);

        await _db.SaveChangesAsync();

        // Reload rooms and students with ids
        rooms = await _db.Rooms.OrderBy(r => r.RoomNo).ToListAsync();
        var allStudents = await _db.Students.Where(s => s.Semester == 1).OrderBy(s => s.RollNo).ToListAsync();

        var totalCapacity = rooms.Sum(r => r.Capacity);
        var selectedStudents = allStudents.Take(totalCapacity).ToList();

        // Distribute students round-robin across rooms then apply seat plan per room
        var roomAssignments = new Dictionary<int, List<int>>();
        var roomCount = rooms.Count;
        for (var idx = 0; idx < selectedStudents.Count; idx++)
        {
            var roomIndex = idx % roomCount;
            var roomId = rooms[roomIndex].RoomId;
            if (!roomAssignments.ContainsKey(roomId)) roomAssignments[roomId] = new List<int>();
            roomAssignments[roomId].Add(selectedStudents[idx].StudentId);
        }

        var allocations = new List<SeatAllocation>();
        foreach (var kv in roomAssignments)
        {
            var room = rooms.First(r => r.RoomId == kv.Key);
            // parse seatsPerBench from benchLayout e.g. "10-per-bench"
            var seatsPerBench = 10;
            var parts = room.BenchLayout?.Split('-');
            if (parts != null && parts.Length > 0 && int.TryParse(parts[0], out var spb)) seatsPerBench = spb;

            var seatPlan = LibraryRules.GenerateSeatPlan(kv.Value.Count, seatsPerBench);
            for (var i = 0; i < kv.Value.Count; i++)
            {
                var seat = seatPlan[i];
                allocations.Add(new SeatAllocation
                {
                    ExamId = exam.ExamId,
                    RoomId = room.RoomId,
                    StudentId = kv.Value[i],
                    BenchNo = seat.BenchNo,
                    SeatNo = seat.SeatNo,
                    InvigilatorId = invs.FirstOrDefault()?.StaffId
                });
            }
        }

        if (allocations.Any())
        {
            await _db.SeatAllocations.AddRangeAsync(allocations);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            rooms = rooms.Count,
            studentsSeeded = students.Count,
            examId = exam.ExamId,
            allocations = allocations.Count
        });
    }
}

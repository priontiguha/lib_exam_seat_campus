namespace LibraryExamAPI.Services;

public static class LibraryRules
{
    public static decimal CalculateFine(DateTime returnDate, DateTime dueDate, decimal perDayFine)
    {
        if (returnDate <= dueDate)
        {
            return 0m;
        }

        var overdueDays = (returnDate.Date - dueDate.Date).Days;
        return overdueDays * perDayFine;
    }

    public static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString("D6");
    }

    public static List<SeatPosition> GenerateSeatPlan(int totalStudents, int seatsPerBench)
    {
        if (seatsPerBench <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seatsPerBench), "Seats per bench must be greater than zero.");
        }

        if (totalStudents <= 0)
        {
            return new List<SeatPosition>();
        }

        var seats = new List<SeatPosition>();
        var benchCount = (int)Math.Ceiling(totalStudents / (double)seatsPerBench);

        // Improved allocation: interleave students across benches to reduce adjacent seating
        // For bench 1..benchCount, fill seat positions 1..seatsPerBench by distributing student indices
        for (var seatNo = 1; seatNo <= seatsPerBench; seatNo++)
        {
            for (var benchNo = 1; benchNo <= benchCount; benchNo++)
            {
                var currentIndex = (seatNo - 1) * benchCount + (benchNo - 1);
                if (currentIndex >= totalStudents) break;
                seats.Add(new SeatPosition { BenchNo = benchNo, SeatNo = seatNo });
            }
        }

        return seats.Take(totalStudents).ToList();
    }
}

public class SeatPosition
{
    public int BenchNo { get; set; }
    public int SeatNo { get; set; }
}

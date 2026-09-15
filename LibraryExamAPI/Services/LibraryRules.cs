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

        for (var index = 0; index < totalStudents; index++)
        {
            var benchNo = (index % benchCount) + 1;
            var seatNo = (index / benchCount) + 1;

            if (seatNo > seatsPerBench)
            {
                break;
            }

            seats.Add(new SeatPosition { BenchNo = benchNo, SeatNo = seatNo });
        }

        return seats;
    }
}

public class SeatPosition
{
    public int BenchNo { get; set; }
    public int SeatNo { get; set; }
}

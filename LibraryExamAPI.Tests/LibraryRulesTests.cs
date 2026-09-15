using LibraryExamAPI.Services;

namespace LibraryExamAPI.Tests;

public class LibraryRulesTests
{
    [Fact]
    public void CalculateFine_ShouldReturnZero_WhenReturnedBeforeDueDate()
    {
        var fine = LibraryRules.CalculateFine(new DateTime(2026, 1, 10), new DateTime(2026, 1, 15), 25m);

        Assert.Equal(0m, fine);
    }

    [Fact]
    public void CalculateFine_ShouldReturnOverdueFine_WhenReturnedAfterDueDate()
    {
        var fine = LibraryRules.CalculateFine(new DateTime(2026, 1, 18), new DateTime(2026, 1, 15), 25m);

        Assert.Equal(75m, fine);
    }

    [Fact]
    public void GenerateSeatPlan_ShouldThrow_WhenSeatsPerBenchIsZeroOrNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryRules.GenerateSeatPlan(20, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryRules.GenerateSeatPlan(20, -5));
    }

    [Fact]
    public void GenerateSeatPlan_ShouldReturnEmpty_WhenStudentCountIsZero()
    {
        var seats = LibraryRules.GenerateSeatPlan(0, 10);

        Assert.Empty(seats);
    }

    [Fact]
    public void GenerateSeatPlan_ShouldInterleaveAcrossBenches()
    {
        var seats = LibraryRules.GenerateSeatPlan(23, 10);

        Assert.Equal(23, seats.Count);
        Assert.Equal(3, seats.GroupBy(s => s.BenchNo).Count());
        Assert.Equal(1, seats[0].BenchNo);
        Assert.Equal(1, seats[0].SeatNo);
        Assert.Equal(2, seats[1].BenchNo);
        Assert.Equal(1, seats[1].SeatNo);
        Assert.Equal(3, seats[2].BenchNo);
        Assert.Equal(1, seats[2].SeatNo);
        Assert.All(seats, seat => Assert.InRange(seat.SeatNo, 1, 10));
    }

    [Fact]
    public void GenerateOtpCode_ShouldReturnSixDigitCode()
    {
        var code = LibraryRules.GenerateOtpCode();

        Assert.Equal(6, code.Length);
        Assert.True(int.TryParse(code, out _));
    }
}

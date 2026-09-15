using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibraryExamAPI.Services;

public static class PdfExportService
{
    public static byte[] GenerateSeatChartPdf(string roomNo, IEnumerable<object> seats)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var seatList = seats.ToList();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Element(header => header.Text($"Seat Chart - {roomNo}").SemiBold().FontSize(20));
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(cell => cell.Text("Student").SemiBold());
                        header.Cell().Element(cell => cell.Text("Bench").SemiBold());
                        header.Cell().Element(cell => cell.Text("Seat").SemiBold());
                    });

                    foreach (var seat in seatList)
                    {
                        var studentName = seat.GetType().GetProperty("studentName")?.GetValue(seat)?.ToString() ?? "Unassigned";
                        var benchNo = seat.GetType().GetProperty("benchNo")?.GetValue(seat)?.ToString() ?? "0";
                        var seatNo = seat.GetType().GetProperty("seatNo")?.GetValue(seat)?.ToString() ?? "0";

                        table.Cell().Element(cell => cell.Text(studentName));
                        table.Cell().Element(cell => cell.Text(benchNo));
                        table.Cell().Element(cell => cell.Text(seatNo));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
}

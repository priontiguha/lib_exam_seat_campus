using Microsoft.AspNetCore.SignalR;

namespace LibraryExamAPI.Hubs;

public class NotificationHub : Hub
{
    public async Task SendCommentUpdate(int bookId, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            type = "comment",
            bookId,
            message,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task SendReactionUpdate(int bookId, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            type = "reaction",
            bookId,
            message,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task SendSeatStatusUpdate(int examId, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            type = "seat-status",
            examId,
            message,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task SendDueDateAlert(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            type = "due-date",
            message,
            createdAt = DateTime.UtcNow
        });
    }
}

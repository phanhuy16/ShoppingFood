using Microsoft.AspNetCore.SignalR;

namespace ShoppingFood.Hubs
{
    public class SignalR : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Gửi tin nhắn từ người dùng đến tất cả client
            await Clients.All.SendAsync("ReceiveMessage", user, message);

            // Bot tự động phản hồi sau 1 giây
            await Task.Delay(1000);
            string botResponse = $"Xin chào {user}, bạn vừa nói '{message}'";
            await Clients.All.SendAsync("ReceiveMessage", "Bot", botResponse);
        }
    }
}

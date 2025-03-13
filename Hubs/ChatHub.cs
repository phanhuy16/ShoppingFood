using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using ShoppingFood.Models;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ShoppingFood.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<AppUserModel> _userManager;

        // Inject UserManager qua constructor
        public ChatHub(UserManager<AppUserModel> userManager)
        {
            _userManager = userManager;
        }
        private static ConcurrentDictionary<string, string> _userConnections = new();
        private static ConcurrentDictionary<string, string> _userIdsToUserNames = new();

        public override async Task OnConnectedAsync()
        {
            string userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value ?? Context.ConnectionId;

            _userConnections[userId] = Context.ConnectionId;

            await Clients.Caller.SendAsync("SetUserId", userId);
            await Clients.Group("Admin").SendAsync("UpdateUserList", _userIdsToUserNames.Values);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.User.FindFirst(ClaimTypes.Name).Value ?? Context.ConnectionId;

            _userConnections.TryRemove(userId, out _);

            // Cập nhật danh sách user online
            await Clients.Group("Admin").SendAsync("UpdateUserList", _userConnections.Keys);

            await base.OnDisconnectedAsync(exception);
        }

        // Gửi tin nhắn từ client đến admin
        public async Task SendMessageToAdmin(string userId, string message)
        {
            await Clients.Group("Admin").SendAsync("ReceiveMessage", userId, message);
        }

        // Gửi tin nhắn từ admin đến client
        public async Task SendMessageToClient(string userId, string message)
        {
            if (_userConnections.TryGetValue(userId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveAdminMessage", message);
            }
            else
            {
                Console.WriteLine($"User {userId} not found in connections.");
            }
        }

        // Thêm admin vào group
        public async Task AddToAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebProject.Data;
using WebProject.Models;
using Microsoft.AspNetCore.Identity;
using WebProject.Services;

namespace WebProject.Hubs
{
    public class MessageHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserPresenceService _presence;


        public MessageHub(AppDbContext context, UserManager<ApplicationUser> userManager, UserPresenceService presence)
        {
            _context = context;
            _userManager = userManager;
            _presence = presence;
        }

        public async Task SendMessage(string senderId, string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            // 1) Veritabanýna mesaj kaydet
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // 2) Alýcýya mesajý gönder (bildirim)
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, content);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _presence.SetOnline(userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if(!string.IsNullOrEmpty(userId))
            {
                _presence.SetOffline(userId);
                var user = await _userManager.FindByIdAsync(userId);
                if(user != null)
                {
                    user.LastSeen = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MarkMessagesAsRead(string senderId, string readerId)
        {
            var unread = _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == readerId && !m.IsRead)
                .ToList();

            if (unread.Any())
            {
                foreach (var m in unread)
                {
                    m.IsRead = true;
                }

                await _context.SaveChangesAsync();

                // Gönderen kullanýcýya "okundu" bildirimi gönder
                await Clients.Group(senderId).SendAsync("MessagesMarkedAsRead", readerId);
            }
        }

        public async Task Typing(string senderId, string receiverId)
        {
            await Clients.Group(receiverId).SendAsync("ShowTyping", senderId);
        }

    }
}

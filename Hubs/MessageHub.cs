using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebProject.Data;
using WebProject.Models;
using Microsoft.AspNetCore.Identity;

namespace WebProject.Hubs
{
    public class MessageHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageHub(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
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

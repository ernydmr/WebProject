using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;
using Microsoft.AspNetCore.SignalR;
using WebProject.Hubs;



namespace WebProject.Controllers;

[Authorize]
public class MessageController : Controller

{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<MessageHub> _hubContext;


    public MessageController(AppDbContext context, UserManager<ApplicationUser> userManager, IHubContext<MessageHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _hubContext = hubContext;
    }



    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        var latestByUser = messages
           .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
           .Select(g => g.OrderByDescending(m => m.SentAt).First())
           .ToList();

        return View(latestByUser);
    }


    [HttpGet]
    public async Task<IActionResult> Create(string receiverId)
    {
        var receiver = await _userManager.FindByIdAsync(receiverId);
        if (receiver == null) return NotFound();
        ViewBag.Receiver = receiver;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string receiverId, string content)
    {
        var sender = await _userManager.GetUserAsync(User);
        var receiver = await _userManager.FindByIdAsync(receiverId);
        if (receiver == null || sender == null || string.IsNullOrWhiteSpace(content)) return BadRequest();

        var message = new Message
        {
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            Content = content

        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Chat",new { userId = receiver.Id});
    }

    public async Task<IActionResult> Chat(string userId)
    {
        var currentUserId = _userManager.GetUserId(User);
        var otherUser = await _userManager.FindByIdAsync(userId);
        if(otherUser == null) return NotFound();

        var messages = await _context.Messages
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                        (m.SenderId == userId && m.ReceiverId == currentUserId))
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .OrderBy(m => m.SentAt)
            .ToListAsync();


        var unread = messages
            .Where(m => !m.IsRead && m.ReceiverId == currentUserId)
            .ToList();

        foreach (var message in unread)
        {
            message.IsRead = true;
        }
        if (unread.Any())
        {
            await _context.SaveChangesAsync();
        }


        await _hubContext.Clients.Group(otherUser.Id).SendAsync("MessagesMarkedAsRead", currentUserId);
        ViewBag.ChatWith = otherUser;
        return View(messages);

    }



}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebProject.Data;
using WebProject.Models;

namespace WebProject.Controllers;

[Authorize]
public class MessageController : Controller

{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessageController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create(string recipientId)
    {
        var recipient = await _userManager.FindByIdAsync(recipientId);
        if (recipient == null) return NotFound();
        Viewbag.Recipient = recipient;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string recipientId, string content)
    {
        var sender = await _userManager.GetUserAsync(User);
        var recipient = await _userManager.FindByIdAsync(recipientId);
        if (recipient == null || sender == null || string.IsNullOrWhiteSpace(content)) return BadRequest();

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = content

        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Inbox");
    }

    public async Task<IActionResult> Inbox()
    {

        var userId = await _userManager.GetUserIdAsync(User);

        var messages = await _context.Messages
            .Include (m => m.Sender)
            .Where (m => m.RecipientId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    public async Task<IActionResult> Sent()
    {
        var userId = await _userManager.GetUserIdAsync(User);
        var messages = await _context.Messages
            .Include(m => m.Recipient)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
        return View(messages);
    }




}
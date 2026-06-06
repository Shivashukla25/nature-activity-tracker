using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NatureActivityTracker.Models;
using NatureActivityTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NatureActivityTracker.Services;

namespace NatureActivityTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EcoIntelligenceService _ecoService;

    public HomeController(ApplicationDbContext context, EcoIntelligenceService ecoService)
    {
        _context = context;
        _ecoService = ecoService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("LogActivity");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // DELETE ACTIVITY
    public IActionResult Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var log = _context.ActivityLogs
            .FirstOrDefault(a => a.Id == id && a.UserId == userId);

        if (log != null)
        {
            _context.ActivityLogs.Remove(log);
            _context.SaveChanges();
        }

        return RedirectToAction("LogActivity");
    }

    // GET DASHBOARD
    public IActionResult LogActivity()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var logs = _context.ActivityLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Date)
            .ToList();

        CalculateDashboard(logs);

        return View(new ActivityModel());
    }

    // POST ACTIVITY
    [HttpPost]
    public async Task<IActionResult> LogActivity(ActivityModel model, IFormFile? ProofImage)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        string? imagePath = null;

        // IMAGE UPLOAD
        if (ProofImage != null && ProofImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProofImage.FileName);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProofImage.CopyToAsync(stream);
            }

            imagePath = "/uploads/" + uniqueFileName;
        }

        if (!string.IsNullOrEmpty(model.ActivityName) && model.Coins > 0)
        {
            var log = new ActivityLog
            {
                ActivityName = model.ActivityName,
                Coins = model.Coins,
                Date = DateTime.Now,
                UserId = userId,
                ProofImagePath = imagePath,
                IsVerified = false
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Activity Logged Successfully! 🌱 Coins: " + model.Coins;
        }

        var logs = _context.ActivityLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Date)
            .ToList();

        CalculateDashboard(logs);

        return View(new ActivityModel());
    }

    // DASHBOARD ANALYTICS
    private void CalculateDashboard(List<ActivityLog> logs)
    {
        ViewBag.TotalCoins = logs.Sum(a => a.Coins);
        ViewBag.ActivityLogs = logs;

        var today = DateTime.Today;

        ViewBag.TodayCoins = logs
            .Where(a => a.Date.Date == today)
            .Sum(a => a.Coins);

        ViewBag.MonthCoins = logs
            .Where(a => a.Date.Month == today.Month && a.Date.Year == today.Year)
            .Sum(a => a.Coins);

        ViewBag.TotalActivities = logs.Count;

        // CHART DATA
        var chartData = logs
            .GroupBy(a => a.ActivityName)
            .Select(g => new
            {
                Activity = g.Key,
                Coins = g.Sum(x => x.Coins)
            })
            .ToList();

        ViewBag.ChartLabels = chartData.Select(c => c.Activity).ToList();
        ViewBag.ChartValues = chartData.Select(c => c.Coins).ToList();

        // AI ENGINE
        int ecoScore = _ecoService.CalculateEcoScore(logs);
        string grade = _ecoService.GetGrade(ecoScore);
        string insight = _ecoService.GenerateInsight(logs);

        ViewBag.EcoScore = ecoScore;
        ViewBag.Grade = grade;
        ViewBag.AIInsight = insight;
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
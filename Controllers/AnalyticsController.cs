using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Требуется аутентификация для всех методов
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetUserAnalytics()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized("Пользователь не авторизован.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("Пользователь не найден.");

        var leases = await _context.LeaseContracts
            .Where(l => l.UserId == userId)
            .Include(l => l.Car)
            .ToListAsync();

        var payments = await _context.Payments
            .Where(p => leases.Select(l => l.Id).Contains(p.LeaseContractId))
            .ToListAsync();

        var totalLeases = leases.Count;
        var totalPayments = payments.Sum(p => p.Amount);
        var averageLeaseDuration = leases.Any()
            ? leases.Average(l => (l.LeaseEndDate - l.LeaseStartDate).TotalDays)
            : 0;
        var activeLeases = leases.Count(l => DateTime.Now < l.LeaseEndDate);

        var brandPreference = leases
            .GroupBy(l => l.Car.Brand)
            .Select(g => new { Brand = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        var analytics = new
        {
            TotalLeases = totalLeases,
            TotalPayments = totalPayments,
            AverageLeaseDurationDays = Math.Round(averageLeaseDuration, 2),
            ActiveLeases = activeLeases,
            BrandPreference = brandPreference
        };

        return Ok(analytics);
    }

    [HttpGet("payment-stats")]
    public async Task<ActionResult<object>> GetPaymentStats()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized("Пользователь не авторизован.");

        var payments = await _context.Payments
            .Where(p => _context.LeaseContracts
                .Where(l => l.UserId == userId)
                .Select(l => l.Id)
                .Contains(p.LeaseContractId))
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(p => p.Amount)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        var labels = payments.Select(p => $"{p.Year}-{p.Month:00}").ToList();
        var data = payments.Select(p => p.TotalAmount).ToList();

        return Ok(new
        {
            Labels = labels,
            Data = data
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return 0;
        }
        return userId;
    }
}
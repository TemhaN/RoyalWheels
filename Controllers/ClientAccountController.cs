using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.DTO;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientAccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientAccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ClientAccountDto>> GetClientAccount()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized("Пользователь не авторизован.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("Пользователь не найден.");

        var leases = await _context.LeaseContracts
            .Include(l => l.Car)
            .Where(l => l.UserId == userId)
            .Select(l => new LeaseContractDto
            {
                CarId = l.CarId,
                CarBrand = l.Car.Brand,
                CarModel = l.Car.Model,
                LeaseStartDate = l.LeaseStartDate,
                LeaseEndDate = l.LeaseEndDate,
                MonthlyPayment = CalculateMonthlyPayment(l.TotalCost, l.LeaseStartDate, l.LeaseEndDate),
                Status = DateTime.Now < l.LeaseEndDate ? "Active" : "Completed"
            })
            .ToListAsync();

        var clientAccount = new ClientAccountDto
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            LeaseContracts = leases
        };
        return Ok(clientAccount);
    }

    private decimal CalculateMonthlyPayment(decimal totalCost, DateTime startDate, DateTime endDate)
    {
        var months = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
        return months == 0 ? totalCost : totalCost / months; // Защита от деления на 0
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
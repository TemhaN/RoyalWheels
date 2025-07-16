using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized("Пользователь не авторизован.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound("Пользователь не найден.");

        var contracts = await _context.LeaseContracts
            .Include(c => c.Car)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var result = new
        {
            user.FullName,
            user.Email,
            user.PhoneNumber,
            Contracts = contracts.Select(c => new {
                CarId = c.Car.Id,
                CarBrand = c.Car.Brand,
                CarModel = c.Car.Model,
                CarPhotoUrl = c.Car.PhotoUrl,
                LeaseStartDate = c.LeaseStartDate,
                LeaseEndDate = c.LeaseEndDate,
                TotalCost = c.TotalCost,
                Status = DateTime.Now < c.LeaseEndDate ? "Active" : "Completed"
            })
        };

        return Ok(result);
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
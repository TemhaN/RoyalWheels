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
public class LeaseController : ControllerBase
{
    private readonly AppDbContext _context;

    public LeaseController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> CreateLease([FromBody] LeaseRequestDto leaseRequest)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("Пользователь не авторизован.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Пользователь не найден.");

            var car = await _context.Cars.FindAsync(leaseRequest.CarId);
            if (car == null) return NotFound("Автомобиль не найден.");

            if (car.Status != CarStatus.Reserved)
            {
                return BadRequest("Автомобиль должен быть забронирован для оформления лизинга.");
            }

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserId == userId &&
                                         r.CarId == leaseRequest.CarId &&
                                         r.IsActive);
            if (reservation == null) return BadRequest("Требуется активное бронирование.");

            var leaseContract = new LeaseContract
            {
                UserId = userId,
                CarId = leaseRequest.CarId,
                LeaseStartDate = leaseRequest.LeaseStartDate,
                LeaseEndDate = leaseRequest.LeaseEndDate,
                TotalCost = CalculateLeaseCost(car.Price, leaseRequest.LeaseStartDate, leaseRequest.LeaseEndDate)
            };

            reservation.IsActive = false;
            car.Status = CarStatus.Leased;
            _context.LeaseContracts.Add(leaseContract);
            await _context.SaveChangesAsync();

            var leaseContractDto = new LeaseContractDto
            {
                CarId = leaseContract.CarId,
                CarBrand = car.Brand,
                CarModel = car.Model,
                LeaseStartDate = leaseContract.LeaseStartDate,
                LeaseEndDate = leaseContract.LeaseEndDate,
                MonthlyPayment = CalculateMonthlyPayment(
                    leaseContract.TotalCost,
                    leaseContract.LeaseStartDate,
                    leaseContract.LeaseEndDate),
                Status = DateTime.UtcNow < leaseContract.LeaseEndDate ? "Active" : "Completed"
            };

            return Ok(new { id = leaseContract.Id, leaseContract = leaseContractDto });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating lease: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaseContractDto>>> GetUserLeases()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized("Пользователь не авторизован.");

        var leases = await _context.LeaseContracts
            .Where(l => l.UserId == userId)
            .Include(l => l.Car)
            .Select(l => new LeaseContractDto
            {
                CarId = l.CarId,
                CarBrand = l.Car.Brand,
                CarModel = l.Car.Model,
                LeaseStartDate = l.LeaseStartDate,
                LeaseEndDate = l.LeaseEndDate,
                MonthlyPayment = CalculateMonthlyPayment(l.TotalCost, l.LeaseStartDate, l.LeaseEndDate),
                Status = DateTime.UtcNow < l.LeaseEndDate ? "Active" : "Completed"
            })
            .ToListAsync();

        return Ok(leases);
    }

    private decimal CalculateMonthlyPayment(decimal totalCost, DateTime startDate, DateTime endDate)
    {
        var months = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
        return months == 0 ? totalCost : totalCost / months;
    }
    private decimal CalculateLeaseCost(decimal carPrice, DateTime startDate, DateTime endDate)
    {
        var days = (endDate - startDate).TotalDays;
        var dailyRate = carPrice * 0.001m;
        return (decimal)days * dailyRate;
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
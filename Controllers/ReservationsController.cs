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
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] ReservationDto reservationRequest)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("Пользователь не авторизован.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("Пользователь не найден.");

            var car = await _context.Cars.FindAsync(reservationRequest.CarId);
            if (car == null) return NotFound("Автомобиль не найден.");

            if (car.Status != CarStatus.Available)
            {
                return BadRequest("Автомобиль недоступен для бронирования.");
            }

            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.CarId == reservationRequest.CarId && r.IsActive);
            if (existingReservation != null && existingReservation.UserId != userId)
            {
                return BadRequest("Автомобиль уже забронирован другим пользователем.");
            }

            if (existingReservation != null && existingReservation.UserId == userId)
            {
                existingReservation.IsActive = false;
                await _context.SaveChangesAsync();
            }

            var reservation = new Reservation
            {
                UserId = userId,
                CarId = reservationRequest.CarId,
                ReservationStart = reservationRequest.ReservationStart,
                ReservationEnd = reservationRequest.ReservationEnd,
                IsActive = true
            };

            car.Status = CarStatus.Reserved;
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var reservationDto = new ReservationDto
            {
                CarId = reservation.CarId,
                ReservationStart = reservation.ReservationStart,
                ReservationEnd = reservation.ReservationEnd
            };

            return Ok(reservationDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating reservation: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations([FromQuery] int? carId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("Пользователь не авторизован.");

            // Очистка истёкших бронирований
            var expiredReservations = await _context.Reservations
                .Where(r => r.IsActive && r.ReservationEnd <= DateTime.UtcNow)
                .ToListAsync();
            foreach (var reservation in expiredReservations)
            {
                reservation.IsActive = false;
                var car = await _context.Cars.FindAsync(reservation.CarId);
                if (car != null && car.Status == CarStatus.Reserved)
                {
                    car.Status = CarStatus.Available;
                }
            }
            await _context.SaveChangesAsync();

            var query = _context.Reservations
                .Where(r => r.IsActive && r.ReservationEnd > DateTime.UtcNow);

            if (carId.HasValue)
            {
                query = query.Where(r => r.CarId == carId.Value);
            }

            var reservations = await query
                .Select(r => new ReservationDto
                {
                    CarId = r.CarId,
                    ReservationStart = r.ReservationStart,
                    ReservationEnd = r.ReservationEnd,
                    UserId = r.UserId
                })
                .ToListAsync();

            return Ok(reservations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting reservations: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
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
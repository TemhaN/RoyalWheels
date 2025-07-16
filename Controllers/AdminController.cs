using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTO;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // --- Управление пользователями ---

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Пользователь не найден." });
        return user;
    }

    [HttpPost("users")]
    public async Task<ActionResult> CreateUser([FromBody] RegisterDto registerDto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null) return BadRequest(new { message = "Пользователь с таким email уже существует." });

        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            PasswordHash = BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User // По умолчанию User, админ может изменить через Update
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Пользователь создан.", userId = user.Id });
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
    {
        if (id != updateDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Пользователь не найден." });

        user.FullName = updateDto.FullName;
        user.Email = updateDto.Email;
        user.PhoneNumber = updateDto.PhoneNumber;
        if (!string.IsNullOrEmpty(updateDto.Password))
        {
            user.PasswordHash = BCrypt.HashPassword(updateDto.Password);
        }
        if (Enum.TryParse<UserRole>(updateDto.Role, out var role))
        {
            user.Role = role;
        }
        else
        {
            return BadRequest(new { message = "Недопустимая роль." });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Пользователь обновлён." });
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Пользователь не найден." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Пользователь удалён." });
    }

    // --- Управление автомобилями ---

    [HttpGet("cars")]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetCars()
    {
        var cars = await _context.Cars
            .GroupJoin(
                _context.Reviews,
                car => car.Id,
                review => review.CarId,
                (car, reviews) => new { car, reviews }
            )
            .Select(
                g => new CarDto
                {
                    Id = g.car.Id,
                    Brand = g.car.Brand,
                    Model = g.car.Model,
                    Year = g.car.Year,
                    Engine = g.car.Engine,
                    BodyType = g.car.BodyType,
                    Price = g.car.Price,
                    PhotoUrl = g.car.PhotoUrl,
                    Status = g.car.Status.ToString(),
                    AverageRating = g.reviews.Any() ? g.reviews.Average(r => r.Rating) : 0
                }
            )
            .ToListAsync();
        return Ok(cars);
    }

    [HttpGet("cars/{id}")]
    public async Task<ActionResult<Car>> GetCar(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound(new { message = "Автомобиль не найден." });
        return car;
    }

    [HttpPost("cars")]
    public async Task<ActionResult> CreateCar([FromBody] CreateCarDto createCarDto)
    {
        var car = new Car
        {
            Brand = createCarDto.Brand,
            Model = createCarDto.Model,
            Year = createCarDto.Year,
            Engine = createCarDto.Engine,
            BodyType = createCarDto.BodyType,
            Price = createCarDto.Price,
            PhotoUrl = createCarDto.PhotoUrl,
            Status = CarStatus.Available
        };

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Автомобиль создан.", carId = car.Id });
    }

    [HttpPut("cars/{id}")]
    public async Task<ActionResult> UpdateCar(int id, [FromBody] UpdateCarDto updateCarDto)
    {
        if (id != updateCarDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound(new { message = "Автомобиль не найден." });

        car.Brand = updateCarDto.Brand;
        car.Model = updateCarDto.Model;
        car.Year = updateCarDto.Year;
        car.Engine = updateCarDto.Engine;
        car.BodyType = updateCarDto.BodyType;
        car.Price = updateCarDto.Price;
        car.PhotoUrl = updateCarDto.PhotoUrl;
        if (Enum.TryParse<CarStatus>(updateCarDto.Status, out var status))
        {
            car.Status = status;
        }
        else
        {
            return BadRequest(new { message = "Недопустимый статус." });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Автомобиль обновлён." });
    }

    [HttpDelete("cars/{id}")]
    public async Task<ActionResult> DeleteCar(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound(new { message = "Автомобиль не найден." });

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Автомобиль удалён." });
    }

    // --- Управление договорами лизинга ---

    [HttpGet("lease-contracts")]
    public async Task<ActionResult<IEnumerable<LeaseContract>>> GetLeaseContracts()
    {
        return await _context.LeaseContracts.Include(lc => lc.User).Include(lc => lc.Car).ToListAsync();
    }

    [HttpGet("lease-contracts/{id}")]
    public async Task<ActionResult<LeaseContract>> GetLeaseContract(int id)
    {
        var leaseContract = await _context.LeaseContracts
            .Include(lc => lc.User)
            .Include(lc => lc.Car)
            .FirstOrDefaultAsync(lc => lc.Id == id);
        if (leaseContract == null) return NotFound(new { message = "Договор лизинга не найден." });
        return leaseContract;
    }

    [HttpPost("lease-contracts")]
    public async Task<ActionResult> CreateLeaseContract([FromBody] UpdateLeaseContractDto leaseContractDto)
    {
        var leaseContract = new LeaseContract
        {
            UserId = leaseContractDto.UserId,
            CarId = leaseContractDto.CarId,
            LeaseStartDate = leaseContractDto.LeaseStartDate,
            LeaseEndDate = leaseContractDto.LeaseEndDate,
            TotalCost = leaseContractDto.TotalCost
        };

        _context.LeaseContracts.Add(leaseContract);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Договор лизинга создан.", leaseContractId = leaseContract.Id });
    }

    [HttpPut("lease-contracts/{id}")]
    public async Task<ActionResult> UpdateLeaseContract(int id, [FromBody] UpdateLeaseContractDto leaseContractDto)
    {
        if (id != leaseContractDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var leaseContract = await _context.LeaseContracts.FindAsync(id);
        if (leaseContract == null) return NotFound(new { message = "Договор лизинга не найден." });

        leaseContract.UserId = leaseContractDto.UserId;
        leaseContract.CarId = leaseContractDto.CarId;
        leaseContract.LeaseStartDate = leaseContractDto.LeaseStartDate;
        leaseContract.LeaseEndDate = leaseContractDto.LeaseEndDate;
        leaseContract.TotalCost = leaseContractDto.TotalCost;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Договор лизинга обновлён." });
    }

    [HttpDelete("lease-contracts/{id}")]
    public async Task<ActionResult> DeleteLeaseContract(int id)
    {
        var leaseContract = await _context.LeaseContracts.FindAsync(id);
        if (leaseContract == null) return NotFound(new { message = "Договор лизинга не найден." });

        _context.LeaseContracts.Remove(leaseContract);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Договор лизинга удалён." });
    }

    // --- Управление платежами ---

    [HttpGet("payments")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
    {
        return await _context.Payments.Include(p => p.LeaseContract).ToListAsync();
    }

    [HttpGet("payments/{id}")]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.LeaseContract)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return NotFound(new { message = "Платёж не найден." });
        return payment;
    }

    [HttpPost("payments")]
    public async Task<ActionResult> CreatePayment([FromBody] UpdatePaymentDto paymentDto)
    {
        var payment = new Payment
        {
            LeaseContractId = paymentDto.LeaseContractId,
            PaymentDate = paymentDto.PaymentDate,
            Amount = paymentDto.Amount,
            IsPaid = paymentDto.IsPaid
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Платёж создан.", paymentId = payment.Id });
    }

    [HttpPut("payments/{id}")]
    public async Task<ActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto paymentDto)
    {
        if (id != paymentDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound(new { message = "Платёж не найден." });

        payment.LeaseContractId = paymentDto.LeaseContractId;
        payment.PaymentDate = paymentDto.PaymentDate;
        payment.Amount = paymentDto.Amount;
        payment.IsPaid = paymentDto.IsPaid;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Платёж обновлён." });
    }

    [HttpDelete("payments/{id}")]
    public async Task<ActionResult> DeletePayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return NotFound(new { message = "Платёж не найден." });

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Платёж удалён." });
    }

    // --- Управление бронированиями ---

    [HttpGet("reservations")]
    public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Car)
            .ToListAsync();
    }

    [HttpGet("reservations/{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return NotFound(new { message = "Бронирование не найдено." });
        return reservation;
    }

    [HttpPost("reservations")]
    public async Task<ActionResult> CreateReservation([FromBody] UpdateReservationDto reservationDto)
    {
        var reservation = new Reservation
        {
            UserId = reservationDto.UserId,
            CarId = reservationDto.CarId,
            ReservationStart = reservationDto.ReservationStart,
            ReservationEnd = reservationDto.ReservationEnd,
            IsActive = reservationDto.IsActive
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Бронирование создано.", reservationId = reservation.Id });
    }

    [HttpPut("reservations/{id}")]
    public async Task<ActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto reservationDto)
    {
        if (id != reservationDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound(new { message = "Бронирование не найдено." });

        reservation.UserId = reservationDto.UserId;
        reservation.CarId = reservationDto.CarId;
        reservation.ReservationStart = reservationDto.ReservationStart;
        reservation.ReservationEnd = reservationDto.ReservationEnd;
        reservation.IsActive = reservationDto.IsActive;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Бронирование обновлено." });
    }

    [HttpDelete("reservations/{id}")]
    public async Task<ActionResult> DeleteReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound(new { message = "Бронирование не найдено." });

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Бронирование удалено." });
    }

    // --- Управление избранным ---

    [HttpGet("favorites")]
    public async Task<ActionResult<IEnumerable<Favorite>>> GetFavorites()
    {
        return await _context.Favorites
            .Include(f => f.User)
            .Include(f => f.Car)
            .ToListAsync();
    }

    [HttpGet("favorites/{id}")]
    public async Task<ActionResult<Favorite>> GetFavorite(int id)
    {
        var favorite = await _context.Favorites
            .Include(f => f.User)
            .Include(f => f.Car)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (favorite == null) return NotFound(new { message = "Избранное не найдено." });
        return favorite;
    }

    [HttpPost("favorites")]
    public async Task<ActionResult> CreateFavorite([FromBody] UpdateFavoriteDto favoriteDto)
    {
        var favorite = new Favorite
        {
            UserId = favoriteDto.UserId,
            CarId = favoriteDto.CarId
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Избранное создано.", favoriteId = favorite.Id });
    }

    [HttpPut("favorites/{id}")]
    public async Task<ActionResult> UpdateFavorite(int id, [FromBody] UpdateFavoriteDto favoriteDto)
    {
        if (id != favoriteDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var favorite = await _context.Favorites.FindAsync(id);
        if (favorite == null) return NotFound(new { message = "Избранное не найдено." });

        favorite.UserId = favoriteDto.UserId;
        favorite.CarId = favoriteDto.CarId;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Избранное обновлено." });
    }

    [HttpDelete("favorites/{id}")]
    public async Task<ActionResult> DeleteFavorite(int id)
    {
        var favorite = await _context.Favorites.FindAsync(id);
        if (favorite == null) return NotFound(new { message = "Избранное не найдено." });

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Избранное удалено." });
    }

    // --- Управление отзывами ---

    [HttpGet("reviews")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Car)
            .ToListAsync();
    }

    [HttpGet("reviews/{id}")]
    public async Task<ActionResult<Review>> GetReview(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (review == null) return NotFound(new { message = "Отзыв не найден." });
        return review;
    }

    [HttpPost("reviews")]
    public async Task<ActionResult> CreateReview([FromBody] UpdateReviewDto reviewDto)
    {
        var review = new Review
        {
            CarId = reviewDto.CarId,
            UserId = reviewDto.UserId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Отзыв создан.", reviewId = review.Id });
    }

    [HttpPut("reviews/{id}")]
    public async Task<ActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto reviewDto)
    {
        if (id != reviewDto.Id) return BadRequest(new { message = "ID не совпадает." });

        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Отзыв не найден." });

        review.CarId = reviewDto.CarId;
        review.UserId = reviewDto.UserId;
        review.Rating = reviewDto.Rating;
        review.Comment = reviewDto.Comment;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Отзыв обновлён." });
    }

    [HttpDelete("reviews/{id}")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound(new { message = "Отзыв не найден." });

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Отзыв удалён." });
    }
}
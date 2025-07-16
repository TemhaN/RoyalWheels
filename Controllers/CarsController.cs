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
public class CarsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
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

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CarDto>>> SearchCars(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minYear,
        [FromQuery] string? status)
    {
        var query = _context.Cars.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(
                car => car.Brand.ToLower().Contains(search) ||
                       car.Model.ToLower().Contains(search) ||
                       car.BodyType.ToLower().Contains(search));
        }
        if (minPrice.HasValue)
        {
            query = query.Where(car => car.Price >= minPrice);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(car => car.Price <= maxPrice);
        }
        if (minYear.HasValue)
        {
            query = query.Where(car => car.Year >= minYear);
        }
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CarStatus>(status, true, out var carStatus))
        {
            query = query.Where(car => car.Status == carStatus);
        }

        var cars = await query
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

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CarDto>> GetCarById(int id)
    {
        var car = await _context.Cars
            .Where(c => c.Id == id)
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
            .FirstOrDefaultAsync();
        if (car == null)
            return NotFound("Автомобиль не найден.");
        return Ok(car);
    }

    [HttpPost]
    public async Task<ActionResult> CreateCar([FromBody] CreateCarDto carDto)
    {
        if (string.IsNullOrEmpty(carDto.Brand) || string.IsNullOrEmpty(carDto.Model))
            return BadRequest("Марка и модель автомобиля обязательны.");

        var car = new Car
        {
            Brand = carDto.Brand,
            Model = carDto.Model,
            Year = carDto.Year,
            Engine = carDto.Engine,
            BodyType = carDto.BodyType,
            Price = carDto.Price,
            PhotoUrl = carDto.PhotoUrl,
            Status = CarStatus.Available
        };

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return Ok("Автомобиль успешно создан.");
    }

    [HttpPost("reserve")]
    public async Task<ActionResult> ReserveCar([FromBody] ReservationDto reservationDto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var car = await _context.Cars.FindAsync(reservationDto.CarId);
        if (car == null)
            return NotFound("Автомобиль не найден.");

        if (car.Status != CarStatus.Available)
            return BadRequest("Автомобиль недоступен для бронирования.");

        var existingReservation = await _context.Reservations
            .AnyAsync(r => r.CarId == reservationDto.CarId && r.IsActive &&
                           r.ReservationStart <= reservationDto.ReservationEnd &&
                           r.ReservationEnd >= reservationDto.ReservationStart);
        if (existingReservation)
            return BadRequest("Автомобиль уже забронирован на это время.");

        var reservation = new Reservation
        {
            UserId = userId,
            CarId = reservationDto.CarId,
            ReservationStart = reservationDto.ReservationStart,
            ReservationEnd = reservationDto.ReservationEnd,
            IsActive = true
        };

        car.Status = CarStatus.Reserved;
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return Ok("Автомобиль успешно забронирован.");
    }

    [HttpGet("reservations")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserReservations()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var reservations = await _context.Reservations
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.Car)
            .Select(r => new
            {
                r.Id,
                r.CarId,
                CarBrand = r.Car.Brand,
                CarModel = r.Car.Model,
                r.ReservationStart,
                r.ReservationEnd
            })
            .ToListAsync();
        return Ok(reservations);
    }

    [HttpPost("compare")]
    public async Task<ActionResult<IEnumerable<CarDto>>> CompareCars([FromBody] CompareCarsDto compareDto)
    {
        if (compareDto.CarIds == null || compareDto.CarIds.Count == 0)
            return BadRequest("Не выбраны автомобили для сравнения.");

        var cars = await _context.Cars
            .Where(c => compareDto.CarIds.Contains(c.Id))
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

        if (cars.Count != compareDto.CarIds.Count)
            return NotFound("Некоторые автомобили не найдены.");

        return Ok(cars);
    }

    [HttpPost("favorites/{carId}")]
    public async Task<ActionResult> AddToFavorites(int carId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("Пользователь не найден.");

        var car = await _context.Cars.FindAsync(carId);
        if (car == null)
            return NotFound("Автомобиль не найден.");

        var existingFavorite = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.CarId == carId);
        if (existingFavorite)
            return BadRequest("Автомобиль уже в избранном.");

        var favorite = new Favorite
        {
            UserId = userId,
            CarId = carId
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();
        return Ok("Автомобиль добавлен в избранное.");
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetFavorites()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Car)
            .GroupJoin(
                _context.Reviews,
                f => f.CarId,
                review => review.CarId,
                (f, reviews) => new { f, reviews }
            )
            .Select(
                g => new CarDto
                {
                    Id = g.f.Car.Id,
                    Brand = g.f.Car.Brand,
                    Model = g.f.Car.Model,
                    Year = g.f.Car.Year,
                    Engine = g.f.Car.Engine,
                    BodyType = g.f.Car.BodyType,
                    Price = g.f.Car.Price,
                    PhotoUrl = g.f.Car.PhotoUrl,
                    Status = g.f.Car.Status.ToString(),
                    AverageRating = g.reviews.Any() ? g.reviews.Average(r => r.Rating) : 0
                }
            )
            .ToListAsync();

        return Ok(favorites);
    }

    [HttpDelete("favorites/{carId}")]
    public async Task<ActionResult> RemoveFromFavorites(int carId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.CarId == carId);
        if (favorite == null)
            return NotFound("Автомобиль не в избранном.");

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return Ok("Автомобиль удалён из избранного.");
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetRecommendations()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var userLeases = await _context.LeaseContracts
            .Where(l => l.UserId == userId)
            .Include(l => l.Car)
            .ToListAsync();

        var userFavorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Car)
            .ToListAsync();

        var preferredBrands = userLeases.Select(l => l.Car.Brand)
            .Union(userFavorites.Select(f => f.Car.Brand))
            .Distinct()
            .ToList();
        var avgPrice = userLeases.Any() ? userLeases.Average(l => l.Car.Price) : 0;

        var recommendations = await _context.Cars
            .Where(c => preferredBrands.Contains(c.Brand) &&
                       (avgPrice == 0 || c.Price <= avgPrice * 1.2m))
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
            .Take(5)
            .ToListAsync();

        return Ok(recommendations);
    }

    [HttpPost("{id}/review")]
    public async Task<ActionResult> CreateReview(int id, [FromBody] CreateReviewDto reviewDto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized("Пользователь не авторизован.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("Пользователь не найден.");

        var car = await _context.Cars.FindAsync(id);
        if (car == null)
            return NotFound("Автомобиль не найден.");

        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            return BadRequest("Рейтинг должен быть от 1 до 5.");

        var existingReview = await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.CarId == id);
        if (existingReview)
            return BadRequest("Вы уже оставили отзыв для этого автомобиля.");

        var review = new Review
        {
            CarId = id,
            UserId = userId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return Ok("Отзыв успешно добавлен.");
    }

    [HttpGet("{id}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetCarReviews(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null)
            return NotFound("Автомобиль не найден.");

        var reviews = await _context.Reviews
            .Where(r => r.CarId == id)
            .Include(r => r.User)
            .Select(
                r => new ReviewDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    UserName = r.User.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }
            )
            .ToListAsync();

        return Ok(reviews);
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
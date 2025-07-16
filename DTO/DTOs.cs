namespace WebApplication1.DTO;

// DTO для автомобиля
public class CarDto
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Engine { get; set; }
    public string BodyType { get; set; }
    public decimal Price { get; set; }
    public string PhotoUrl { get; set; }
    public string Status { get; set; }
    public double AverageRating { get; set; }
}

// DTO для создания автомобиля
public class CreateCarDto
{
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Engine { get; set; }
    public string BodyType { get; set; }
    public decimal Price { get; set; }
    public string PhotoUrl { get; set; }
}

// DTO для личного кабинета клиента
public class ClientAccountDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public List<LeaseContractDto> LeaseContracts { get; set; }
}

// DTO для договора лизинга
public class LeaseContractDto
{
    public int CarId { get; set; }
    public string CarBrand { get; set; }
    public string CarModel { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; }
}

// DTO для заявки на лизинг
public class LeaseRequestDto
{
    public int UserId { get; set; }
    public int CarId { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }
}

// DTO для платежа
public class PaymentDto
{
    public int Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}

// DTO для логина пользователя
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// DTO для регистрации пользователя
public class RegisterDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
}

// DTO для бронирования
public class ReservationDto
{
    public int CarId { get; set; }
    public DateTime ReservationStart { get; set; }
    public DateTime ReservationEnd { get; set; }
    public int UserId { get; set; }
}

public class CompareCarsDto
{
    public List<int> CarIds { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public string UserName { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int CarId { get; set; }
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; }
}
public class AuthResponseDto
{
    public string Token { get; set; }
    public string Message { get; set; }
}

public class UpdateUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; } // Опционально
    public string Role { get; set; } // "User" или "Admin"
}

// DTO для обновления автомобиля
public class UpdateCarDto
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Engine { get; set; }
    public string BodyType { get; set; }
    public decimal Price { get; set; }
    public string PhotoUrl { get; set; }
    public string Status { get; set; } // "Available", "Reserved", "Leased"
}

// DTO для обновления договора лизинга
public class UpdateLeaseContractDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CarId { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }
    public decimal TotalCost { get; set; }
}

// DTO для обновления платежа
public class UpdatePaymentDto
{
    public int Id { get; set; }
    public int LeaseContractId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}

// DTO для обновления бронирования
public class UpdateReservationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CarId { get; set; }
    public DateTime ReservationStart { get; set; }
    public DateTime ReservationEnd { get; set; }
    public bool IsActive { get; set; }
}

// DTO для обновления избранного
public class UpdateFavoriteDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CarId { get; set; }
}

// DTO для обновления отзыва
public class UpdateReviewDto
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
}
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
}
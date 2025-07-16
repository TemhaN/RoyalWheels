namespace WebApplication1.Models;

public enum UserRole
{
    User, // Обычный пользователь
    Admin // Администратор
}

// Сущность для описания пользователей системы
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public enum CarStatus
{
    Available,
    Reserved,
    Leased
}
// Сущность для описания автомобилей
public class Car
{
    public int Id { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Engine { get; set; }
    public string BodyType { get; set; }
    public decimal Price { get; set; }
    public string PhotoUrl { get; set; }
    public CarStatus Status { get; set; } = CarStatus.Available;
}

// Сущность для описания договоров лизинга
public class LeaseContract
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; }
    public DateTime LeaseStartDate { get; set; }
    public DateTime LeaseEndDate { get; set; }
    public decimal TotalCost { get; set; }
}

// Сущность для описания платежей
public class Payment
{
    public int Id { get; set; }
    public int LeaseContractId { get; set; }
    public LeaseContract LeaseContract { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}

// Сущность для бронирования автомобилей
public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; }
    public DateTime ReservationStart { get; set; }
    public DateTime ReservationEnd { get; set; }
    public bool IsActive { get; set; }
}
public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; }
}
public class Review
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
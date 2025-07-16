using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1;

public class AppDbContext : DbContext
{
    public DbSet<Car> Cars { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<LeaseContract> LeaseContracts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Review> Reviews { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка автоинкремента для всех сущностей
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Car>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<LeaseContract>()
            .Property(lc => lc.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Payment>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Favorite>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Review>()
            .Property(r => r.Id)
            .ValueGeneratedOnAdd();

        // Связь между LeaseContract и User
        modelBuilder.Entity<LeaseContract>()
            .HasOne(lc => lc.User)
            .WithMany()
            .HasForeignKey(lc => lc.UserId);

        // Связь между LeaseContract и Car
        modelBuilder.Entity<LeaseContract>()
            .HasOne(lc => lc.Car)
            .WithMany()
            .HasForeignKey(lc => lc.CarId);

        // Связь между Payment и LeaseContract
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.LeaseContract)
            .WithMany()
            .HasForeignKey(p => p.LeaseContractId);

        // Связь между Reservation и User, Car
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Car)
            .WithMany()
            .HasForeignKey(r => r.CarId);

        // Связь между Favorite и User, Car
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Car)
            .WithMany()
            .HasForeignKey(f => f.CarId);

        // Связь между Review и User, Car
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Car)
            .WithMany()
            .HasForeignKey(r => r.CarId);

        // Указываем, что Role в User маппится как строка
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}
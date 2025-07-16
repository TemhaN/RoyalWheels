using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System;

namespace WebApplication1
{
    using BCrypt.Net;

    public static class DatabaseInitializer
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                // Проверяем наличие пользователей
                if (!await context.Users.AnyAsync())
                {
                    // Пароль для всех пользователей: password123
                    var passwordHash = BCrypt.HashPassword("password123");

                    context.Users.Add(new User
                    {
                        FullName = "Админ Админов",
                        Email = "admin@example.com",
                        PhoneNumber = "+79990001122",
                        PasswordHash = BCrypt.HashPassword("admin"),
                        Role = UserRole.Admin
                    });

                    // Добавляем пользователей
                    context.Users.AddRange(
                                    new User { FullName = "Иван Иванов", Email = "ivan@example.com", PhoneNumber = "+79991234567", PasswordHash = passwordHash, Role = UserRole.User },
                                    new User { FullName = "Мария Петрова", Email = "maria@example.com", PhoneNumber = "+79997654321", PasswordHash = passwordHash, Role = UserRole.User },
                                    new User { FullName = "Алексей Смирнов", Email = "alexey@example.com", PhoneNumber = "+79993332211", PasswordHash = passwordHash, Role = UserRole.User },
                                    new User { FullName = "Екатерина Волкова", Email = "ekaterina@example.com", PhoneNumber = "+79994445566", PasswordHash = passwordHash, Role = UserRole.User },
                                    new User { FullName = "Дмитрий Соколов", Email = "dmitry@example.com", PhoneNumber = "+79995556677", PasswordHash = passwordHash, Role = UserRole.User }
                                );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Пользователи добавлены, включая администратора.");

                    // Добавляем автомобили
                    context.Cars.AddRange(
                        new Car { Brand = "Toyota", Model = "Camry", Year = 2020, Engine = "2.5L", BodyType = "Sedan", Price = 2000000, PhotoUrl = "https://mkt-vehicleimages-prd.autotradercdn.ca/photos/chrome/Expanded/White/2020TOC020016/2020TOC02001601.jpg", Status = CarStatus.Available },
                        new Car { Brand = "BMW", Model = "X5", Year = 2019, Engine = "3.0L", BodyType = "SUV", Price = 3500000, PhotoUrl = "https://mkt-vehicleimages-prd.autotradercdn.ca/photos/chrome/Expanded/White/2019BMS190001/2019BMS19000101.jpg", Status = CarStatus.Reserved },
                        new Car { Brand = "Mercedes", Model = "C-Class", Year = 2021, Engine = "2.0L", BodyType = "Sedan", Price = 2800000, PhotoUrl = "https://www.mercedes-benz.ca/content/dam/mb-nafta/ca/myco/my25/c-class/class-page/series/MBCAN-2025-C-SEDAN-CL-1-3-DR.jpg", Status = CarStatus.Leased },
                        new Car { Brand = "Honda", Model = "Civic", Year = 2018, Engine = "1.8L", BodyType = "Hatchback", Price = 1500000, PhotoUrl = "https://di-uploads-pod10.dealerinspire.com/hondaworlddowney/uploads/2018/03/2017-honda-civic-lx-front-side.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Audi", Model = "A4", Year = 2020, Engine = "2.0L", BodyType = "Sedan", Price = 2500000, PhotoUrl = "https://cfwww.hgregoire.com/photos/by-size/612659/3648x2048/6718435.JPG", Status = CarStatus.Available },
                        new Car { Brand = "Lexus", Model = "RX", Year = 2021, Engine = "3.5L", BodyType = "SUV", Price = 4200000, PhotoUrl = "https://mkt-vehicleimages-prd.autotradercdn.ca/photos/chrome/Expanded/White/2021LES120001/2021LES12000101.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Volkswagen", Model = "Tiguan", Year = 2019, Engine = "2.0L", BodyType = "SUV", Price = 1800000, PhotoUrl = "https://static.overfuel.com/photos/86/184300/3VV3B7AX8KM137135_19.webp", Status = CarStatus.Available },
                        new Car { Brand = "Hyundai", Model = "Tucson", Year = 2020, Engine = "2.4L", BodyType = "SUV", Price = 1700000, PhotoUrl = "https://mkt-vehicleimages-prd.autotradercdn.ca/photos/chrome/Expanded/White/2020HYS020001/2020HYS02000101.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Kia", Model = "Sportage", Year = 2021, Engine = "2.0L", BodyType = "SUV", Price = 1900000, PhotoUrl = "https://www.co-optoyota.com.au/media-files/inventory/3c773d79-a420-4cf8-89c3-5a8a78afb1a8/00a03bb3-2588-4570-a746-0d3d2dd45218/large-image.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Mazda", Model = "CX-5", Year = 2020, Engine = "2.5L", BodyType = "SUV", Price = 2100000, PhotoUrl = "https://imgcdn.zigwheels.com.au/large/gallery/exterior/5/42/mazda-cx-5-front-angle-low-view-563994.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Ford", Model = "Focus", Year = 2019, Engine = "1.5L", BodyType = "Hatchback", Price = 1200000, PhotoUrl = "https://imgcdn.zigwheels.ph/large/gallery/exterior/7/846/ford-focus-front-angle-low-view-961094.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Skoda", Model = "Octavia", Year = 2021, Engine = "1.8L", BodyType = "Sedan", Price = 1600000, PhotoUrl = "https://gumtreeau-res.cloudinary.com/image/private/t_$_s-l400/move/9af2591d-b71e-4f55-91f8-a3ef00cde7c7", Status = CarStatus.Available },
                        new Car { Brand = "Volvo", Model = "XC60", Year = 2020, Engine = "2.0L", BodyType = "SUV", Price = 3200000, PhotoUrl = "https://media.drive.com.au/obj/tx_q:50,rs:auto:960:540:1/driveau/upload/vehicles/redbook/AUVVOLV2020AEEY/S0007L3P", Status = CarStatus.Available },
                        new Car { Brand = "Subaru", Model = "Forester", Year = 2019, Engine = "2.5L", BodyType = "SUV", Price = 2200000, PhotoUrl = "https://edge.pxcrush.net/dealerweb/car/cil/u8hkze2tuzil6eemqijkomwub.jpg?width=1000", Status = CarStatus.Available },
                        new Car { Brand = "Nissan", Model = "Qashqai", Year = 2021, Engine = "1.6L", BodyType = "SUV", Price = 1800000, PhotoUrl = "https://images.autobu.eu/mscars/thumbs/xl/nissan-qashqai-mhev-158-xtronic-2wd-n-connecta-comfort-pack-my24_10914716450903059710_0_xl.webp", Status = CarStatus.Available },
                        new Car { Brand = "Land Rover", Model = "Range Rover Evoque", Year = 2020, Engine = "2.0L", BodyType = "SUV", Price = 3800000, PhotoUrl = "https://static.overfuel.com/photos/398/120104/RU3JO6W4NRCP566VCBT4.webp", Status = CarStatus.Available },
                        new Car { Brand = "Porsche", Model = "Cayenne", Year = 2021, Engine = "3.0L", BodyType = "SUV", Price = 6500000, PhotoUrl = "https://autotraderau-res.cloudinary.com/image/upload/e_trim:10,f_auto/c_scale,t_cg_base,w_678/glasses/xIjRKdDM.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Jeep", Model = "Grand Cherokee", Year = 2019, Engine = "3.6L", BodyType = "SUV", Price = 3200000, PhotoUrl = "https://media.motorfuse.com/img.cfm/type/3/img/0F16CD4A4D02DBD7C4E6AFC4C52D0D10F5CB41", Status = CarStatus.Available },
                        new Car { Brand = "Tesla", Model = "Model 3", Year = 2021, Engine = "Electric", BodyType = "Sedan", Price = 3800000, PhotoUrl = "https://img.autobytel.com/chrome/colormatched_01/white/1280/cc_2021tsc03_01_1280/cc_2021tsc030006_01_1280_solb.jpg", Status = CarStatus.Available },
                        new Car { Brand = "Chevrolet", Model = "Camaro", Year = 2020, Engine = "6.2L", BodyType = "Coupe", Price = 4200000, PhotoUrl = "https://carsales.pxcrush.net/car/spec/S0007XBM.jpg?pxc_method=GravityFill&width=480&height=320&watermark=491459979", Status = CarStatus.Available },
                        new Car { Brand = "Toyota", Model = "Corolla", Year = 2022, Engine = "1.8L Hybrid", BodyType = "Sedan", Price = 1800000, PhotoUrl = "https://www.auto-data.net/images/f102/Toyota-Corolla-XII-E210-facelift-2022.jpg", Status = CarStatus.Available },
    new Car
    {
        Brand = "Toyota",
        Model = "Corolla Cross",
        Year = 2022,
        Engine = "2.0L",
        BodyType = "SUV",
        Price = 2000000,
        PhotoUrl = "https://images.cdn.autocar.co.uk/sites/autocar.co.uk/files/styles/gallery_slide/public/toyota-corolla-cross-01-front-tracking_4.jpg?itok=haCZAzUc",
        Status = CarStatus.Available
    },
    new Car
    {
        Brand = "Ford",
        Model = "Mustang",
        Year = 2021,
        Engine = "5.0L V8",
        BodyType = "Coupe",
        Price = 4500000,
        PhotoUrl = "https://img.autobytel.com/chrome/colormatched_01/white/640/cc_2021foc21_01_640/cc_2021foc210145_01_640_pq90e.jpg",
        Status = CarStatus.Available
    },
    new Car
    {
        Brand = "Ford",
        Model = "Mustang",
        Year = 2025,
        Engine = "V8 Dark Horse",
        BodyType = "Coupe",
        Price = 5500000,
        PhotoUrl = "https://media.ed.edmunds-media.com/ford/mustang/2025/oem/2025_ford_mustang_coupe_dark-horse_fq_oem_1_1280.jpg",
        Status = CarStatus.Available
    },
    new Car
    {
        Brand = "Honda",
        Model = "Accord",
        Year = 2022,
        Engine = "1.5L Turbo",
        BodyType = "Sedan",
        Price = 2300000,
        PhotoUrl = "https://mkt-vehicleimages-prd.autotradercdn.ca/photos/chrome/Expanded/White/2022HOC010001/2022HOC01000101.jpg",
        Status = CarStatus.Available
    },
    new Car
    {
        Brand = "Honda",
        Model = "Accord",
        Year = 2025,
        Engine = "1.5L Turbo Hybrid",
        BodyType = "Sedan",
        Price = 2700000,
        PhotoUrl = "https://carbike360-ae.s3.me-central-1.amazonaws.com/Bestune_B70_e050ab2b72.png",
        Status = CarStatus.Available
    }
                    );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Автомобили добавлены.");

                    // Добавляем бронирования
                    context.Reservations.AddRange(
                        new Reservation { UserId = 1, CarId = 2, ReservationStart = new DateTime(2025, 6, 10, 10, 0, 0, DateTimeKind.Utc), ReservationEnd = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc), IsActive = true },
                        new Reservation { UserId = 2, CarId = 4, ReservationStart = new DateTime(2025, 6, 12, 12, 0, 0, DateTimeKind.Utc), ReservationEnd = new DateTime(2025, 6, 14, 12, 0, 0, DateTimeKind.Utc), IsActive = false },
                        new Reservation { UserId = 3, CarId = 7, ReservationStart = new DateTime(2025, 7, 1, 9, 0, 0, DateTimeKind.Utc), ReservationEnd = new DateTime(2025, 7, 10, 9, 0, 0, DateTimeKind.Utc), IsActive = true },
                        new Reservation { UserId = 4, CarId = 12, ReservationStart = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc), ReservationEnd = new DateTime(2025, 6, 25, 14, 0, 0, DateTimeKind.Utc), IsActive = true }
                    );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Бронирования добавлены.");

                    // Добавляем договоры лизинга
                    context.LeaseContracts.AddRange(
                        new LeaseContract { UserId = 3, CarId = 3, LeaseStartDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), LeaseEndDate = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc), TotalCost = 336000 },
                        new LeaseContract { UserId = 1, CarId = 1, LeaseStartDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), LeaseEndDate = new DateTime(2025, 8, 31, 23, 59, 59, DateTimeKind.Utc), TotalCost = 120000 },
                        new LeaseContract { UserId = 5, CarId = 19, LeaseStartDate = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc), LeaseEndDate = new DateTime(2025, 11, 30, 23, 59, 59, DateTimeKind.Utc), TotalCost = 228000 }
                    );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Договоры лизинга добавлены.");

                    // Добавляем платежи
                    context.Payments.AddRange(
                        new Payment { LeaseContractId = 1, PaymentDate = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc), Amount = 28000, IsPaid = true },
                        new Payment { LeaseContractId = 1, PaymentDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc), Amount = 28000, IsPaid = false },
                        new Payment { LeaseContractId = 2, PaymentDate = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc), Amount = 20000, IsPaid = true },
                        new Payment { LeaseContractId = 3, PaymentDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc), Amount = 38000, IsPaid = true }
                    );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Платежи добавлены.");

                    // Добавляем избранное
                    context.Favorites.AddRange(
                        new Favorite { UserId = 1, CarId = 4 },
                        new Favorite { UserId = 2, CarId = 5 },
                        new Favorite { UserId = 3, CarId = 1 },
                        new Favorite { UserId = 4, CarId = 8 },
                        new Favorite { UserId = 5, CarId = 19 },
                        new Favorite { UserId = 1, CarId = 12 },
                        new Favorite { UserId = 2, CarId = 17 }
                    );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Избранное добавлено.");

                    // Добавляем отзывы
                    context.Reviews.AddRange(
                        new Review { CarId = 3, UserId = 3, Rating = 4, Comment = "Комфортный автомобиль, но расход топлива выше ожидаемого.", CreatedAt = new DateTime(2025, 2, 15, 10, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 1, UserId = 1, Rating = 5, Comment = "Отличная машина, надёжная и экономичная.", CreatedAt = new DateTime(2025, 4, 10, 12, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 4, UserId = 2, Rating = 3, Comment = "Неплохая, но шумоизоляция слабая.", CreatedAt = new DateTime(2025, 6, 5, 15, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 5, UserId = 4, Rating = 4, Comment = "Хороший автомобиль с отличным интерьером.", CreatedAt = new DateTime(2025, 3, 20, 11, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 7, UserId = 3, Rating = 5, Comment = "Превосходный кроссовер за свои деньги.", CreatedAt = new DateTime(2025, 5, 12, 14, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 9, UserId = 2, Rating = 4, Comment = "Удобный и практичный автомобиль для города.", CreatedAt = new DateTime(2025, 1, 25, 9, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 12, UserId = 1, Rating = 5, Comment = "Лучший выбор в своем классе. Рекомендую!", CreatedAt = new DateTime(2025, 4, 5, 16, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 14, UserId = 5, Rating = 3, Comment = "Неплохо, но ожидал большего за эти деньги.", CreatedAt = new DateTime(2025, 2, 28, 13, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 16, UserId = 4, Rating = 5, Comment = "Роскошный внедорожник с отличным дизайном.", CreatedAt = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 17, UserId = 2, Rating = 5, Comment = "Мощный и стильный. Мечта сбылась!", CreatedAt = new DateTime(2025, 5, 15, 12, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 19, UserId = 5, Rating = 5, Comment = "Электромобиль будущего уже сегодня. В восторге!", CreatedAt = new DateTime(2025, 4, 22, 15, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 2, UserId = 1, Rating = 4, Comment = "Хороший внедорожник, но дорогой в обслуживании.", CreatedAt = new DateTime(2025, 3, 10, 11, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 6, UserId = 3, Rating = 5, Comment = "Тихий, комфортный и надежный. Идеален для семьи.", CreatedAt = new DateTime(2025, 2, 5, 14, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 10, UserId = 4, Rating = 4, Comment = "Отличное сочетание цены и качества.", CreatedAt = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc) },
                        new Review { CarId = 20, UserId = 5, Rating = 5, Comment = "Мощный двигатель и агрессивный дизайн. Люблю!", CreatedAt = new DateTime(2025, 5, 20, 16, 0, 0, DateTimeKind.Utc) }
                    );

                    await context.SaveChangesAsync();
                    Console.WriteLine("Отзывы добавлены.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сидинге данных: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}
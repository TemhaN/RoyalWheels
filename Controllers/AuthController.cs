using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.DTO;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

using BCrypt.Net;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        if (existingUser != null) return BadRequest(new { message = "Пользователь с таким email уже существует." });

        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            PasswordHash = BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User // Все новые пользователи по умолчанию User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Message = "Регистрация успешна!"
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null || !BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Неверный email или пароль." });
        }

        var token = GenerateJwtToken(user);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Message = "Вход успешен!"
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult> UpdateProfile([FromBody] RegisterDto updateDto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized(new { message = "Пользователь не авторизован." });

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "Пользователь не найден." });

        user.FullName = updateDto.FullName;
        user.Email = updateDto.Email;
        user.PhoneNumber = updateDto.PhoneNumber;
        if (!string.IsNullOrEmpty(updateDto.Password))
        {
            user.PasswordHash = BCrypt.HashPassword(updateDto.Password);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Профиль обновлён." });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()) // Добавляем роль в токен
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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
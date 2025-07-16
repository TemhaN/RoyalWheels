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
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{leaseContractId}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByLeaseContract(int leaseContractId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized("Пользователь не авторизован.");

        var leaseExists = await _context.LeaseContracts
            .AnyAsync(l => l.Id == leaseContractId && l.UserId == userId);
        if (!leaseExists) return NotFound("Договор лизинга не найден или не принадлежит пользователю.");

        var payments = await _context.Payments
            .Where(p => p.LeaseContractId == leaseContractId)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                IsPaid = p.IsPaid
            })
            .ToListAsync();

        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] PaymentDto paymentRequest)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized("Пользователь не авторизован.");

            var leaseContract = await _context.LeaseContracts
                .FirstOrDefaultAsync(l => l.Id == paymentRequest.Id && l.UserId == userId);
            if (leaseContract == null)
                return NotFound("Договор лизинга не найден или не принадлежит пользователю.");

            var payment = new Payment
            {
                LeaseContractId = paymentRequest.Id,
                PaymentDate = paymentRequest.PaymentDate,
                Amount = paymentRequest.Amount,
                IsPaid = paymentRequest.IsPaid
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var paymentDto = new PaymentDto
            {
                Id = payment.Id,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                IsPaid = payment.IsPaid
            };

            return Ok(paymentDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating payment: {ex.Message}\nStackTrace: {ex.StackTrace}");
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
namespace ChatApp.Api.Controllers;

using ChatApp.Application.Abstractions;
using ChatApp.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly ICurrentUserProvider _currentUser;

    public PaymentsController(IPaymentService payments, ICurrentUserProvider currentUser)
    {
        _payments = payments;
        _currentUser = currentUser;
    }

    [HttpPost]
    public Task<PaymentDto> Create([FromBody] CreatePaymentRequest req)
        => _payments.CreateAsync(_currentUser.UserId, req);

    [HttpGet]
    public Task<IReadOnlyList<PaymentDto>> List()
        => _payments.ListForUserAsync(_currentUser.UserId);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> Get(Guid id)
    {
        var dto = await _payments.GetByIdAsync(_currentUser.UserId, id);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/complete")]
    public Task<PaymentDto> Complete(Guid id)
        => _payments.CompleteAsync(id);
}

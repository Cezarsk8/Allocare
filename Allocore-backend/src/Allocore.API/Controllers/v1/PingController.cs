namespace Allocore.API.Controllers.v1;

using Allocore.Application.Features.Ping;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PingResponse>> Get(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new PingQuery(), cancellationToken);
        return Ok(response);
    }
}

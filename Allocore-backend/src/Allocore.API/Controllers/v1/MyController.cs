namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Application.Features.Companies.Queries;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/my")]
[Authorize]
public class MyController : ControllerBase
{
    private readonly IMediator _mediator;

    public MyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all companies the current user is associated with.
    /// </summary>
    [HttpGet("companies")]
    [ProducesResponseType(typeof(IEnumerable<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCompanies(CancellationToken cancellationToken)
    {
        var companies = await _mediator.Send(new GetMyCompaniesQuery(), cancellationToken);
        return Ok(companies);
    }
}

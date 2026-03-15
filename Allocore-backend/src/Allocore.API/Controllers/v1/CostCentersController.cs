namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.CostCenters.Commands;
using Allocore.Application.Features.CostCenters.DTOs;
using Allocore.Application.Features.CostCenters.Queries;
using Allocore.Application.Features.Employees.Queries;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/cost-centers")]
[Authorize]
public class CostCentersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CostCentersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List cost centers for a company (paginated, filterable).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCostCenters(
        Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetCostCentersPagedQuery(companyId, page, pageSize, isActive, search),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a cost center by ID with employee count.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCostCenter(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCostCenterByIdQuery(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new cost center.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCostCenter(
        Guid companyId,
        [FromBody] CreateCostCenterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCostCenterCommand(companyId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetCostCenter), new { companyId, id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update a cost center.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCostCenter(
        Guid companyId,
        Guid id,
        [FromBody] UpdateCostCenterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCostCenterCommand(companyId, id, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Deactivate a cost center.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateCostCenter(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateCostCenterCommand(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Activate a cost center.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateCostCenter(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivateCostCenterCommand(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Get employees in a cost center (paginated).
    /// </summary>
    [HttpGet("{id:guid}/employees")]
    public async Task<IActionResult> GetCostCenterEmployees(
        Guid companyId,
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetEmployeesByCostCenterQuery(companyId, id, page, pageSize),
            cancellationToken);
        return Ok(result);
    }
}

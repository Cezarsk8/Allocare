namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.Employees.Commands;
using Allocore.Application.Features.Employees.DTOs;
using Allocore.Application.Features.Employees.Queries;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List employees for a company (paginated, filterable).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEmployees(
        Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? costCenterId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetEmployeesPagedQuery(companyId, page, pageSize, costCenterId, isActive, search),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get an employee by ID with cost center details.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmployee(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new employee.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEmployee(
        Guid companyId,
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateEmployeeCommand(companyId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetEmployee), new { companyId, id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update an employee.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(
        Guid companyId,
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateEmployeeCommand(companyId, id, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Terminate an employee.
    /// </summary>
    [HttpPatch("{id:guid}/terminate")]
    public async Task<IActionResult> TerminateEmployee(
        Guid companyId,
        Guid id,
        [FromBody] TerminateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new TerminateEmployeeCommand(companyId, id, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Reactivate a terminated employee.
    /// </summary>
    [HttpPatch("{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateEmployee(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReactivateEmployeeCommand(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Deactivate an employee.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateEmployee(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateEmployeeCommand(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Activate an employee.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateEmployee(Guid companyId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ActivateEmployeeCommand(companyId, id), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}

namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.Companies.Commands;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Application.Features.Companies.Queries;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new company. The creating user becomes the Owner.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCompanyCommand(request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetCompany), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Get a company by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompany(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Update a company.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCompanyCommand(id, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Add a user to a company.
    /// </summary>
    [HttpPost("{companyId:guid}/users")]
    [ProducesResponseType(typeof(UserCompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddUserToCompany(Guid companyId, [FromBody] AddUserToCompanyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddUserToCompanyCommand(companyId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Remove a user from a company.
    /// </summary>
    [HttpDelete("{companyId:guid}/users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveUserFromCompany(Guid companyId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveUserFromCompanyCommand(companyId, userId), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Get all users in a company.
    /// </summary>
    [HttpGet("{companyId:guid}/users")]
    [ProducesResponseType(typeof(IEnumerable<UserCompanyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompanyUsers(Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCompanyUsersQuery(companyId), cancellationToken);
        return Ok(result);
    }
}

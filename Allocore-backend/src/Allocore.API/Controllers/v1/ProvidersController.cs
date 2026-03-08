namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.Providers.Commands;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Application.Features.Providers.Queries;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/providers")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProvidersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List providers for a company (paginated, filterable).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProviders(
        Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProvidersPagedQuery(companyId, page, pageSize, category, isActive, search),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a provider by ID with full details and contacts.
    /// </summary>
    [HttpGet("{providerId:guid}")]
    public async Task<IActionResult> GetProvider(Guid companyId, Guid providerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProviderByIdQuery(companyId, providerId), cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new provider (optionally with contacts).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProvider(
        Guid companyId,
        [FromBody] CreateProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProviderCommand(companyId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetProvider), new { companyId, providerId = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update a provider's details (not contacts).
    /// </summary>
    [HttpPut("{providerId:guid}")]
    public async Task<IActionResult> UpdateProvider(
        Guid companyId,
        Guid providerId,
        [FromBody] UpdateProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProviderCommand(companyId, providerId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Deactivate a provider (soft delete).
    /// </summary>
    [HttpPatch("{providerId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateProvider(Guid companyId, Guid providerId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeactivateProviderCommand(companyId, providerId), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    /// <summary>
    /// Add a contact to a provider.
    /// </summary>
    [HttpPost("{providerId:guid}/contacts")]
    public async Task<IActionResult> AddContact(
        Guid companyId,
        Guid providerId,
        [FromBody] AddProviderContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddProviderContactCommand(companyId, providerId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Update a provider contact.
    /// </summary>
    [HttpPut("{providerId:guid}/contacts/{contactId:guid}")]
    public async Task<IActionResult> UpdateContact(
        Guid companyId,
        Guid providerId,
        Guid contactId,
        [FromBody] UpdateProviderContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProviderContactCommand(companyId, providerId, contactId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>
    /// Remove a contact from a provider.
    /// </summary>
    [HttpDelete("{providerId:guid}/contacts/{contactId:guid}")]
    public async Task<IActionResult> RemoveContact(
        Guid companyId,
        Guid providerId,
        Guid contactId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveProviderContactCommand(companyId, providerId, contactId), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}

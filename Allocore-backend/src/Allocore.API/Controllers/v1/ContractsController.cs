namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.Contracts.Commands;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Application.Features.Contracts.Queries;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetContracts(
        Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? providerId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? expiringOnly = null,
        [FromQuery] int expiringDays = 30,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetContractsPagedQuery(companyId, page, pageSize, providerId, status, expiringOnly, expiringDays, search),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{contractId:guid}")]
    public async Task<IActionResult> GetContract(Guid companyId, Guid contractId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetContractByIdQuery(companyId, contractId), cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringContracts(
        Guid companyId,
        [FromQuery] int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetExpiringContractsQuery(companyId, withinDays), cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-provider/{providerId:guid}")]
    public async Task<IActionResult> GetContractsByProvider(
        Guid companyId,
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetContractsByProviderQuery(companyId, providerId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContract(
        Guid companyId,
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateContractCommand(companyId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetContract), new { companyId, contractId = result.Value!.Id }, result.Value);
    }

    [HttpPut("{contractId:guid}")]
    public async Task<IActionResult> UpdateContract(
        Guid companyId,
        Guid contractId,
        [FromBody] UpdateContractRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateContractCommand(companyId, contractId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("{contractId:guid}/status")]
    public async Task<IActionResult> UpdateContractStatus(
        Guid companyId,
        Guid contractId,
        [FromBody] UpdateContractStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateContractStatusCommand(companyId, contractId, request.Status), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("{contractId:guid}/services")]
    public async Task<IActionResult> AddService(
        Guid companyId,
        Guid contractId,
        [FromBody] CreateContractServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AddContractServiceCommand(companyId, contractId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPut("{contractId:guid}/services/{serviceId:guid}")]
    public async Task<IActionResult> UpdateService(
        Guid companyId,
        Guid contractId,
        Guid serviceId,
        [FromBody] CreateContractServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateContractServiceCommand(companyId, contractId, serviceId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("{contractId:guid}/services/{serviceId:guid}")]
    public async Task<IActionResult> RemoveService(
        Guid companyId,
        Guid contractId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveContractServiceCommand(companyId, contractId, serviceId), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }
}

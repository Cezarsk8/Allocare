namespace Allocore.API.Controllers.v1;

using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Allocore.Application.Features.Notes.Commands;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Application.Features.Notes.Queries;
using Allocore.Domain.Entities.Notes;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/companies/{companyId:guid}")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("providers/{providerId:guid}/notes")]
    public async Task<IActionResult> GetProviderNotes(
        Guid companyId,
        Guid providerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetNotesByEntityQuery(companyId, NoteEntityType.Provider, providerId, page, pageSize, category),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("providers/{providerId:guid}/notes")]
    public async Task<IActionResult> AddProviderNote(
        Guid companyId,
        Guid providerId,
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateNoteCommand(companyId, NoteEntityType.Provider, providerId, request),
            cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("contracts/{contractId:guid}/notes")]
    public async Task<IActionResult> GetContractNotes(
        Guid companyId,
        Guid contractId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetNotesByEntityQuery(companyId, NoteEntityType.Contract, contractId, page, pageSize, category),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("contracts/{contractId:guid}/notes")]
    public async Task<IActionResult> AddContractNote(
        Guid companyId,
        Guid contractId,
        [FromBody] CreateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateNoteCommand(companyId, NoteEntityType.Contract, contractId, request),
            cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPut("notes/{noteId:guid}")]
    public async Task<IActionResult> UpdateNote(
        Guid companyId,
        Guid noteId,
        [FromBody] UpdateNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateNoteCommand(companyId, noteId, request), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpDelete("notes/{noteId:guid}")]
    public async Task<IActionResult> DeleteNote(
        Guid companyId,
        Guid noteId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteNoteCommand(companyId, noteId), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPatch("notes/{noteId:guid}/pin")]
    public async Task<IActionResult> TogglePin(
        Guid companyId,
        Guid noteId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new TogglePinNoteCommand(companyId, noteId), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpGet("reminders")]
    public async Task<IActionResult> GetReminders(
        Guid companyId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetRemindersQuery(companyId, fromDate, toDate),
            cancellationToken);
        return Ok(result);
    }
}

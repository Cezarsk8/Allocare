namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Common;

public record UpdateNoteCommand(
    Guid CompanyId,
    Guid NoteId,
    UpdateNoteRequest Request
) : IRequest<Result<NoteDto>>;

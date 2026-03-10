namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Notes;

public record CreateNoteCommand(
    Guid CompanyId,
    NoteEntityType EntityType,
    Guid EntityId,
    CreateNoteRequest Request
) : IRequest<Result<NoteDto>>;

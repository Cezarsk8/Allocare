namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Domain.Common;

public record DeleteNoteCommand(
    Guid CompanyId,
    Guid NoteId
) : IRequest<Result>;

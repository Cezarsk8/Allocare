namespace Allocore.Application.Features.Notes.Queries;

using MediatR;
using Allocore.Application.Common;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Entities.Notes;

public record GetNotesByEntityQuery(
    Guid CompanyId,
    NoteEntityType EntityType,
    Guid EntityId,
    int Page = 1,
    int PageSize = 20,
    string? Category = null
) : IRequest<PagedResult<NoteDto>>;

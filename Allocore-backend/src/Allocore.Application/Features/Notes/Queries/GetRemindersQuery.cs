namespace Allocore.Application.Features.Notes.Queries;

using MediatR;
using Allocore.Application.Features.Notes.DTOs;

public record GetRemindersQuery(
    Guid CompanyId,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<IEnumerable<NoteDto>>;

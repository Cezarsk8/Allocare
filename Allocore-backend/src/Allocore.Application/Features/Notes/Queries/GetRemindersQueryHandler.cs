namespace Allocore.Application.Features.Notes.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Notes.DTOs;

public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, IEnumerable<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetRemindersQueryHandler(
        INoteRepository noteRepository,
        IUserRepository userRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _noteRepository = noteRepository;
        _userRepository = userRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<NoteDto>> Handle(GetRemindersQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return [];

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, query.CompanyId, cancellationToken);
        if (!hasAccess)
            return [];

        var fromDate = query.FromDate ?? DateTime.UtcNow;
        var toDate = query.ToDate ?? DateTime.UtcNow.AddDays(30);

        var notes = await _noteRepository.GetRemindersForCompanyAsync(
            query.CompanyId, fromDate, toDate, cancellationToken);

        // Batch load author names
        var notesList = notes.ToList();
        var authorIds = notesList.Select(n => n.AuthorUserId).Distinct();
        var authors = await _userRepository.GetByIdsAsync(authorIds, cancellationToken);
        var authorMap = authors.ToDictionary(u => u.Id, u => u.FullName);

        return notesList.Select(n =>
            NoteMapper.ToDto(n, authorMap.GetValueOrDefault(n.AuthorUserId, "Unknown User")));
    }
}

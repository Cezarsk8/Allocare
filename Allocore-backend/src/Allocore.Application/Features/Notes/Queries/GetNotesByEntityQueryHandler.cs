namespace Allocore.Application.Features.Notes.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Common;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Entities.Notes;

public class GetNotesByEntityQueryHandler : IRequestHandler<GetNotesByEntityQuery, PagedResult<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetNotesByEntityQueryHandler(
        INoteRepository noteRepository,
        IProviderRepository providerRepository,
        IContractRepository contractRepository,
        IUserRepository userRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _noteRepository = noteRepository;
        _providerRepository = providerRepository;
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<NoteDto>> Handle(GetNotesByEntityQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return new PagedResult<NoteDto>([], query.Page, query.PageSize, 0, 0);

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, query.CompanyId, cancellationToken);
        if (!hasAccess)
            return new PagedResult<NoteDto>([], query.Page, query.PageSize, 0, 0);

        // Verify entity exists and belongs to company
        var entityValid = await ValidateEntityAsync(query.EntityType, query.EntityId, query.CompanyId, cancellationToken);
        if (!entityValid)
            return new PagedResult<NoteDto>([], query.Page, query.PageSize, 0, 0);

        // Parse category filter
        NoteCategory? categoryFilter = null;
        if (!string.IsNullOrEmpty(query.Category) && Enum.TryParse<NoteCategory>(query.Category, true, out var parsed))
            categoryFilter = parsed;

        var (notes, totalCount) = await _noteRepository.GetPagedByEntityAsync(
            query.EntityType, query.EntityId, query.Page, query.PageSize, categoryFilter, cancellationToken);

        // Batch load author names
        var notesList = notes.ToList();
        var authorIds = notesList.Select(n => n.AuthorUserId).Distinct();
        var authors = await _userRepository.GetByIdsAsync(authorIds, cancellationToken);
        var authorMap = authors.ToDictionary(u => u.Id, u => u.FullName);

        var dtos = notesList.Select(n =>
            NoteMapper.ToDto(n, authorMap.GetValueOrDefault(n.AuthorUserId, "Unknown User")));

        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
        return new PagedResult<NoteDto>(dtos, query.Page, query.PageSize, totalCount, totalPages);
    }

    private async Task<bool> ValidateEntityAsync(NoteEntityType entityType, Guid entityId, Guid companyId, CancellationToken cancellationToken)
    {
        switch (entityType)
        {
            case NoteEntityType.Provider:
                var provider = await _providerRepository.GetByIdAsync(entityId, cancellationToken);
                return provider != null && provider.CompanyId == companyId;

            case NoteEntityType.Contract:
                var contract = await _contractRepository.GetByIdAsync(entityId, cancellationToken);
                return contract != null && contract.CompanyId == companyId;

            default:
                return false;
        }
    }
}

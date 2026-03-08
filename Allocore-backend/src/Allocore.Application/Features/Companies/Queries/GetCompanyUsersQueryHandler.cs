namespace Allocore.Application.Features.Companies.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Companies.DTOs;

public class GetCompanyUsersQueryHandler : IRequestHandler<GetCompanyUsersQuery, IEnumerable<UserCompanyDto>>
{
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetCompanyUsersQueryHandler(
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<UserCompanyDto>> Handle(GetCompanyUsersQuery request, CancellationToken cancellationToken)
    {
        // Verify user has access to this company
        if (_currentUser.UserId.HasValue)
        {
            var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
                _currentUser.UserId.Value, request.CompanyId, cancellationToken);
            var isAdmin = _currentUser.Roles.Contains("Admin");

            if (!hasAccess && !isAdmin)
                return Enumerable.Empty<UserCompanyDto>();
        }

        var userCompanies = await _userCompanyRepository.GetByCompanyIdAsync(
            request.CompanyId, cancellationToken);

        return userCompanies
            .Where(uc => uc.User is not null)
            .Select(uc => new UserCompanyDto(
                uc.UserId,
                uc.User!.Email,
                uc.User.FullName,
                uc.CompanyId,
                uc.Company?.Name ?? string.Empty,
                uc.RoleInCompany.ToString(),
                uc.CreatedAt
            ));
    }
}

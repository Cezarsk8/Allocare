namespace Allocore.Application.Features.Companies.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Companies.DTOs;

public class GetMyCompaniesQueryHandler : IRequestHandler<GetMyCompaniesQuery, IEnumerable<CompanyDto>>
{
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyCompaniesQueryHandler(
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CompanyDto>> Handle(GetMyCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Enumerable.Empty<CompanyDto>();

        var userCompanies = await _userCompanyRepository.GetByUserIdAsync(
            _currentUser.UserId.Value, cancellationToken);

        return userCompanies
            .Where(uc => uc.Company is not null)
            .Select(uc => new CompanyDto(
                uc.Company!.Id,
                uc.Company.Name,
                uc.Company.LegalName,
                uc.Company.TaxId,
                uc.Company.IsActive,
                uc.Company.CreatedAt,
                uc.RoleInCompany.ToString()
            ));
    }
}

namespace Allocore.Application.Features.Companies.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetCompanyByIdQueryHandler(
        ICompanyRepository companyRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _companyRepository = companyRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CompanyDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
            return Result.Failure<CompanyDto>("Company not found.");

        // Check if user has access to this company
        string? userRole = null;
        if (_currentUser.UserId.HasValue)
        {
            var uc = await _userCompanyRepository.GetByUserAndCompanyAsync(
                _currentUser.UserId.Value, request.CompanyId, cancellationToken);
            var isAdmin = _currentUser.Roles.Contains("Admin");

            if (uc is null && !isAdmin)
                return Result.Failure<CompanyDto>("You don't have access to this company.");

            userRole = uc?.RoleInCompany.ToString();
        }

        return Result.Success(new CompanyDto(
            company.Id,
            company.Name,
            company.LegalName,
            company.TaxId,
            company.IsActive,
            company.CreatedAt,
            userRole
        ));
    }
}

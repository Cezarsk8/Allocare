namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Companies;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(
        ICompanyRepository companyRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (await _companyRepository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result.Failure<CompanyDto>("A company with this name already exists.");

        var company = Company.Create(request.Name, request.LegalName, request.TaxId);
        await _companyRepository.AddAsync(company, cancellationToken);

        if (_currentUser.UserId.HasValue)
        {
            var userCompany = UserCompany.Create(_currentUser.UserId.Value, company.Id, RoleInCompany.Owner);
            await _userCompanyRepository.AddAsync(userCompany, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CompanyDto(
            company.Id,
            company.Name,
            company.LegalName,
            company.TaxId,
            company.IsActive,
            company.CreatedAt,
            "Owner"
        ));
    }
}

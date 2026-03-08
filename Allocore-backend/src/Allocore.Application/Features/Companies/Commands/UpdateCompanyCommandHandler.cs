namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompanyCommandHandler(
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

    public async Task<Result<CompanyDto>> Handle(UpdateCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company is null)
            return Result.Failure<CompanyDto>("Company not found.");

        // Check permission: must be Owner or Admin
        if (_currentUser.UserId.HasValue)
        {
            var isOwner = await _userCompanyRepository.UserIsOwnerOfCompanyAsync(
                _currentUser.UserId.Value, command.CompanyId, cancellationToken);
            var isAdmin = _currentUser.Roles.Contains("Admin");

            if (!isOwner && !isAdmin)
                return Result.Failure<CompanyDto>("You don't have permission to update this company.");
        }

        var request = command.Request;
        company.Update(request.Name, request.LegalName, request.TaxId);
        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get current user's role in this company
        string? userRole = null;
        if (_currentUser.UserId.HasValue)
        {
            var uc = await _userCompanyRepository.GetByUserAndCompanyAsync(
                _currentUser.UserId.Value, command.CompanyId, cancellationToken);
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

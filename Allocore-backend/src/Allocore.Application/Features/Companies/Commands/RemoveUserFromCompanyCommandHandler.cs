namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Companies;

public class RemoveUserFromCompanyCommandHandler : IRequestHandler<RemoveUserFromCompanyCommand, Result>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromCompanyCommandHandler(
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

    public async Task<Result> Handle(RemoveUserFromCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company is null)
            return Result.Failure("Company not found.");

        // Check permission: must be Owner or Admin
        if (_currentUser.UserId.HasValue)
        {
            var isOwner = await _userCompanyRepository.UserIsOwnerOfCompanyAsync(
                _currentUser.UserId.Value, command.CompanyId, cancellationToken);
            var isAdmin = _currentUser.Roles.Contains("Admin");

            if (!isOwner && !isAdmin)
                return Result.Failure("You don't have permission to remove users from this company.");
        }

        var userCompany = await _userCompanyRepository.GetByUserAndCompanyAsync(
            command.UserId, command.CompanyId, cancellationToken);
        if (userCompany is null)
            return Result.Failure("User is not linked to this company.");

        // Prevent removing the last Owner
        if (userCompany.RoleInCompany == RoleInCompany.Owner)
        {
            var companyUsers = await _userCompanyRepository.GetByCompanyIdAsync(command.CompanyId, cancellationToken);
            var ownerCount = companyUsers.Count(uc => uc.RoleInCompany == RoleInCompany.Owner);
            if (ownerCount <= 1)
                return Result.Failure("Cannot remove the last owner of a company.");
        }

        await _userCompanyRepository.DeleteAsync(userCompany, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

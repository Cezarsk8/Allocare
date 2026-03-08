namespace Allocore.Application.Features.Companies.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Companies.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Companies;

public class AddUserToCompanyCommandHandler : IRequestHandler<AddUserToCompanyCommand, Result<UserCompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AddUserToCompanyCommandHandler(
        ICompanyRepository companyRepository,
        IUserRepository userRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserCompanyDto>> Handle(AddUserToCompanyCommand command, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company is null)
            return Result.Failure<UserCompanyDto>("Company not found.");

        // Check permission: must be Owner or Admin
        if (_currentUser.UserId.HasValue)
        {
            var isOwner = await _userCompanyRepository.UserIsOwnerOfCompanyAsync(
                _currentUser.UserId.Value, command.CompanyId, cancellationToken);
            var isAdmin = _currentUser.Roles.Contains("Admin");

            if (!isOwner && !isAdmin)
                return Result.Failure<UserCompanyDto>("You don't have permission to add users to this company.");
        }

        var user = await _userRepository.GetByIdAsync(command.Request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<UserCompanyDto>("User not found.");

        var existingLink = await _userCompanyRepository.GetByUserAndCompanyAsync(
            command.Request.UserId, command.CompanyId, cancellationToken);
        if (existingLink is not null)
            return Result.Failure<UserCompanyDto>("User is already linked to this company.");

        if (!Enum.TryParse<RoleInCompany>(command.Request.RoleInCompany, out var role))
            return Result.Failure<UserCompanyDto>("Invalid role specified.");

        var userCompany = UserCompany.Create(command.Request.UserId, command.CompanyId, role);
        await _userCompanyRepository.AddAsync(userCompany, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserCompanyDto(
            user.Id,
            user.Email,
            user.FullName,
            company.Id,
            company.Name,
            role.ToString(),
            userCompany.CreatedAt
        ));
    }
}

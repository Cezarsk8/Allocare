namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Contracts;

public class UpdateContractStatusCommandHandler : IRequestHandler<UpdateContractStatusCommand, Result>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateContractStatusCommandHandler(
        IContractRepository contractRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateContractStatusCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure("You don't have access to this company.");

        var contract = await _contractRepository.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract == null || contract.CompanyId != command.CompanyId)
            return Result.Failure("Contract not found.");

        if (!Enum.TryParse<ContractStatus>(command.NewStatus, true, out var newStatus))
            return Result.Failure("Invalid contract status.");

        contract.UpdateStatus(newStatus);
        await _contractRepository.UpdateAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

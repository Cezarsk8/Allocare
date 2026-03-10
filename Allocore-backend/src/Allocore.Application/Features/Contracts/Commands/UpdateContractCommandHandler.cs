namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Contracts;

public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, Result<ContractDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateContractCommandHandler(
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

    public async Task<Result<ContractDto>> Handle(UpdateContractCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ContractDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ContractDto>("You don't have access to this company.");

        var contract = await _contractRepository.GetByIdWithDetailsAsync(command.ContractId, cancellationToken);
        if (contract == null || contract.CompanyId != command.CompanyId)
            return Result.Failure<ContractDto>("Contract not found.");

        // Check contract number uniqueness if changed
        if (!string.IsNullOrEmpty(command.Request.ContractNumber) && command.Request.ContractNumber != contract.ContractNumber)
        {
            if (await _contractRepository.ExistsByContractNumberInCompanyExcludingAsync(
                command.CompanyId, command.Request.ContractNumber, command.ContractId, cancellationToken))
                return Result.Failure<ContractDto>("A contract with this number already exists in this company.");
        }

        // Parse enums
        var status = contract.Status;
        if (!string.IsNullOrEmpty(command.Request.Status) && !Enum.TryParse(command.Request.Status, true, out status))
            return Result.Failure<ContractDto>("Invalid contract status.");

        var billingFrequency = contract.BillingFrequency;
        if (!string.IsNullOrEmpty(command.Request.BillingFrequency) && !Enum.TryParse(command.Request.BillingFrequency, true, out billingFrequency))
            return Result.Failure<ContractDto>("Invalid billing frequency.");

        contract.Update(
            command.Request.Title,
            command.Request.ContractNumber,
            status,
            command.Request.StartDate,
            command.Request.EndDate,
            command.Request.RenewalDate,
            command.Request.AutoRenew,
            command.Request.RenewalNoticeDays,
            billingFrequency,
            command.Request.TotalValue,
            command.Request.Currency,
            command.Request.PaymentTerms,
            command.Request.PriceConditions,
            command.Request.LegalTeamContact,
            command.Request.InternalOwner,
            command.Request.Description,
            command.Request.TermsAndConditions);

        await _contractRepository.UpdateAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ContractMapper.ToDto(contract, contract.Provider?.Name ?? string.Empty));
    }
}

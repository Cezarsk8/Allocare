namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Contracts;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, Result<ContractDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContractCommandHandler(
        IContractRepository contractRepository,
        IProviderRepository providerRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _providerRepository = providerRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ContractDto>> Handle(CreateContractCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ContractDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ContractDto>("You don't have access to this company.");

        // Verify provider exists and belongs to company
        var provider = await _providerRepository.GetByIdAsync(command.Request.ProviderId, cancellationToken);
        if (provider == null || provider.CompanyId != command.CompanyId)
            return Result.Failure<ContractDto>("Provider not found in this company.");

        // Check contract number uniqueness
        if (!string.IsNullOrEmpty(command.Request.ContractNumber))
        {
            if (await _contractRepository.ExistsByContractNumberInCompanyAsync(
                command.CompanyId, command.Request.ContractNumber, cancellationToken))
                return Result.Failure<ContractDto>("A contract with this number already exists in this company.");
        }

        // Parse enums
        var status = ContractStatus.Draft;
        if (!string.IsNullOrEmpty(command.Request.Status) && !Enum.TryParse(command.Request.Status, true, out status))
            return Result.Failure<ContractDto>("Invalid contract status.");

        var billingFrequency = BillingFrequency.Monthly;
        if (!string.IsNullOrEmpty(command.Request.BillingFrequency) && !Enum.TryParse(command.Request.BillingFrequency, true, out billingFrequency))
            return Result.Failure<ContractDto>("Invalid billing frequency.");

        var contract = Contract.Create(
            command.CompanyId,
            command.Request.ProviderId,
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

        // Add services if provided
        if (command.Request.Services != null)
        {
            foreach (var svcReq in command.Request.Services)
            {
                var svc = ContractService.Create(
                    contract.Id,
                    svcReq.ServiceName,
                    svcReq.ServiceDescription,
                    svcReq.UnitPrice,
                    svcReq.UnitType,
                    svcReq.Quantity,
                    svcReq.Notes);
                contract.AddService(svc);
            }
        }

        await _contractRepository.AddAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ContractMapper.ToDto(contract, provider.Name));
    }
}

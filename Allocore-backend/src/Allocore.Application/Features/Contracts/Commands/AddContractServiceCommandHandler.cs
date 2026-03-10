namespace Allocore.Application.Features.Contracts.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Contracts;

public class AddContractServiceCommandHandler : IRequestHandler<AddContractServiceCommand, Result<ContractServiceDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AddContractServiceCommandHandler(
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

    public async Task<Result<ContractServiceDto>> Handle(AddContractServiceCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ContractServiceDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ContractServiceDto>("You don't have access to this company.");

        var contract = await _contractRepository.GetByIdWithDetailsAsync(command.ContractId, cancellationToken);
        if (contract == null || contract.CompanyId != command.CompanyId)
            return Result.Failure<ContractServiceDto>("Contract not found.");

        var svc = ContractService.Create(
            contract.Id,
            command.Request.ServiceName,
            command.Request.ServiceDescription,
            command.Request.UnitPrice,
            command.Request.UnitType,
            command.Request.Quantity,
            command.Request.Notes);

        contract.AddService(svc);
        await _contractRepository.UpdateAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ContractMapper.ToServiceDto(svc));
    }
}

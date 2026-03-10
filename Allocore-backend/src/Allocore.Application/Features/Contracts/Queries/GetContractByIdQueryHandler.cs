namespace Allocore.Application.Features.Contracts.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Contracts.DTOs;
using Allocore.Domain.Common;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, Result<ContractDto>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;

    public GetContractByIdQueryHandler(
        IContractRepository contractRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser)
    {
        _contractRepository = contractRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<ContractDto>> Handle(GetContractByIdQuery query, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ContractDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, query.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ContractDto>("You don't have access to this company.");

        var contract = await _contractRepository.GetByIdWithDetailsAsync(query.ContractId, cancellationToken);
        if (contract == null || contract.CompanyId != query.CompanyId)
            return Result.Failure<ContractDto>("Contract not found.");

        return Result.Success(ContractMapper.ToDto(contract, contract.Provider?.Name ?? string.Empty));
    }
}

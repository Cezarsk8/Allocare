namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Providers;

public class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, Result<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProviderCommandHandler(
        IProviderRepository providerRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _providerRepository = providerRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProviderDto>> Handle(UpdateProviderCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ProviderDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ProviderDto>("You don't have access to this company.");

        var provider = await _providerRepository.GetByIdWithContactsAsync(command.ProviderId, cancellationToken);
        if (provider is null || provider.CompanyId != command.CompanyId)
            return Result.Failure<ProviderDto>("Provider not found.");

        // Check for duplicate name within company (excluding current)
        if (await _providerRepository.ExistsByNameInCompanyExcludingAsync(
                command.CompanyId, command.Request.Name, command.ProviderId, cancellationToken))
            return Result.Failure<ProviderDto>("A provider with this name already exists in this company.");

        if (!Enum.TryParse<ProviderCategory>(command.Request.Category, true, out var category))
            return Result.Failure<ProviderDto>("Invalid provider category.");

        provider.Update(
            command.Request.Name,
            category,
            command.Request.LegalName,
            command.Request.TaxId,
            command.Request.Website,
            command.Request.Description);

        await _providerRepository.UpdateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProviderDto(
            provider.Id,
            provider.CompanyId,
            provider.Name,
            provider.LegalName,
            provider.TaxId,
            provider.Category.ToString(),
            provider.Website,
            provider.Description,
            provider.IsActive,
            provider.CreatedAt,
            provider.UpdatedAt,
            provider.Contacts.Select(c => new ProviderContactDto(
                c.Id, c.Name, c.Email, c.Phone, c.Role, c.IsPrimary))
        ));
    }
}

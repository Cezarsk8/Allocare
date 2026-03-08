namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Providers;

public class CreateProviderCommandHandler : IRequestHandler<CreateProviderCommand, Result<ProviderDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProviderCommandHandler(
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

    public async Task<Result<ProviderDto>> Handle(CreateProviderCommand command, CancellationToken cancellationToken)
    {
        // Verify user has access to company
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ProviderDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ProviderDto>("You don't have access to this company.");

        // Check for duplicate name within company
        if (await _providerRepository.ExistsByNameInCompanyAsync(command.CompanyId, command.Request.Name, cancellationToken))
            return Result.Failure<ProviderDto>("A provider with this name already exists in this company.");

        // Parse category
        if (!Enum.TryParse<ProviderCategory>(command.Request.Category, true, out var category))
            return Result.Failure<ProviderDto>("Invalid provider category.");

        // Create provider
        var provider = Provider.Create(
            command.CompanyId,
            command.Request.Name,
            category,
            command.Request.LegalName,
            command.Request.TaxId,
            command.Request.Website,
            command.Request.Description);

        // Add contacts if provided
        if (command.Request.Contacts != null)
        {
            foreach (var contactReq in command.Request.Contacts)
            {
                var contact = ProviderContact.Create(
                    provider.Id,
                    contactReq.Name,
                    contactReq.Email,
                    contactReq.Phone,
                    contactReq.Role,
                    contactReq.IsPrimary);
                provider.AddContact(contact);
            }
        }

        await _providerRepository.AddAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(provider));
    }

    private static ProviderDto MapToDto(Provider provider) => new(
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
    );
}

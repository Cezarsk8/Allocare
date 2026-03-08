namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Providers.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Providers;

public class AddProviderContactCommandHandler : IRequestHandler<AddProviderContactCommand, Result<ProviderContactDto>>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AddProviderContactCommandHandler(
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

    public async Task<Result<ProviderContactDto>> Handle(AddProviderContactCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<ProviderContactDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<ProviderContactDto>("You don't have access to this company.");

        var provider = await _providerRepository.GetByIdWithContactsAsync(command.ProviderId, cancellationToken);
        if (provider is null || provider.CompanyId != command.CompanyId)
            return Result.Failure<ProviderContactDto>("Provider not found.");

        // If new contact is primary, unset existing primary
        if (command.Request.IsPrimary)
        {
            foreach (var existing in provider.Contacts.Where(c => c.IsPrimary))
            {
                existing.Update(existing.Name, existing.Email, existing.Phone, existing.Role, false);
            }
        }

        var contact = ProviderContact.Create(
            provider.Id,
            command.Request.Name,
            command.Request.Email,
            command.Request.Phone,
            command.Request.Role,
            command.Request.IsPrimary);

        provider.AddContact(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProviderContactDto(
            contact.Id, contact.Name, contact.Email, contact.Phone, contact.Role, contact.IsPrimary));
    }
}

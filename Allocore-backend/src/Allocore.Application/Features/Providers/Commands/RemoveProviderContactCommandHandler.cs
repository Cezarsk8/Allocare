namespace Allocore.Application.Features.Providers.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;

public class RemoveProviderContactCommandHandler : IRequestHandler<RemoveProviderContactCommand, Result>
{
    private readonly IProviderRepository _providerRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProviderContactCommandHandler(
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

    public async Task<Result> Handle(RemoveProviderContactCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure("You don't have access to this company.");

        var provider = await _providerRepository.GetByIdWithContactsAsync(command.ProviderId, cancellationToken);
        if (provider is null || provider.CompanyId != command.CompanyId)
            return Result.Failure("Provider not found.");

        var contact = provider.Contacts.FirstOrDefault(c => c.Id == command.ContactId);
        if (contact is null)
            return Result.Failure("Contact not found.");

        provider.RemoveContact(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

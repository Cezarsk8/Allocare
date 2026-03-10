namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Notes;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Result<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNoteCommandHandler(
        INoteRepository noteRepository,
        IProviderRepository providerRepository,
        IContractRepository contractRepository,
        IUserRepository userRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _providerRepository = providerRepository;
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NoteDto>> Handle(CreateNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<NoteDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<NoteDto>("You don't have access to this company.");

        // Verify the target entity exists and belongs to the company
        var entityValidation = await ValidateEntityAsync(command.EntityType, command.EntityId, command.CompanyId, cancellationToken);
        if (!entityValidation.IsSuccess)
            return Result.Failure<NoteDto>(entityValidation.Error);

        // Parse category
        var category = NoteCategory.General;
        if (!string.IsNullOrEmpty(command.Request.Category) && !Enum.TryParse(command.Request.Category, true, out category))
            return Result.Failure<NoteDto>("Invalid note category.");

        var note = Note.Create(
            command.CompanyId,
            command.EntityType,
            command.EntityId,
            _currentUser.UserId.Value,
            command.Request.Content,
            category,
            command.Request.IsPinned,
            command.Request.ReminderDate);

        await _noteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Resolve author name
        var user = await _userRepository.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
        var authorName = user?.FullName ?? "Unknown User";

        return Result.Success(NoteMapper.ToDto(note, authorName));
    }

    private async Task<Result> ValidateEntityAsync(NoteEntityType entityType, Guid entityId, Guid companyId, CancellationToken cancellationToken)
    {
        switch (entityType)
        {
            case NoteEntityType.Provider:
                var provider = await _providerRepository.GetByIdAsync(entityId, cancellationToken);
                if (provider == null || provider.CompanyId != companyId)
                    return Result.Failure("Provider not found in this company.");
                break;

            case NoteEntityType.Contract:
                var contract = await _contractRepository.GetByIdAsync(entityId, cancellationToken);
                if (contract == null || contract.CompanyId != companyId)
                    return Result.Failure("Contract not found in this company.");
                break;

            default:
                return Result.Failure("Unsupported entity type.");
        }

        return Result.Success();
    }
}

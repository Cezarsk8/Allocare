namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;

public class TogglePinNoteCommandHandler : IRequestHandler<TogglePinNoteCommand, Result>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public TogglePinNoteCommandHandler(
        INoteRepository noteRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(TogglePinNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure("You don't have access to this company.");

        var note = await _noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note == null || note.CompanyId != command.CompanyId)
            return Result.Failure("Note not found.");

        if (note.IsPinned)
            note.Unpin();
        else
            note.Pin();

        await _noteRepository.UpdateAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Domain.Common;

public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Result>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNoteCommandHandler(
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

    public async Task<Result> Handle(DeleteNoteCommand command, CancellationToken cancellationToken)
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

        // Only author or Admin can delete
        if (note.AuthorUserId != _currentUser.UserId.Value && !_currentUser.Roles.Contains("Admin"))
            return Result.Failure("You can only delete your own notes.");

        await _noteRepository.DeleteAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

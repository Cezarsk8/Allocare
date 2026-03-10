namespace Allocore.Application.Features.Notes.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Common;
using Allocore.Domain.Entities.Notes;

public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand, Result<NoteDto>>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNoteCommandHandler(
        INoteRepository noteRepository,
        IUserRepository userRepository,
        IUserCompanyRepository userCompanyRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _userRepository = userRepository;
        _userCompanyRepository = userCompanyRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NoteDto>> Handle(UpdateNoteCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return Result.Failure<NoteDto>("User not authenticated.");

        var hasAccess = await _userCompanyRepository.UserHasAccessToCompanyAsync(
            _currentUser.UserId.Value, command.CompanyId, cancellationToken);
        if (!hasAccess)
            return Result.Failure<NoteDto>("You don't have access to this company.");

        var note = await _noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note == null || note.CompanyId != command.CompanyId)
            return Result.Failure<NoteDto>("Note not found.");

        // Only author or Admin can update
        if (note.AuthorUserId != _currentUser.UserId.Value && !_currentUser.Roles.Contains("Admin"))
            return Result.Failure<NoteDto>("You can only edit your own notes.");

        var category = NoteCategory.General;
        if (!string.IsNullOrEmpty(command.Request.Category) && !Enum.TryParse(command.Request.Category, true, out category))
            return Result.Failure<NoteDto>("Invalid note category.");

        note.Update(command.Request.Content, category, command.Request.IsPinned, command.Request.ReminderDate);

        await _noteRepository.UpdateAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _userRepository.GetByIdAsync(note.AuthorUserId, cancellationToken);
        var authorName = user?.FullName ?? "Unknown User";

        return Result.Success(NoteMapper.ToDto(note, authorName));
    }
}

namespace Allocore.Application.Features.Notes.Validators;

using FluentValidation;
using Allocore.Application.Features.Notes.DTOs;
using Allocore.Domain.Entities.Notes;

public class UpdateNoteRequestValidator : AbstractValidator<UpdateNoteRequest>
{
    public UpdateNoteRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Note content is required")
            .MaximumLength(10000).WithMessage("Note content must not exceed 10,000 characters");

        RuleFor(x => x.Category)
            .Must(c => c == null || Enum.TryParse<NoteCategory>(c, true, out _))
            .WithMessage("Category must be one of: General, Negotiation, Meeting, Decision, Reminder, Issue, FollowUp, PhoneCall, Email, InternalDiscussion");

        RuleFor(x => x.ReminderDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Reminder date must be in the future")
            .When(x => x.ReminderDate.HasValue);
    }
}

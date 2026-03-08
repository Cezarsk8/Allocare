namespace Allocore.Application.Features.Users.Commands;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Features.Users.DTOs;
using Allocore.Domain.Common;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UserDto>("User not found");
        }

        user.UpdateProfile(
            command.Request.FirstName,
            command.Request.LastName,
            command.Request.Locale
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.IsEmailVerified,
            user.IsActive,
            user.Locale.Value,
            user.CreatedAt
        ));
    }
}

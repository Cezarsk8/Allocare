namespace Allocore.Application.Features.Users.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Features.Users.DTOs;
using Allocore.Domain.Common;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UserDto>("User not found");
        }

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

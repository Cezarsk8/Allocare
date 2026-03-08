namespace Allocore.Application.Features.Users.Queries;

using MediatR;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Features.Users.DTOs;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var userDtos = users.Select(user => new UserDto(
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

        return new PagedResult<UserDto>(userDtos, totalCount, request.Page, request.PageSize);
    }
}

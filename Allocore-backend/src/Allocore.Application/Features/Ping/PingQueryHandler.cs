namespace Allocore.Application.Features.Ping;

using MediatR;

public class PingQueryHandler : IRequestHandler<PingQuery, PingResponse>
{
    public Task<PingResponse> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PingResponse("pong", DateTime.UtcNow));
    }
}

namespace Allocore.Application.Features.Ping;

using MediatR;

public record PingQuery : IRequest<PingResponse>;

public record PingResponse(string Message, DateTime Timestamp);

namespace Allocore.Infrastructure.Services;

using Allocore.Application.Abstractions.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}

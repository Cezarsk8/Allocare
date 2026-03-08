namespace Allocore.Infrastructure.Persistence.InMemory;

using Allocore.Application.Abstractions.Persistence;
using Allocore.Domain.Common;
using System.Collections.Concurrent;

public class InMemoryRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : Entity
{
    private readonly ConcurrentDictionary<Guid, T> _store = new();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<T>>(_store.Values.ToList());
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(entity.Id, out _);
        return Task.CompletedTask;
    }
}

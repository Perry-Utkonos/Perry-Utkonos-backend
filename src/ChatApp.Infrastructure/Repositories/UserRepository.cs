namespace ChatApp.Infrastructure.Repositories;

using ChatApp.Application.Abstractions;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly ChatDbContext _db;
    public UserRepository(ChatDbContext db) => _db = db;

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
    {
        var v = username.Trim().ToLowerInvariant();
        return _db.Users.AnyAsync(u => u.Username.ToLower() == v, ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var v = email.Trim().ToLowerInvariant();
        return _db.Users.AnyAsync(u => u.Email.ToLower() == v, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

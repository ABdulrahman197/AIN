using AIN.Application.Interfaces.IRepos;
using AIN.Core.Entites;
using AIN.Infrastructrue.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Infrastructrue.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly AinDbContext _db;
        public UserRepo(AinDbContext db) { _db = db; }

        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct) => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken ct) => _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        public Task<List<UserAccount>> GetAllAsync(CancellationToken ct) => _db.Users.ToListAsync(ct);
        public Task AddAsync(UserAccount user, CancellationToken ct) => _db.Users.AddAsync(user, ct).AsTask();
        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
        public async Task UpdateAsync(UserAccount user, CancellationToken ct)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync(ct);
        }

    }
}

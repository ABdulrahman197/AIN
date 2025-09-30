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
    public class AuthorityRepo : IAuthorityRepo
    {
        private readonly AinDbContext _db;
        public AuthorityRepo(AinDbContext db) { _db = db; }

        public Task<Authority?> GetByNameAsync(string name, CancellationToken ct) => _db.Authorities.OrderBy(a => a.Name).FirstOrDefaultAsync(a => a.Name == name, ct);
        public Task<bool> AnyAsync(CancellationToken ct) => _db.Authorities.AnyAsync(ct);
        public Task AddRangeAsync(IEnumerable<Authority> authorities, CancellationToken ct) { _db.Authorities.AddRange(authorities); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
        public async Task<IReadOnlyList<Authority>> ListAsync(CancellationToken ct) => await _db.Authorities.AsNoTracking().OrderBy(a => a.Name).ToListAsync(ct);
    }
}

using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IRepos
{
    public interface IAuthorityRepo
    {
        Task<Authority?> GetByNameAsync(string name, CancellationToken ct);
        Task<bool> AnyAsync(CancellationToken ct);
        Task AddRangeAsync(IEnumerable<Authority> authorities, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        Task<IReadOnlyList<Authority>> ListAsync(CancellationToken ct);
    }
}

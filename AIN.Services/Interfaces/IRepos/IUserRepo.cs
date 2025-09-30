using AIN.Core.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIN.Application.Interfaces.IRepos
{
    public interface IUserRepo
    {
        Task UpdateAsync(AIN.Core.Entites.UserAccount user, CancellationToken ct);

        Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct);   
        Task<UserAccount?> GetByEmailAsync(string email, CancellationToken ct);
        Task<List<UserAccount>> GetAllAsync(CancellationToken ct);
        Task AddAsync(UserAccount user, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}

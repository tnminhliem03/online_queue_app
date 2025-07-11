using Microsoft.EntityFrameworkCore;
using OnlineQueueAPI.Data;
using OnlineQueueAPI.Models;

namespace OnlineQueueAPI.DL.AccountDL
{
    public class AccountDL : BaseDL<User>, IAccountDL
    {
        private readonly OnlineQueueDbContext _dbContext;

        public AccountDL(OnlineQueueDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetByUserId(Guid userId)
        {
            return _dbContext.Users
                .Include(u => u.Appointments)
                    !.ThenInclude(q => q.Queue)
                        !.ThenInclude(s => s!.Service)
                            !.ThenInclude(o => o!.Organization)
                .Include(u => u.UserOrganizationRoles)
                    !.ThenInclude(uor => uor.Organization)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<User?> GetByPhoneNumber(string phoneNumber)
        {
            return _dbContext.Users
                .Include(u => u.Appointments)
                    !.ThenInclude(q => q.Queue)
                        !.ThenInclude(s => s!.Service)
                            !.ThenInclude(o => o!.Organization)
                .Include(u => u.UserOrganizationRoles)
                    !.ThenInclude(uor => uor.Organization)
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
    }
}

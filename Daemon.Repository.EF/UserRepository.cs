namespace Daemon.Repository.EF
{
    using Daemon.Infrustructure.EF;
    using Daemon.Repository.Contract;
    using Daemon.Model;
    using System;
    using System.Linq.Expressions;
    public class UserRepository : Repository<User>, IUserRepository
    {
        protected override Expression<Func<User, int>> PrimaryKeyExpression => r => r.Id;

        public UserRepository(ApiDBContent context) : base(context)
        {
        }
    }
}


using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
namespace Daemon.Repository.EF
{
    public class OrderRepository: Repository<Order>, IOrderRepository
    {
        protected override Expression<Func<Order, int>> PrimaryKeyExpression => r => r.Id;

        public OrderRepository(ApiDBContent context) : base(context)
        {
        }
    }
}

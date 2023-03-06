using System.Linq;
using Daemon.Common;
using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Daemon.Common.Exceptions;
using System.Collections.Generic;
using Daemon.Repository.EF.Enums;
using Daemon.Common.Helpers;
namespace Daemon.Repository.EF
{
    public class CustomerRepository: Repository<Order>, IOrderRepository
    {
        protected override Expression<Func<Order, int>> PrimaryKeyExpression => r => r.Id;

        public CustomerRepository(ApiDBContent context) : base(context)
        {
        }
    }
}

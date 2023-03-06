namespace Daemon.Repository.EF
{
    using Daemon.Infrustructure.EF;
    using Daemon.Repository.Contract;
    using Daemon.Model;
    using System;
    using System.Linq.Expressions;
    public class RecordIDFilterRepository: Repository<RecordIDFilter>, IRecordIDFilterRepository
    {
        protected override Expression<Func<RecordIDFilter, int>> PrimaryKeyExpression => r => r.Id;

        public RecordIDFilterRepository(ApiDBContent context) : base(context)
        {
        }
    }
}

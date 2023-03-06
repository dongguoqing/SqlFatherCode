namespace Daemon.Repository.EF
{
    using Daemon.Repository.Contract;
    using Daemon.Infrustructure.EF;
    using Daemon.Model;
    using System.Linq.Expressions;
    using System;
    public class ClientConfigRepository: BaseSingleTableRepository<ClientConfig,short>, IClientConfigRepository
    {
        protected override Expression<Func<ClientConfig, short>> PrimaryKeyExpression => r => r.Id;
        public ClientConfigRepository(ApiDBContent context) : base(context)
        {

        }


        public override ClientConfig Find()
        {
            ClientConfig entity = base.Find();
            if (string.IsNullOrEmpty(entity.TimeZone))
            {
                throw new InvalidOperationException("Found ClientConfig with null TimeZone.");
            }

            return entity;
        }

        protected override ClientConfig GetDefaultEntity()
        {
            return null;
        }
    }
}

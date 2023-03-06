using Daemon.Infrustructure.EF;
using Daemon.Repository.Contract;
using Daemon.Model;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Daemon.Common.Helpers;
using Daemon.Repository.EF.Enums;
using Daemon.Infrustructure.Contract;
namespace Daemon.Repository.EF
{
	public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
	{
		protected override Expression<Func<RefreshToken, int>> PrimaryKeyExpression => null;
		public RefreshTokenRepository(ApiDBContent context) : base(context)
		{

		}
	}
}

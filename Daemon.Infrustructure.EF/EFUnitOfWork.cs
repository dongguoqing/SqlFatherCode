using Daemon.Infrustructure.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data.Common;

namespace Daemon.Infrustructure.EF
{
    /// <summary>
    /// 用于创建唯一上下文 保证线程内唯一
    /// </summary>
    public class EFUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DbContext _context;
        private DbTransaction dbTransaction = null;

        public EFUnitOfWork(DbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// 执行事务
        /// </summary>
        /// <returns></returns>
        public IUnitOfWork BeginTransaction()
        {
            _context.Database.BeginTransaction();
            dbTransaction = _context.Database.CurrentTransaction.GetDbTransaction();
            return this;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            _context.SaveChanges();
            _context.Database.CommitTransaction();
        }


        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (dbTransaction != null)
            {
                this.dbTransaction.Dispose();
            }
            this._context.Dispose();
        }

        /// <summary>
        /// 事务回滚
        /// </summary>
        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }
    }
}

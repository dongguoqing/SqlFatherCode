using Daemon.Repository.Contract;
using Daemon.Service.Contract;
using Daemon.Common.Exceptions;
using Daemon.Common.Enums;
using Daemon.Common.SqlParser.Schema;
using Daemon.Common.SqlParser;
using Daemon.Model;
namespace Daemon.Service
{
    public class DictService : IDictService
    {
        private readonly IDictRepository _dictRepository;
        public DictService(IDictRepository dictRepository)
        {
            _dictRepository = dictRepository;
        }

        public Dict FindById(int id)
        {
            return _dictRepository.FindById(id);
        }
    }
}
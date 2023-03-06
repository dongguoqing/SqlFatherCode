using System.CodeDom.Compiler;
namespace Daemon.Controllers.V2
{
    using Microsoft.AspNetCore.Mvc;
    using Daemon.Model;
    using Daemon.Model.ViewModel;
    using System.Collections.Generic;
    using System.Linq;
    [ApiController]
    [Route("v2/blueearth/table")]
    public class DbTableController
    {
        private readonly ApiDBContent _context;
        public DbTableController(ApiDBContent context)
        {
            _context = context;
        }

        // [Route(nameof(GetTable))]
        // public List<TableInfo> GetTable()
        // {
        //     var tables = _context.Model.
        // }

    }
}

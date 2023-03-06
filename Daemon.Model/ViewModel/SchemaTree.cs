using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Model.ViewModel
{
    public class SchemaTree
    {
        public string NodeName { get; set; }

        public int Level { get; set; }

        public string Code { get; set; }

        public List<SchemaTree> Children { get; set; }
    }
}
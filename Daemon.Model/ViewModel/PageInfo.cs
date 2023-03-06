using System;
namespace Daemon.Model.ViewModel
{
    using System.Collections.Generic;
    public class PageInfo
    {
        public IEnumerable<Object> Records { get; set; }

        public int Size { get; set; }

        public int? Total { get; set; }

        public int Current { get; set; }

        public int Pages { get; set; }
    }
}

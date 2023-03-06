using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Model.ViewModel
{
    public class RequestDownloadModel
    {
        public string FileStr { get; set; }

        public string FileName { get; set; }

        public FileType FileType { get; set; }
    }

    public enum FileType
    {
        Service,
        IService,
        Repositpry,
        IRepository,
        Controller
    }
}
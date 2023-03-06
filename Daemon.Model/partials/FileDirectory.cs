using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    public partial class FileDirectory
    {
        [NotMapped]
        public List<FileDirectory> Children { get; set; }
    }
}

namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;
    public partial class BlogRole
    {
        [NotMapped]
        public List<BlogRoleResource> ResourceList { get; set; }
        [NotMapped]
        public List<int> IdList { get; set; }
    }
}

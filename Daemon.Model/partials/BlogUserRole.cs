namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Collections.Generic;
    public partial class BlogUserRole
    {
        [NotMapped]
        public List<int> RoleList{get;set;}
    }
}

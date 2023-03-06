namespace Daemon.Model
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    public partial class Resource
    {
        [NotMapped]
        public List<Resource> Children { get; set; }

        /// <summary>
        /// 父级id
        /// </summary>
        /// <value></value>
        [NotMapped]
        public int? Pid { get; set; }

        [NotMapped]
        public int RoleResourceId{get;set;}

        [NotMapped]
        public string ResourceTypeName { get; set; }

        /// <summary>
        /// 父级菜单名称
        /// </summary>
        /// <value></value>
        [NotMapped]
        public string PName { get; set; }
    }
}

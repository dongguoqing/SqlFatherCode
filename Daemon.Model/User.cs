using System;
using System.Collections.Generic;
using Daemon.Model.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable

namespace Daemon.Model
{
    [Table("user")]
    public partial class User 
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserAccount { get; set; }
        public string UserAvatar { get; set; }
        public sbyte? Gender { get; set; }
        public string UserRole { get; set; }
        public string UserPassword { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public sbyte IsDelete { get; set; }
    }
}

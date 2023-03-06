using System.ComponentModel.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace Daemon.Model
{
    public class UserExt
    {

        [Column("AddUserId")]
        public int AddUserId { get; set; }

        [Column("UpdateUserId")]
        public int UpdateUserId { get; set; }

        [ForeignKey("AddUserId")]
        public BlogUser AddUser { get; set; }

    }
}

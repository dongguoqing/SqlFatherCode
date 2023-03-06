using System.ComponentModel.DataAnnotations.Schema;
using System;
namespace Daemon.Model
{
    public class BaseModel
    {
        [Column("AddTime")]
        public DateTime? AddTime { get; set; }

        [Column("UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        [Column("AddUserId")]
        public int AddUserId { get; set; }

        [Column("UpdateUserId")]
        public int UpdateUserId { get; set; }

        [ForeignKey("AddUserId")]
        public BlogUser AddUser { get; set; }
        [ForeignKey("UpdateUserId")]
        public BlogUser UpdateUser { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    [Table("table_info")]
    public partial class TableInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int ReviewStatus { get; set; }
        public string ReviewMessage { get; set; }
        public long UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public sbyte IsDelete { get; set; }
        public int? IsPublic { get; set; }
    }
}

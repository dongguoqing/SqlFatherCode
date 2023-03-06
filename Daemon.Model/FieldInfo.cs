namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System;
    [Table("field_info")]
    public partial class FieldInfo
    {
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("fieldName")]
        public string FieldName { get; set; }

        [Column("reviewStatus")]
        public int ReviewStatus { get; set; }

        public string ReviewMessage { get; set; }
        [Column("createTime")]
        public DateTime CreateTime { get; set; }

        [Column("updateTime")]
        public DateTime UpdateTime { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("isDelete")]
        public bool IsDelete { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("isPublic")]
        public int? IsPublic { get; set; }
    }
}

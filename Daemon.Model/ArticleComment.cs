using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
namespace Daemon.Model
{
    [Table("articlecomment")]
    public partial class ArticleComment:BaseModel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        /// <value></value>
        [Column("Comment")]
        public string Comment { get; set; }

        /// <summary>
        /// 文章id
        /// </summary>
        /// <value></value>
        [Column("ArticleId")]
        public int ArticleId { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        /// <value></value>
        [Column("AdmireCount")]
        public int AdmireCount { get; set; }
        /// <summary>
        /// 评论回复谁
        /// </summary>
        /// <value></value>
        [Column("ToId")]
        public int? ToId { get; set; }
        /// <summary>
        /// 当前评论回复的对应评论的主键Id
        /// </summary>
        /// <value></value>
        [Column("CommentId")]
        public int? CommentId { get; set; }
        /// <summary>
        /// 是否有回复
        /// </summary>
        /// <value></value>
        public bool HasReply{get;set;}
    }
}

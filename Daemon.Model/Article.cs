using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Daemon.Model
{
    [Table("article")]
    public partial class Article : BaseModel
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Content")]
        public string Content { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Column("AdmireCount")]
        public int AdmireCount { get; set; }

        [Column("IsDelete")]
        public int IsDelete { get; set; }

        [Column("State")]
        public int State { get; set; }
        /// <summary>
        /// 阅读数
        /// </summary>
        /// <value></value>

        [Column("ReadCount")]
        public int ReadCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        /// <value></value>
        [Column("ConCount")]
        public int ConCount{get;set;}
        /// <summary>
        /// 标签
        /// </summary>
        /// <value></value>
        [Column("Tag")]
        public string Tag{get;set;}
        /// <summary>
        /// 好文
        /// </summary>
        /// <value></value>

        [Column("IsGood")]
        public int IsGood { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        /// <value></value>

        [Column("Summary")]
        public string Summary { get; set; }
        /// <summary>
        /// 官方
        /// </summary>
        /// <value></value>

        [Column("IsOfficial")]
        public int IsOfficial { get; set; }
        /// <summary>
        /// 推荐
        /// </summary>
        /// <value></value>

        [Column("IsRecommend")]
        public int IsRecommend { get; set; }

        [Column("LanguageTypeId")]
        public int? LanguageTypeId { get; set; }
        [Column("Avater")]
        public string Avater { get; set; }
        [ForeignKey("LanguageTypeId")]
        public BlogDictionary Dic{get;set;}
    }
}

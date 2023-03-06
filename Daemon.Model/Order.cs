using System;
namespace Daemon.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("order")]
    public class Order
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get;set;}

        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public string CustomerName { get; set; }

        public string DemandTheme { get; set; }

        public int DemandCount { get; set; }

        public int OrderStatus { get; set; }
        public decimal Balance { get; set; }
    }
}

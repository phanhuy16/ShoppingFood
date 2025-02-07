using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models
{
    public class OrderModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string OrderCode { get; set; }

        public string UserName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }
    }
}

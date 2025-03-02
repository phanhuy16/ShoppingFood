using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class MenuModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
        [Required]
        public int Position { get; set; }
        [Required]
        public string Controller { get; set; }
        [Required]
        public string Action { get; set; }
        public string Url { get; set; }
    }
}

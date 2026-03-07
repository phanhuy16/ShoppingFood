using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShoppingFood.Models
{
    public abstract class CommonAbstract
    {
        [ValidateNever]
        public string CreatedBy { get; set; } = null!;
        [ValidateNever]
        public DateTime CreatedDate { get; set; }
        [ValidateNever]
        public DateTime ModifierDate { get; set; }
        [ValidateNever]
        public string ModifierBy { get; set; } = null!;
    }
}

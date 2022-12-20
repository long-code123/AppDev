using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AppDev.Models
{
    public class RequestCategory
    {
        public int Id { get; set; }

        public string StoreOwnerId { get; set; } = null!;
        
        [ValidateNever]
        [Display(Name = "Store Owner")]
        public ApplicationUser StoreOwner { get; set; } = null!;

        public string Name { get; set; } = null!;

        [Display(Name = "Is Appproved")]    
        public bool? IsAppproved { get; set; } = null;

        [StringLength(255)]
        public string Message { get; set; } = "";

        public bool IsEditable(string storeOwnerId)
        {
            if (storeOwnerId != this.StoreOwnerId)
                return false;
            if (IsAppproved != null)
                return false;
            return true;
        }

        public bool IsApprovable()
        {
            if (IsAppproved == true)
                return false;
            return true;
        }
    }
}

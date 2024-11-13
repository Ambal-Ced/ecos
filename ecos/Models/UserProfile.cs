using ecos.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace ecos.Models
{
    public class UserProfile
    {
        [Key]
        public string UniqueUserName { get; set; }

        // Foreign key reference to ApplicationUser
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime Month { get; set; }
    }
}

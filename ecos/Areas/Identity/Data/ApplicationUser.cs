using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ecos.Areas.Identity.Data;

// Add profile data for application users by adding properties to the AppUser class
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Type { get; set; }
    public DateTime DateTime { get; set; }
}


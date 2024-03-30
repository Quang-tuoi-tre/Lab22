using Microsoft.AspNetCore.Identity;

namespace Lab22.DataAccess
{
    public class ApplicationUser:IdentityUser
    {
        
        public string FullName { get; set; }      
       
        public string Address { get; set; }

        public int age { get; set; }
    }
}

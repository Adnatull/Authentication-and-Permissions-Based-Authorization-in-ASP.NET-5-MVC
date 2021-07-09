using Identity.Models;

namespace Identity.Constants
{
    public class DefaultApplicationUsers
    {
        public static AppUser GetSuperUser()
        {
            var defaultUser = new AppUser
            {
                Id = "39c64ad6-39ae-4de0-a0b2-9298a46b4b4c",
                UserName = "SuperAdmin",
                Email = "a2masum@yahoo.com",
                FirstName = "Al",
                LastName = "Masum",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };
            return defaultUser;
        }
    }
}

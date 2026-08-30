using Andrej_Kolega_IIS.Shared.Entities;
using Microsoft.AspNetCore.Identity;

namespace Andrej_Kolega_IIS.Shared.Data
{
    public static class DbSeeder
    {
        public static void SeedUsers(AppDbContext context)
        {
            if (context.Users.Any())
            {
                return;
            }

            var hasher = new PasswordHasher<User>();

            var readOnlyUser = new User
            {
                Username = "readonly",
                Role = UserRole.ReadOnly
            };
            readOnlyUser.PasswordHash = hasher.HashPassword(readOnlyUser, "readonly123");

            var fullAccessUser = new User
            {
                Username = "fulladmin",
                Role = UserRole.FullAccess
            };
            fullAccessUser.PasswordHash = hasher.HashPassword(fullAccessUser, "fulladmin123");

            context.Users.AddRange(readOnlyUser, fullAccessUser);
            context.SaveChanges();
        }
    }
}

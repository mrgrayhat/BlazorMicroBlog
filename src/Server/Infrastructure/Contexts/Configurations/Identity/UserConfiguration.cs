using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Server.Infrastructure.Contexts.Configurations.Identity
{
    /// <summary>
    /// configure Identity Users and seed default users.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<IdentityUser>
    {
        public void Configure(EntityTypeBuilder<IdentityUser> builder)
        {
            //a hasher to hash the password before seeding the user to the db
            var hasher = new PasswordHasher<IdentityUser>();
            builder.HasData(
                new IdentityUser // default admin user
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", // primary key
                    UserName = "admin",
                    NormalizedUserName = "Admin".ToUpper(),
                    Email = "admin@microblog.com",
                    NormalizedEmail = "ADMIN@MICROBLOG.COM".ToUpper(),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    PasswordHash = hasher.HashPassword(null, "Pa$$w0rd")
                },
                new IdentityUser // default normal user
                {
                    Id = "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                    UserName = "user1",
                    NormalizedUserName = "USER1".ToUpper(),
                    Email = "user1@microblog.com",
                    NormalizedEmail = "USER1@MICROBLOG.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    PasswordHash = hasher.HashPassword(null, "user1")
                });

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroBlog.Server.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Server.Infrastructure.Contexts.Configurations.Identity
{
    /// <summary>
    /// configure Identity Users and seed default users.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<UserInfo>
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            //a hasher to hash the password before seeding the user to the db
            var hasher = new PasswordHasher<UserInfo>();
            builder.HasData(
                new UserInfo // default admin user
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", // primary key
                    Avatar = @"\site\avatars\male2_big.png",
                    UserName = "admin",
                    NormalizedUserName = "Admin".ToUpper(),
                    Email = "admin@microblog.com",
                    NormalizedEmail = "ADMIN@MICROBLOG.COM".ToUpper(),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    PasswordHash = hasher.HashPassword(null, "Pa$$w0rd"),
                    Country = "USA",
                    LocaleCulture = "en-US",
                    RegisterDate = DateTime.Now,
                    Sex = "male",
                    Age = 25,
                    Bio = @"i'm <b> saeed </b>, micro blog administrator. you are reading my bio."
                },
                new UserInfo // default normal user
                {
                    Id = "b8633e2d-a33b-45e6-8329-1958b3252bbd",
                    UserName = "user1",
                    Avatar = @"\site\avatars\user1_64.png",
                    NormalizedUserName = "USER1".ToUpper(),
                    Email = "user1@microblog.com",
                    NormalizedEmail = "USER1@MICROBLOG.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    PasswordHash = hasher.HashPassword(null, "user1"),
                    Country = "IR",
                    LocaleCulture = "fa-IR",
                    RegisterDate = DateTime.Now,
                    Sex = "female",
                    Age = 18,
                    Bio = @"i'm <b> User1 </b>, a normal user in micro blog. you are reading my bio."
                });

        }
    }
}

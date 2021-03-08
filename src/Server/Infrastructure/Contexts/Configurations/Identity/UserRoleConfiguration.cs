using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Server.Infrastructure.Contexts.Configurations.Identity
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(
                new IdentityUserRole<string> // assign Default Admin to Admins Role
                {
                    RoleId = "363bd3e0-10d5-4806-83a5-e4f4acce7e76",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
                },
                new IdentityUserRole<string> // assign Default User to Users Role
                {
                    RoleId = "830770c2-1efa-445d-bbe7-3d8cec0e22f5",
                    UserId = "b8633e2d-a33b-45e6-8329-1958b3252bbd"
                });
        }
    }

}
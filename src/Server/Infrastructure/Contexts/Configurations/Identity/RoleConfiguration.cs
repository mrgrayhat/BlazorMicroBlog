using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroBlog.Server.Infrastructure.Contexts.Configurations.Identity
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Id = "830770c2-1efa-445d-bbe7-3d8cec0e22f5",
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole
                {
                    Id = "363bd3e0-10d5-4806-83a5-e4f4acce7e76",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
            );
        }
    }
}

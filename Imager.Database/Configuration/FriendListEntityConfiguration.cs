using Imager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Imager.Database.Configuration;

public class FriendListEntityConfiguration : IEntityTypeConfiguration<FriendListEntity>
{
    public void Configure(EntityTypeBuilder<FriendListEntity> builder)
    {
        builder.HasMany(f => f.List)
            .WithMany();
    }
}
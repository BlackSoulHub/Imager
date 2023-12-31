﻿using Imager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Imager.Database.Configuration;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Metadata.FindNavigation(nameof(UserEntity.Images))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(e => e.Friends)
            .WithOne(f => f.User);
    }
}
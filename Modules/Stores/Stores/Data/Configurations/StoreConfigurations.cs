using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stores.Stores.Models;

namespace Stores.Data.Configurations;

public class StoreConfigurations : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255).IsRequired();
        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(255).IsRequired();       
        builder.Property(x => x.CoverUrl).HasMaxLength(255);       

        builder.HasIndex(x => x.Slug).IsUnique();       
        builder.HasIndex(x => x.OwnerId);      
    }
}
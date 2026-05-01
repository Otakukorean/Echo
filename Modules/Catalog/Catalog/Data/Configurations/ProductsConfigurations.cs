using Catalog.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Data.Configurations;

public class ProductsConfigurations : IEntityTypeConfiguration<Products>
{
    public void Configure(EntityTypeBuilder<Products> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.Price).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(255);

        builder.HasIndex(x => x.Slug).IsUnique();
        
        builder.HasMany(x => x.Categories)
            .WithMany()
            .UsingEntity(j => j.ToTable("ProductCategories"));
        
        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(x => x.Categories).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Images).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

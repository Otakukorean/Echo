using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(255).IsRequired();
        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.Index).IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.Index });
        
    }
}

using Catalog.Catalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Data.Configurations;

public class VariationConfiguration : IEntityTypeConfiguration<Variation>
{
    public void Configure(EntityTypeBuilder<Variation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.Property(x => x.Url).HasMaxLength(255);
        builder.Property(x => x.Price).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.StoreId });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orders.Orders.Models;

namespace Orders.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CustomerEmail).HasMaxLength(255).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ShippingAddress).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Subtotal).IsRequired();
        builder.Property(x => x.Total).IsRequired();

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.StoreId);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnePass.Services.DataAccess
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("PRODUCT");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasColumnName("PRODUCT_ID");
            builder.Property(x => x.Name)
                .HasColumnName("NAME");
            builder.Property(x => x.Login)
                .HasColumnName("LOGIN");
            builder.Property(x => x.Password)
                .HasColumnName("PASSWORD");
        }
    }
}

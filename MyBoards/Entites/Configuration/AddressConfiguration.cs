using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entites.Configuration
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.OwnsOne(a => a.Coordinates, cmb =>
                {
                    cmb.Property(c => c.Latitude).HasPrecision(18, 7);
                    cmb.Property(c => c.Longitude).HasPrecision(18, 7);
                });
        }
    }
}
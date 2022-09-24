using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace MyBoards.Entites.Configuration
{
    public class EpicConfiguration : IEntityTypeConfiguration<Epic>
    {
        public void Configure(EntityTypeBuilder<Epic> builder)
        {
            builder.Property(endDate => endDate.EndDate).HasPrecision(3);
        }
    }
}
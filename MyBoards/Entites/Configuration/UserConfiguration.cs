using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entites.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasOne(user => user.Address)
            .WithOne(address => address.User)
            .HasForeignKey<Address>(a => a.UserId);

            //!index na jedne pole
            //builder.HasIndex(u => u.Email);
            //! index na wiele wlasciwosci
            builder.HasIndex(u => new { u.FullName, u.Email });
        }
    }
}
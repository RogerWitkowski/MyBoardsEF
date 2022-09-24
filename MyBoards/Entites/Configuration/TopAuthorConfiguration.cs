using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyBoards.Entites.ViewModels;
using System.Reflection.Emit;

namespace MyBoards.Entites.Configuration
{
    public class TopAuthorConfiguration : IEntityTypeConfiguration<TopAuthor>
    {
        public void Configure(EntityTypeBuilder<TopAuthor> builder)
        {
            builder.ToView("View_TopAuthors");
            builder.HasNoKey();
        }
    }
}
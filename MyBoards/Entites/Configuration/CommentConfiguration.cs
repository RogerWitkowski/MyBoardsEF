using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace MyBoards.Entites.Configuration
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.Property(createdDate => createdDate.CreatedDate).HasDefaultValueSql("getutcdate()");
            builder.Property(updatedDate => updatedDate.UpdatedDate).ValueGeneratedOnUpdate();

            builder.HasOne(author => author.Author)
                .WithMany(comments => comments.Comments)
                .HasForeignKey(comment => comment.AuthorId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
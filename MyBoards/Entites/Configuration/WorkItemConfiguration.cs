using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entites.Configuration
{
    public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
    {
        public void Configure(EntityTypeBuilder<WorkItem> builder)
        {
            builder.Property(area => area.Area).HasColumnType("varchar(200)");
            builder.Property(iterationPath => iterationPath.IterationPath).HasColumnName("Iteration_Path");

            builder.Property(priority => priority.Priority).HasDefaultValue(1);

            builder.HasMany(workItem => workItem.Comments).WithOne(comment => comment.WorkItem)
                .HasForeignKey(comment => comment.WorkItemId);

            builder.HasOne(workItem => workItem.Author).WithMany(user => user.WorkItems)
                .HasForeignKey(workItem => workItem.AuthorId);

            builder.HasMany(w => w.Tags).WithMany(t => t.WorkItems)
                .UsingEntity<WorkItemTag>(
                    workItemTag => workItemTag
                        .HasOne(wit => wit.Tag)
                        .WithMany()
                        .HasForeignKey(wit => wit.TagId),

                    workItemTag => workItemTag
                        .HasOne(wit => wit.WorkItem)
                        .WithMany()
                        .HasForeignKey(wit => wit.WorkItemId),

                    workItemTag =>
                    {
                        workItemTag.HasKey(key => new { key.TagId, key.WorkItemId });
                        workItemTag.Property(publicationDate => publicationDate.PublicationDate)
                            .HasDefaultValueSql("getutcdate()");
                    });

            builder.HasOne(workItem => workItem.State).WithMany().HasForeignKey(workItem => workItem.StateId);
        }
    }
}
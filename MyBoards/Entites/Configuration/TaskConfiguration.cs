using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace MyBoards.Entites.Configuration
{
    public class TaskConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.Property(activity => activity.Activity).HasMaxLength(200);
            builder.Property(remainingWork => remainingWork.RemaningWork).HasPrecision(14, 2);
        }
    }
}
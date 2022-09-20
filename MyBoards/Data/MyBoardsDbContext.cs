using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyBoards.Entites;
using Task = MyBoards.Entites.Task;

namespace MyBoards.Data
{
    public class MyBoardsDbContext : DbContext
    {
        public MyBoardsDbContext(DbContextOptions<MyBoardsDbContext> options) : base(options)
        {
        }

        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<Epic> Epics { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<WorkItemState> WorkItemStates { get; set; }

        //TODO: TWORZENIE  KLUCZA GLOWNEGO ZLOZONEGO Z 2 WLASCIWOSCI KLASY
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<User>()
        //        .HasKey(x => new { x.Email, x.LastName });
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Epic>(entBuilder =>
            {
                entBuilder.Property(endDate => endDate.EndDate).HasPrecision(3);
            });

            modelBuilder.Entity<Issue>(entBuilder =>
            {
                entBuilder.Property(effort => effort.Effort).HasColumnType("decimal(5,2)");
            });

            modelBuilder.Entity<Task>(entBuilder =>
            {
                entBuilder.Property(activity => activity.Activity).HasMaxLength(200);
                entBuilder.Property(remainingWork => remainingWork.RemainingWork).HasPrecision(14, 2);
            });

            //TODO: NAKLADANIE ATRYBUTOW DLA WLASCIWOSCI DANEGO OBIEKTU
            modelBuilder.Entity<WorkItem>(entBuild =>
            {
                entBuild.Property(area => area.Area).HasColumnType("varchar(200)");
                entBuild.Property(iterationPath => iterationPath.IterationPath).HasColumnName("Iteration_Path");

                entBuild.Property(priority => priority.Priority).HasDefaultValue(1);

                entBuild.HasMany(workItem => workItem.Comments).WithOne(comment => comment.WorkItem)
                    .HasForeignKey(comment => comment.WorkItemId);

                entBuild.HasOne(workItem => workItem.Author).WithMany(user => user.WorkItems)
                    .HasForeignKey(workItem => workItem.AuthorId);

                entBuild.HasMany(w => w.Tags).WithMany(t => t.WorkItems)
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

                entBuild.HasOne(workItem => workItem.State).WithMany().HasForeignKey(workItem => workItem.StateId);
            });

            modelBuilder.Entity<Comment>(entBuild =>
            {
                entBuild.Property(createdDate => createdDate.CreatedDate).HasDefaultValueSql("getutcdate()");
                entBuild.Property(updatedDate => updatedDate.UpdatedDate).ValueGeneratedOnUpdate();

                entBuild.HasOne(author => author.Author).WithMany(comments => comments.Comments)
                    .HasForeignKey(comment => comment.AuthorId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<User>()
                .HasOne(user => user.Address)
                .WithOne(address => address.User)
                .HasForeignKey<Address>(a => a.UserId);

            modelBuilder.Entity<WorkItemState>(entBuild =>
            {
                entBuild.Property(state => state.Value).IsRequired().HasMaxLength(60);
            });
        }
    }
}
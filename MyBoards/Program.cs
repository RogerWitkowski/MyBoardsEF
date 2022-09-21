using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using MyBoards.Data;
using MyBoards.Entites;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

//builder.Services.AddMvc().AddNewtonsoftJson();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MyBoardsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsSQLConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseAuthorization();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<MyBoardsDbContext>();

var pendingMigrations = dbContext.Database.GetPendingMigrations();
if (pendingMigrations.Any())
{
    dbContext.Database.Migrate();
}

var users = dbContext.Users.ToList();
if (!users.Any())
{
    var user1 = new User()
    {
        FullName = "User1 UserOne",
        Email = "email111@email.com",
        Address = new Address()
        {
            City = "Warszawa",
            Street = "Warszawska"
        }
    };
    var user2 = new User()
    {
        FullName = "User2 UserTwo",
        Email = "email222@email.com",
        Address = new Address()
        {
            City = "Chrz¹szczyrzewoszyce",
            Street = "£êko³ody"
        }
    };
    dbContext.Users.AddRange(user1, user2);
    dbContext.SaveChanges();
}

//var tags = dbContext.Tags.ToList();
//if (!tags.Any())
//{
//    var tag1 = new Tag()
//    {
//        Value = "Web"
//    };
//    var tag2 = new Tag()
//    {
//        Value = "UI"
//    };
//    var tag3 = new Tag()
//    {
//        Value = "Desktop"
//    };
//    var tag4 = new Tag()
//    {
//        Value = "API"
//    };
//    var tag5 = new Tag()
//    {
//        Value = "Service"
//    };

//    dbContext.Tags.AddRange(tag1, tag2, tag3, tag4, tag5);
//    dbContext.SaveChanges();
//}

app.MapGet("data", async (MyBoardsDbContext dbContext) =>
{
    //var tags = dbContext.Tags.ToList();
    //var epic = dbContext.Epics.First();
    //var user = dbContext.Users.FirstOrDefault(u => u.FirstName == "Stella Mann");
    //return new { epic, user };

    //var toDoWorkItems = dbContext.WorkItems.Where(i => i.StateId == 1).ToList();
    //return new { toDoWorkItems };

    //var newComments = await dbContext.Comments.Where(u => u.CreatedDate > new DateTime(2022, 7, 23)).ToListAsync();
    //return newComments;

    //var top5NewestComments = await dbContext.Comments
    //    .OrderByDescending(c => c.CreatedDate)
    //    .Take(5)
    //    .ToListAsync();
    //return new { top5NewestComments };

    //var stateCount = await dbContext.WorkItems
    //    .GroupBy(x => x.StateId)
    //    .Select(g =>
    //        new { stateId = g.Key, count = g.Count() })
    //    .ToListAsync();

    //return new { stateCount };

    //var onHoldEpics = await dbContext.Epics
    //    .Where(s => s.StateId == 4)
    //    .OrderBy(p => p.Priority)
    //    .ToListAsync();

    //return new { onHoldEpics };

    var authorCommentsCounts = await dbContext.Comments
        .GroupBy(a => a.AuthorId)
        .Select(g => new { g.Key, Count = g.Count() })
        .ToListAsync();

    var topAuthorComments = authorCommentsCounts
            .First(a => a.Count == authorCommentsCounts
            .Max(Comment => Comment.Count));

    var userDetails = dbContext.Users.First(u => u.Id == topAuthorComments.Key);

    return new { userDetails, commentCount = topAuthorComments.Count };
});

app.MapPost("update", async (MyBoardsDbContext dbContext) =>
{
    Epic epic = await dbContext.Epics.FirstAsync(epic => epic.Id == 1);

    //var onHoldState = await dbContext.WorkItemStates.FirstAsync(s => s.Value == "On Hold");

    //epic.StateId = onHoldState.Id;
    //await dbContext.SaveChangesAsync();

    var rejectedState = await dbContext.WorkItemStates.FirstAsync(s => s.Value == "Rejected");

    epic.State = rejectedState;
    await dbContext.SaveChangesAsync();

    //epic.Area = "Updated area";
    //epic.Priority = 1;
    //epic.StartDate = DateTime.Now;
    //epic.StateId = 1;

    //await dbContext.SaveChangesAsync();

    return epic;
});

app.MapPost("create", async (MyBoardsDbContext dbContext) =>
{
    //Tag mvcTag = new Tag()
    //{
    //    Value = "MVC"
    //};
    //Tag aspTag = new Tag()
    //{
    //    Value = "ASP"
    //};

    //var tags = new List<Tag>() { mvcTag, aspTag };

    //await dbContext.AddAsync(tag);
    //await dbContext.Tags.AddAsync(tag);
    //await dbContext.AddRangeAsync(tags);

    var address = new Address()
    {
        City = "Warszawa",
        Country = "Polska",
        Street = "Piwna",
    };

    var user = new User()
    {
        FullName = "Jan Kowalksi",
        Email = "jakKowalski@gmail.com",
        Address = address
    };

    dbContext.Users.Add(user);

    await dbContext.SaveChangesAsync();
    return user;
});

app.MapGet("userComments", async (MyBoardsDbContext dbContext) =>
{
    var user = await dbContext.Users
        .Include(c => c.Comments).ThenInclude(w => w.WorkItem)
        .Include(a => a.Address)
        .FirstAsync(u => u.Id == Guid.Parse("68366DBE-0809-490F-CC1D-08DA10AB0E61"));

    //var userComments = await dbContext.Comments.Where(c => c.AuthorId == user.Id).ToListAsync();

    return user;
});

app.MapDelete("delete", async (MyBoardsDbContext dbContext) =>
{
    var workItemTags = await dbContext.WorkItemTag.Where(i => i.WorkItemId == 12).ToListAsync();
    dbContext.WorkItemTag.RemoveRange(workItemTags);

    var workItem = await dbContext.WorkItems.FirstAsync(i => i.Id == 16);
    dbContext.RemoveRange(workItem);

    await dbContext.SaveChangesAsync();
});

app.MapDelete("deleteUser", async (MyBoardsDbContext dbContext) =>
{
    var user = await dbContext.Users
        .FirstAsync(i => i.Id == Guid.Parse("DC231ACF-AD3C-445D-CC08-08DA10AB0E61"));

    var userComments = await dbContext.Comments.Where(c => c.AuthorId == user.Id).ToListAsync();

    dbContext.RemoveRange(userComments);
    await dbContext.SaveChangesAsync();

    dbContext.Users.Remove(user);
    await dbContext.SaveChangesAsync();
});

app.MapDelete("deleteUserClientCascade", async (MyBoardsDbContext dbContext) =>
{
    var user = await dbContext.Users
        .Include(c => c.Comments)
        .FirstAsync(i => i.Id == Guid.Parse("4C081D84-C565-4327-CBCB-08DA10AB0E61"));

    dbContext.Remove(user);
    await dbContext.SaveChangesAsync();
});

app.Run();
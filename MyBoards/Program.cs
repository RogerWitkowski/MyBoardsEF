using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBoards.Data;
using MyBoards.DTO;
using MyBoards.Entites;
using MyBoards.Sieve;
using Newtonsoft.Json;
using Sieve.Models;
using Sieve.Services;
using Expression = Castle.DynamicProxy.Generators.Emitters.SimpleAST.Expression;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

//builder.Services.AddMvc().AddNewtonsoftJson();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MyBoardsDbContext>(options =>
{
    //options.UseLazyLoadingProxies();
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

//DataGenerator.Seed(dbContext);

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
    var withAddress = true;

    var user = await dbContext.Users
        //.Include(c => c.Comments).ThenInclude(w => w.WorkItem)
        //.Include(a => a.Address)
        .FirstAsync(u => u.Id == Guid.Parse("68366DBE-0809-490F-CC1D-08DA10AB0E61"));

    if (withAddress)
    {
        var result = new { FullName = user.FullName, Address = $"{user.Address.Street} {user.Address.City}" };
        return result;
    }

    //var userComments = await dbContext.Comments.Where(c => c.AuthorId == user.Id).ToListAsync();

    return new { FullName = user.FullName, Address = "-" };
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

app.MapGet("dataCheckTracking", async (MyBoardsDbContext dbContext) =>
{
    var user = await dbContext.Users
        .FirstAsync(i => i.Id == Guid.Parse("D00D8059-8977-4E5F-CBD2-08DA10AB0E61"));

    var trackedEntries1 = dbContext.ChangeTracker.Entries();

    user.Email = "test@gmail.com";

    var trackedEntries2 = dbContext.ChangeTracker.Entries();

    await dbContext.SaveChangesAsync();
    return user;
});

app.MapGet("dataCheckTrackingDel", async (MyBoardsDbContext dbContext) =>
{
    var user = await dbContext.Users
        .FirstAsync(u => u.Id == Guid.Parse("CA5E3E47-EC21-4DDB-CC1B-08DA10AB0E61"));

    dbContext.Users.Remove(user);

    var newUser = new User()
    {
        FullName = "New User"
    };

    await dbContext.Users.AddAsync(newUser);

    var trackedEntries3 = dbContext.ChangeTracker.Entries();

    await dbContext.SaveChangesAsync();
});

app.MapGet("removeWorkItemwithNoSelectQuery", async (MyBoardsDbContext dbContext) =>
{
    var workItem = new Epic()
    {
        Id = 2
    };

    var entry = dbContext.Attach(workItem);
    entry.State = EntityState.Deleted;

    await dbContext.SaveChangesAsync();

    return workItem;
});

app.MapGet("workItemStateAsNoTracking", async (MyBoardsDbContext dbContext) =>
{
    var workItemsState = await dbContext.WorkItemStates
        .AsNoTracking()
        .ToListAsync();

    var entries11 = dbContext.ChangeTracker.Entries();

    return workItemsState;
});

app.MapGet("rawSqlQuery", async (MyBoardsDbContext dbContext) =>
{
    var minWorkItemsCount = "85";

    var workItemStates = await dbContext.WorkItemStates
        .FromSqlInterpolated($@"
    SELECT wis.Id, wis.Value
    FROM [MyBoardEF_DB].[dbo].[WorkItemStates] wis
    JOIN WorkItems wi on wi.StateId = wis.Id
    GROUP BY wis.Id, wis.Value
    HAVING COUNT(*) > + {minWorkItemsCount}"
        )
        .ToListAsync();

    dbContext.Database.ExecuteSqlRaw(@"
UPDATE Comments
SET UpdatedDate = GETDATE()
WHERE AuthorId = '9A8E164A-F3C2-40C3-CBCD-08DA10AB0E61'
");

    return workItemStates;
});
app.MapGet("top5Authors", async (MyBoardsDbContext dbContext) =>
{
    var top5Authors = await dbContext.ViewTopAuthor.ToListAsync();
    return top5Authors;
});

app.MapGet("LatLong", async (MyBoardsDbContext dbContext) =>
{
    var LatFilter = dbContext.Addresses.Where(a => a.Coordinates.Latitude > 10);
});

app.MapGet("pagination", async (MyBoardsDbContext dbContext) =>
{
    // user input
    var filter = "a";
    string sortBy = "FullName";
    bool sortByDescending = false;
    int pageNumber = 1;
    int pageSize = 10;

    var query = dbContext.Users
        .Where(u => filter == null || (u.Email.ToLower().Contains(filter.ToLower()) ||
                                       (u.FullName.ToLower().Contains(filter.ToLower()))));

    var totalCount = query.Count();

    if (sortBy != null)
    {
        //Expression<Func<User, object>> sortByExpression = user => user.Email;
        var columnsSelector = new Dictionary<string, Expression<Func<User, object>>>
        {
            {nameof(User.Email), user => user.Email},
            {nameof(User.FullName), user => user.FullName }
        };

        var sortByExpression = columnsSelector[sortBy];

        query = sortByDescending
            ? query.OrderByDescending(sortByExpression)
            : query.OrderBy(sortByExpression);
    }

    var result = await query.Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToListAsync();

    var pagedResult = new PagedResult<User>(result, totalCount, pageSize, pageNumber);

    return pagedResult;
});

app.MapGet("dataSelectOperator", async (MyBoardsDbContext dbContext) =>
{
    var usersComments = await dbContext.Users
        .Include(a => a.Address)
        .Include(u => u.Comments)
        .Where(a => a.Address.Country == "Albania")
        .SelectMany(u => u.Comments)
        .Select(c => c.Message)
        //.Select(u => u.FullName)
        .ToListAsync();

    //var comments = users.SelectMany(u => u.comments).Select(c => c.Message);

    return usersComments;
});

app.MapGet("dataLazyLoading", async (MyBoardsDbContext dbContext) =>
{
    var users = await dbContext.Users
        .Where(u => u.Address.Country == "Albania")
        //.Include(u => u.Comments)
        .ToListAsync();

    foreach (var user in users)
    {
        foreach (var comment in user.Comments)
        {
        }
    }
});

app.MapPost("sieve", async ([FromBody] SieveModel query, ISieveProcessor sieveProcessor, MyBoardsDbContext dbContext) =>
{
    var epics = dbContext.Epics
        .Include(e => e.Author)
        .AsQueryable();

    var dtos = await sieveProcessor
        .Apply(query, epics)
        .Select(e => new EpicDto()
        {
            Id = e.Id,
            Area = e.Area,
            Priority = e.Priority,
            StartDate = e.StartDate,
            AuthorFullName = e.Author.FullName
        })
        .ToListAsync();

    var totalCount = await sieveProcessor
        .Apply(query, epics, applyPagination: false, applySorting: false)
        .CountAsync();

    var result = new PagedResult<EpicDto>(dtos, totalCount, query.Page.Value, query.PageSize.Value);

    return result;
});

app.Run();
using Microsoft.EntityFrameworkCore;
using MyBoards.Data;
using MyBoards.Entites;

var builder = WebApplication.CreateBuilder(args);

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
        FirstName = "User1",
        LastName = "UserOne",
        Email = "email111@email.com",
        Address = new Address()
        {
            City = "Warszawa",
            Street = "Warszawska"
        }
    };
    var user2 = new User()
    {
        FirstName = "User2",
        LastName = "UserTwo",
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

app.Run();
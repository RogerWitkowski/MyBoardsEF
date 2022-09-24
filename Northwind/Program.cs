using Microsoft.EntityFrameworkCore;
using Northwind.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<NorthwindContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwindSQLConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("data", async (NorthwindContext nDbContext) =>
{
    var sampleData = await nDbContext.Products
        .Take(100)
        .ToListAsync();
    return sampleData;
});

app.Run();
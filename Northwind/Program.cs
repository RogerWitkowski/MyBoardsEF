using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Northwind.Entities;
using System.Text.Json.Serialization;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.Configure<JsonOptions>(options =>
//{
//    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//    //options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
//});

builder.Services.AddDbContext<NorthwindContext>(options =>
{
    //options.UseLazyLoadingProxies();
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

app.MapGet("getOrderDetails", async (NorthwindContext dbContext) =>
{
    Order order = await GetOrder(46, dbContext, o => o.OrderDetails);
    return new { OrderId = order.OrderId, details = order.OrderDetails };
});

//app.MapGet("getOrderWithShipper", async (NorthwindContext dbContext) =>
//{
//    Order order = await GetOrder(46, dbContext, s => s.ShipViaNavigation);
//    return new { OrderId = order.OrderId, ShipVIa = order.ShipVia, Shipper = order.ShipViaNavigation };
//});

app.MapGet("getOrderWithShipper", async (NorthwindContext dbContext) =>
{
    Order order = await GetOrder(46, dbContext, s => s.ShipViaNavigation);
    return new { OrderId = order.OrderId, ShipVIa = order.ShipVia, Shipper = order.ShipViaNavigation };
});

app.MapGet("getOrderWithCustomer", async (NorthwindContext dbContext) =>
{
    Order order = await GetOrder(46, dbContext, c => c.Customer);
    return new { OrderId = order.OrderId, Customer = order.Customer };
});

app.MapPut("update", async (NorthwindContext dbContext) =>
{
    var users = await dbContext.Employees
        .Where(e => e.HireDate < new DateTime(2021, 6, 1))
        .ToListAsync();

    foreach (var user in users)
    {
        user.Notes = "New employee";
    }

    await dbContext.SaveChangesAsync();
});

app.MapPut("updateWithLinQ2Db", async (NorthwindContext dbContext) =>
{
    var employees = dbContext.Employees
        .Where(e => e.HireDate < new DateTime(2021, 6, 1));

    await LinqToDB.LinqExtensions.UpdateAsync(employees.ToLinqToDB(), x => new Employee
    {
        Notes = "Liq New Employee"
    });
});

app.Run();

async Task<Order> GetOrder(int orderId, NorthwindContext dbContext, params Expression<Func<Order, object>>[] includes)
{
    //Order order = await dbContext.Orders
    //    .Include(o => o.OrderDetails)
    //    .Include(o => o.ShipViaNavigation)
    //    .Include(o => o.Customer)
    //    .FirstAsync(o => o.OrderId == orderId);

    var baseQuery = dbContext.Orders
        .AsQueryable()
        .Where(o => o.OrderId == orderId);

    if (includes.Any())
    {
        foreach (var include in includes)
        {
            baseQuery = baseQuery.Include(include);
        }
    }

    var order = await baseQuery.FirstAsync(o => o.OrderId == orderId);

    return order;
};
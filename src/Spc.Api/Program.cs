using Microsoft.EntityFrameworkCore;
using Spc.Application.Drugs;
using Spc.Application.Inventory;
using Spc.Application.Orders;
using Spc.Application.Pharmacies;
using Spc.Infrastructure.Data;
using Spc.Infrastructure.Services;
using System.Text.Json.Serialization;
using Spc.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Framework Services (Controllers, Swagger)
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Register Database (SQLite)
builder.Services.AddDbContext<SpcDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("SpcDb")));

// 3. Register Application Services (Dependency Injection)
builder.Services.AddScoped<IDrugService, DrugService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPharmacyService, PharmacyService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

// 4. Seed Data (Runs automatically on startup)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SpcDbContext>();
    await DbInitializer.SeedAsync(db);
}

// 5. Configure HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add MultiTenant
builder.Services.AddMultiTenant<TenantInfo>()
                // Set the tenant manually
                .WithStaticStrategy("acme")
                .WithConfigurationStore();

var app = builder.Build();

// Apply migrations if needed
var store = app.Services.GetRequiredService<IMultiTenantStore<TenantInfo>>();
foreach(var tenant in await store.GetAllAsync())
{
    await using var db = new ApplicationDbContext(tenant);
    await db.Database.MigrateAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMultiTenant();

app.MapGet("/getTenant", (HttpContext httpContext) =>
{
    var TenantInfo = httpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
    return TenantInfo.Id;
})
.WithName("GetTenant")
.WithOpenApi();

app.Run();

using MediatR.Caching.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sample.Data;

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.SetMinimumLevel(LogLevel.Debug).ClearProviders().AddConsole();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer("data source=localhost;initial catalog=Kwik;integrated security=True;MultipleActiveResultSets=True;App=MediatR.Caching;connect timeout=2;TrustServerCertificate=true;"));
builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining(typeof(Program));
}).AddMediatRCaching()
.AddCachePoliciesFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


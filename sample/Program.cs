using MediatR.Caching.Extensions;
using Microsoft.EntityFrameworkCore;
using Sample.Data;

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.SetMinimumLevel(LogLevel.Debug).ClearProviders().AddConsole();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
//builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer("data source=localhost;initial catalog=ToDo;integrated security=True;MultipleActiveResultSets=True;App=MediatR.Caching;connect timeout=2;TrustServerCertificate=true;"));
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseInMemoryDatabase(databaseName: "ToDoDatabase");
});
builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining(typeof(Program));
}).AddMediatRCaching()
.AddCachePoliciesFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Run seeder
using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	var seeder = new TodoSeeder(context);
	seeder.SeedData();
}

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


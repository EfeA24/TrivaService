using Microsoft.EntityFrameworkCore;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.ServiceRepositoryAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.StockRepositoryAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.UserRepositoryAbstractions;
using TrivaService.Data;
using TrivaService.Repositories.CommonRepositories;
using TrivaService.Repositories.RepositoryImplementations.ServiceRepositoryImplementations;
using TrivaService.Repositories.RepositoryImplementations.StockRepositoryImplementations;
using TrivaService.Repositories.RepositoryImplementations.UserRepositoryImplementations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<DapperOptions>(options =>
{
    options.ConnectionString = connectionString;
    options.CommandTimeoutSeconds = 30;
});

builder.Services.AddScoped<DapperContext>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceItemRepository, ServiceItemRepository>();
builder.Services.AddScoped<IServiceVisualsRepository, ServiceVisualsRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
builder.Services.AddScoped<IUsersRepository, UserRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

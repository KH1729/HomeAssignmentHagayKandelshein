using Microsoft.EntityFrameworkCore;
using CurrencyExchangeAPI.Data;
using CurrencyExchangeAPI.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = null // Disable static web assets
});

// Disable static web assets completely
builder.WebHost.ConfigureAppConfiguration((context, config) =>
{
    context.HostingEnvironment.WebRootPath = null;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<ExchangeRateService>();
builder.Services.AddHostedService<ExchangeRateBackgroundService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();


using BagiraWebApi;
using BagiraWebApi.Services;
using BagiraWebApi.Services.Bagira;
using BagiraWebApi.Services.Exchanges;
using BagiraWebApi.Services.Loggers.FileLogger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("RemoteConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<Exchange1C>();
builder.Services.AddScoped<GoodService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddCors();
builder.Services.AddOutputCache(oc =>
{
    oc.DefaultExpirationTimeSpan = TimeSpan.FromDays(1);
    oc.AddPolicy("GoodsMenuTag", pb => pb.Tag("GoodsMenu"));
    oc.AddPolicy("GoodsTag", pb => pb.Tag("Goods"));
});

builder.Logging.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "logs"));


var app = builder.Build();
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseCors(builder => builder.AllowAnyOrigin());

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.UseOutputCache();

app.Run();

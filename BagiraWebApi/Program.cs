using BagiraServer.Services.Parser;
using BagiraWebApi;
using BagiraWebApi.Configs.Messenger;
using BagiraWebApi.Services;
using BagiraWebApi.Services.Auth;
using BagiraWebApi.Services.Bagira;
using BagiraWebApi.Services.Exchanges;
using BagiraWebApi.Services.Loggers.FileLogger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("Configs/Messenger/messengerConfig.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/keywords.json", optional: true, reloadOnChange: true);
builder.Services.AddOptions<AuthConfig>().BindConfiguration("Auth").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<MessengerConfig>().BindConfiguration("Messenger").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<BagiraConfig>().BindConfiguration("Bagira").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<Connection1CConfig>().BindConfiguration("Connection1C").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<KeywordsConfig>().BindConfiguration("Keywords").ValidateDataAnnotations().ValidateOnStart();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = builder.Configuration.GetConnectionString("RemoteConnection");
//var connectionString = builder.Configuration.GetConnectionString("DebugConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<Exchange1C>();
builder.Services.AddScoped<GoodService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<ParserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddCors();
builder.Services.AddOutputCache(oc =>
{
    oc.DefaultExpirationTimeSpan = TimeSpan.FromHours(1);
    oc.AddPolicy("GoodsMenuTag", pb => pb.Tag("GoodsMenu"));
    oc.AddPolicy("GoodsTag", pb => pb.Tag("Goods"));
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authConfig = builder.Configuration.GetSection("Auth").Get<AuthConfig>();
        options.TokenValidationParameters = AuthService.GetTokenValidationParameters(authConfig);
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

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseOutputCache();

app.Run();

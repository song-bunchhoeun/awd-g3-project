using DGC.eKYC.Api.Extensions;
using DGC.eKYC.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.ConfigureRedisCache(builder.Configuration);
builder.Services.ConfigureBusinessServices(builder.Configuration);
builder.Services.ConfigureHttpClient(builder.Configuration);
builder.Services.ConfigureApi(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
app.UseMiddleware<ExceptionHelperMiddleware>();
app.UseSwaggerConfig();
app.UseAuthorization();
app.MapControllers();
app.Run();

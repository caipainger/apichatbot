using Microsoft.EntityFrameworkCore; // Add this using statement
using BBtaChatbotJackApi.Context; // Add this using statement
using Microsoft.Extensions.Configuration; // Add this using statement

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
builder.Services.AddScoped<BBtaChatbotJackApi.Services.BancoBogotaFunciones>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<BBtaChatbotJackApi.Middleware.ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();


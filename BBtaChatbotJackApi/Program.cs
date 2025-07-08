using Microsoft.EntityFrameworkCore; // Add this using statement
using BBtaChatbotJackApi.Context; // Add this using statement
using Microsoft.Extensions.Configuration;
using BBtaChatbotJackApi.Services; // Add this using statement

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
builder.Services.AddScoped<BancoBogotaDAO>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<FileProcessorService>();
builder.Services.AddScoped<BancoBogotaFunciones>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
       connectionString, ServerVersion.AutoDetect(connectionString)
    );

    options.EnableSensitiveDataLogging(); // Solo para desarrollo
    options.LogTo(Console.WriteLine, LogLevel.Information);
}); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //dbContext.Database.Migrate(); // Usar en producción
    //dbContext.Database.EnsureCreated(); // Usar en desarrollo
    dbContext.Database.EnsureCreated();


}
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


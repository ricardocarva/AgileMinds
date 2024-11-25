using AgileMindsWebAPI.Data;
using DiscordBot;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext for your database (using MySQL as an example)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Add JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = false
    };
});

// 3. Add CORS to allow the Blazor client to communicate with the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// 4. Add Controllers and Swagger
builder.Services.AddHttpClient();

builder.Services.AddAuthentication();
builder.Services.AddControllers();
builder.Services.AddAntiforgery();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddSingleton<DiscordBotService>();
builder.Services.AddTransient<SlashCommandModule>();


var app = builder.Build();

// Configure the HTTP request pipeline.

// 5. Enable Swagger in Development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6. Enable CORS (before routing and authentication)
app.UseCors("AllowAllOrigins");

// 7. Use HTTPS redirection
app.UseHttpsRedirection();

// test database connection during startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // try to access the database to confirm the connection
        dbContext.Database.CanConnect();
        Console.WriteLine("Successfully connected to the database.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect to the database: {ex.Message}");
    }
}

// 8. Use Routing (before authentication)
app.UseRouting();

// 9. Enable Authentication
app.UseAuthentication();
//app.UseAntiforgery();

// 10. Enable Authorization (must come after authentication)
app.UseAuthorization();

// 11. Map the controller endpoints
app.MapControllers();

app.Run();
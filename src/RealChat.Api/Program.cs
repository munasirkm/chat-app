using Microsoft.EntityFrameworkCore;
using RealChat.Api.Api;
using RealChat.Api.Data;
using RealChat.Api.Hubs;
using RealChat.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=realchat.db"));

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<IConnectionTracker, ConnectionTracker>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { name = "RealChat API", status = "running" }));
app.MapHub<ChatHub>("/hubs/chat");
app.MapChatApi();

app.Run();

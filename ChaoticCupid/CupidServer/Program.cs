using CupidServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<CupidHub>("/cupidHub");

app.Run();

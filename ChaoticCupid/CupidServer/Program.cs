using System.Text;
using CupidServer;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<CupidHub>("/cupidHub");

app.Run("http://localhost:5267");

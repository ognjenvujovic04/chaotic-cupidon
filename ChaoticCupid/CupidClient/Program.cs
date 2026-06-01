using Microsoft.AspNetCore.SignalR.Client;

namespace CupidClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7128/cupidHub")
                .Build();

            await connection.StartAsync();
            Console.WriteLine("[CUPID] Connected! Sending love letters every 60 seconds...\n");

            while (true)
            {
                await connection.InvokeAsync("SendLoveLetters");
                Console.WriteLine($"[CUPID] Love letters sent at {DateTime.Now}");
                await Task.Delay(60000);
            }
        }
    }
}

using System.Text;
using Microsoft.AspNetCore.SignalR.Client;

namespace CupidClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5267/cupidHub")
                .Build();

            await connection.StartAsync();
            Console.WriteLine("\U0001F3E9 Connected! Sending love letters every 60 seconds...\n");

            while (true)
            {
                await connection.InvokeAsync("SendLoveLetters");
                Console.WriteLine($"\U0001F498 Love letters sent at {DateTime.Now}");
                await Task.Delay(60000);
            }
        }
    }
}

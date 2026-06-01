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

            connection.Closed += (ex) =>
            {
                Console.WriteLine("\n\U0001F534 Server disconnected.");
                Environment.Exit(1);
                return Task.CompletedTask;
            };

            try
            {
                await connection.StartAsync();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("⚠️ Could not connect to server. Make sure CupidServer is running.");
                return;
            }

            Console.WriteLine("\U0001F3E9 Connected! Sending love letters every 60 seconds...\n");

            while (true)
            {
                try
                {
                    await connection.InvokeAsync("SendLoveLetters");
                    Console.WriteLine($"\U0001F498 Love letters sent at {DateTime.Now}");
                }
                catch (Exception)
                {
                    Console.WriteLine("⚠️ Lost connection to server.");
                    return;
                }

                await Task.Delay(60000);
            }
        }
    }
}

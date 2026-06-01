using System.Text;
using Microsoft.AspNetCore.SignalR.Client;

namespace PersonClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string username = ReadNonEmptyString("Enter username: ");
            string city = ReadNonEmptyString("Enter city: ");
            int age = ReadPositiveInt("Enter age: ");
            string phoneNumber = ReadPositiveNumber("Enter phone number: ");

            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5267/cupidHub")
                .Build();

            connection.On<string, string, int, string, string>("LoveLetterArrived",
                (senderUsername, senderCity, senderAge, senderPhone, message) =>
                {
                    Console.WriteLine("\n\U0001F48C --- Love Letter Arrived! ---");
                    Console.WriteLine($"\U0001F464 From: {senderUsername}");
                    Console.WriteLine($"\U0001F3E0 City: {senderCity}");
                    Console.WriteLine($"\U0001F382 Age: {senderAge}");

                    if (message != "Nisam zainteresovan/a za upoznavanje.")
                    {
                        Console.WriteLine($"\U0001F4DE Phone: {senderPhone}");
                    }

                    Console.WriteLine($"\U0001F48C Message: \"{message}\"");
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("✅ Press Enter to confirm or type /block username:");
                });

            await connection.StartAsync();
            Console.WriteLine($"\U0001F517 Connected as {username}.");

            await connection.InvokeAsync("InitSinglePerson", username, city, age, phoneNumber);
            Console.WriteLine("\U0001F498 Registered! Waiting for love letters...\n");

            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    await connection.InvokeAsync("ConfirmReceived");
                    Console.WriteLine("✅ Letter confirmed. Waiting for next one...\n");
                }
                else if (input.StartsWith("/block "))
                {
                    string userToBlock = input.Substring(7).Trim();
                    if (!string.IsNullOrEmpty(userToBlock))
                    {
                        await connection.InvokeAsync("BlockUser", userToBlock);
                        Console.WriteLine($"\U0001F6AB Blocked user '{userToBlock}'.");
                    }
                    else
                    {
                        Console.WriteLine("[PERSON] Usage: /block username");
                    }
                }
            }
        }

        static string ReadNonEmptyString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();
                Console.WriteLine("Input cannot be empty. Please try again.");
            }
        }

        static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value) && value > 0)
                    return value;
                Console.WriteLine("Please enter a valid positive number.");
            }
        }

        static string ReadPositiveNumber(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && long.TryParse(input, out long value) && value > 0)
                    return input.Trim();
                Console.WriteLine("Please enter a valid positive phone number.");
            }
        }
    }
}

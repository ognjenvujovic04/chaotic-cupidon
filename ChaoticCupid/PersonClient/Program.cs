using Microsoft.AspNetCore.SignalR.Client;

namespace PersonClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string username = ReadNonEmptyString("Enter username: ");
            string city = ReadNonEmptyString("Enter city: ");
            int age = ReadPositiveInt("Enter age: ");
            string phoneNumber = ReadPositiveNumber("Enter phone number: ");

            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7128/cupidHub")
                .Build();

            connection.On<string, string, int, string, string>("LoveLetterArrived",
                (senderUsername, senderCity, senderAge, senderPhone, message) =>
                {
                    Console.WriteLine("\n--- Love Letter Arrived! ---");
                    Console.WriteLine($"From: {senderUsername}");
                    Console.WriteLine($"City: {senderCity}");
                    Console.WriteLine($"Age: {senderAge}");

                    if (message != "Nisam zainteresovan/a za upoznavanje.")
                    {
                        Console.WriteLine($"Phone: {senderPhone}");
                    }

                    Console.WriteLine($"Message: \"{message}\"");
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("Press Enter to confirm or type /block username:");
                });

            await connection.StartAsync();
            Console.WriteLine($"[PERSON] Connected as {username}.");

            await connection.InvokeAsync("InitSinglePerson", username, city, age, phoneNumber);
            Console.WriteLine("[PERSON] Registered! Waiting for love letters...\n");

            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    await connection.InvokeAsync("ConfirmReceived");
                    Console.WriteLine("[PERSON] Letter confirmed. Waiting for next one...\n");
                }
                else if (input.StartsWith("/block "))
                {
                    string userToBlock = input.Substring(7).Trim();
                    if (!string.IsNullOrEmpty(userToBlock))
                    {
                        await connection.InvokeAsync("BlockUser", userToBlock);
                        Console.WriteLine($"[PERSON] Blocked user '{userToBlock}'.");
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

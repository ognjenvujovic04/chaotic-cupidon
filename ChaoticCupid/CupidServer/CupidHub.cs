using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;

namespace CupidServer
{
    public class CupidHub : Hub
    {
        private static readonly Dictionary<string, Person> _persons = new();

        private static readonly string[] _loveMessages =
        {
            "Radujem se nasem susretu!",
            "Zelim da se upoznamo.",
            "Nisam zainteresovan/a za upoznavanje."
        };

        public async Task InitSinglePerson(string username, string city, int age, string phoneNumber)
        {
            if (_persons.ContainsKey(username))
            {
                Console.WriteLine($"[SERVER] Username '{username}' already exists!");
                return;
            }

            var person = new Person
            {
                ConnectionId = Context.ConnectionId,
                Username = username,
                City = city,
                Age = age,
                PhoneNumber = phoneNumber
            };

            _persons.Add(username, person);

            await Groups.AddToGroupAsync(Context.ConnectionId, "Singles");

            Console.WriteLine($"[SERVER] {username} ({city}, {age}) registered as single!");
        }

        public Task ConfirmReceived()
        {
            var person = _persons.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (person != null)
            {
                person.HasUnreadLetter = false;
                Console.WriteLine($"[SERVER] {person.Username} confirmed letter received.");
            }

            return Task.CompletedTask;
        }

        public Task BlockUser(string usernameToBlock)
        {
            var person = _persons.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (person != null && !person.BlockedUsers.Contains(usernameToBlock))
            {
                person.BlockedUsers.Add(usernameToBlock);
                Console.WriteLine($"[SERVER] {person.Username} blocked {usernameToBlock}.");
            }

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var person = _persons.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (person != null)
            {
                _persons.Remove(person.Username);
                Console.WriteLine($"[SERVER] {person.Username} disconnected and removed.");
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendLoveLetters()
        {
            var persons = _persons.Values.ToList();

            if (persons.Count < 2)
            {
                Console.WriteLine("[SERVER] Not enough persons to send letters.");
                return;
            }

            foreach (var recipient in persons)
            {
                if (recipient.HasUnreadLetter)
                    continue;

                var candidates = persons
                    .Where(p => p.Username != recipient.Username
                             && !recipient.BlockedUsers.Contains(p.Username))
                    .ToList();

                if (candidates.Count == 0)
                    continue;

                var bestMatch = candidates
                    .Select(c => new { Candidate = c, Score = CalculateScore(c, recipient) })
                    .OrderByDescending(x => x.Score)
                    .First()
                    .Candidate;

                string message = _loveMessages[RandomInt(0, _loveMessages.Length)];

                bool showPhone = message != "Nisam zainteresovan/a za upoznavanje.";

                await Clients.Client(recipient.ConnectionId).SendAsync("LoveLetterArrived",
                    bestMatch.Username,
                    bestMatch.City,
                    bestMatch.Age,
                    showPhone ? bestMatch.PhoneNumber : "",
                    message);

                recipient.HasUnreadLetter = true;

                Console.WriteLine($"[SERVER] Letter sent to {recipient.Username} from {bestMatch.Username} ({message})");
            }
        }

        private static int CalculateScore(Person sender, Person receiver)
        {
            int score = 0;

            if (sender.City.Equals(receiver.City, StringComparison.OrdinalIgnoreCase))
                score += 30;

            if (Math.Abs(sender.Age - receiver.Age) <= 2)
                score += 20;

            score += RandomInt(0, 101);

            return score;
        }

        private static int RandomInt(int minValue, int maxValue)
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return minValue + (value % (maxValue - minValue));
        }

        public static Dictionary<string, Person> GetPersons() => _persons;
    }
}

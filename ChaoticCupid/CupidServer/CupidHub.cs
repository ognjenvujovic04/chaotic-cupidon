using Microsoft.AspNetCore.SignalR;

namespace CupidServer
{
    public class CupidHub : Hub
    {
        private static readonly Dictionary<string, Person> _persons = new();

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

        public static Dictionary<string, Person> GetPersons() => _persons;
    }
}

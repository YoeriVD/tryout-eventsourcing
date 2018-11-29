using System;
using System.Text;
using Bogus;
using EventSourceDotNet.Core;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EventSourceDotNet.ConsoleApp
{
    internal class Program
    {
        private static IEventStoreConnection _storeConnection;

        private static readonly Faker<InventoryItem> faker = new Faker<InventoryItem>()
            .RuleFor(item => item.Id, faker => faker.Random.UInt())
            .RuleFor(item => item.Name, faker => faker.Commerce.ProductName());

        private static void Main(string[] args)
        {
            _storeConnection =
                EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "console-app");
            _storeConnection.ConnectAsync().Wait();
            var inventory = new Inventory(_storeConnection);
            inventory.Load().Wait();
            Console.WriteLine($"{inventory.Items.Count} item(s) loaded.");
            var cmd = Commands.List;
            do
            {
                foreach (var value in Enum.GetValues(typeof(Commands))) Console.WriteLine($"{value}");


                cmd = Enum.Parse<Commands>(Console.ReadLine());
                switch (cmd)
                {
                    case Commands.List:
                        List(inventory);
                        break;
                    case Commands.Add:
                        Add();
                        break;
                    case Commands.Clean:
                        break;
                    case Commands.Stop:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } while (cmd != Commands.Stop);
        }

        private static void Add()
        {
            var item = faker.Generate();
            var itemAsJson = JsonConvert.SerializeObject(item);
            var itemùAsBytes = Encoding.Unicode.GetBytes(itemAsJson);
            var eventData = new EventData(
                Guid.NewGuid(),
                "addInventoryItem",
                data: itemùAsBytes,
                isJson: true,
                metadata: null
            );
            _storeConnection.AppendToStreamAsync("InventoryItems", ExpectedVersion.Any, eventData).Wait();
        }

        private static void List(Inventory inventory)
        {
            inventory.Items.ForEach(item => Console.WriteLine(item));
        }

        private enum Commands
        {
            List = 1,
            Add = 2,
            Clean = 3,
            Stop = 0
        }
    }
}
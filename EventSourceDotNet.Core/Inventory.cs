using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EventSourceDotNet.Core
{
    public class Inventory
    {
        private readonly IEventStoreConnection _storeConnection;

        public Inventory(IEventStoreConnection storeConnection)
        {
            _storeConnection = storeConnection;
        }

        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

        public async Task Load()
        {
            var readEvents = await _storeConnection.ReadAllEventsForwardAsync(new Position(0, 0), 4000, false);
            foreach (var resolvedEvent in readEvents.Events)
            {
                Console.WriteLine("Processing event", resolvedEvent.Event.EventType);
                ProcessEvent(resolvedEvent);
            }

            _storeConnection.SubscribeToAllAsync(false, EventAppeared);
        }

        private async Task EventAppeared(EventStoreSubscription eventStoreSubscription, ResolvedEvent resolvedEvent)
        {
            Console.WriteLine($"new event: {resolvedEvent.Event.EventType}");
            ProcessEvent(resolvedEvent);
            await Task.FromResult(0);
        }

        private void ProcessEvent(ResolvedEvent resolvedEvent)
        {
            
            if (resolvedEvent.Event.EventType == "addInventoryItem")
            {
                var dataAsBytes = resolvedEvent.Event.Data;
                var dataAsJson = Encoding.Unicode.GetString(dataAsBytes);
                var item = JsonConvert.DeserializeObject<InventoryItem>(dataAsJson);
                if (item != null) Items.Add(item);
            }
        }
    }
}
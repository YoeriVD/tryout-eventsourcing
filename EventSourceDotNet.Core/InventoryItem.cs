namespace EventSourceDotNet.Core
{
    public class InventoryItem
    {
        public uint Id { get; set; }
        public string Name { get; set; }

        public override string ToString() => $"{Id}: {Name}";
    }
}
namespace MauiBleRfIdBridgeExampleApp.Models
{
    public class RfIdTagData
    {
        public byte[]? RfIdTagUid { get; set; }
        public string? RfIdTagText { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

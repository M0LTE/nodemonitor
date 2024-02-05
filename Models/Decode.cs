namespace nodemonitor.Models;

public record struct Decode
{
    public string Timestamp { get; internal set; }
    public string Node { get; internal set; }
    public string ModemId { get; internal set; }
    public int ModemPort { get; internal set; }
    public string Direction { get; internal set; }
    public string Data { get; internal set; }
}
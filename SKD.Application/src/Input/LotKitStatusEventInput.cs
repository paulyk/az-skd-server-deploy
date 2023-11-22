#nullable enable
namespace SKD.Service;

public class LotKitStatusEventInput {
    public string LotNo { get; init; } = "";
    public KitStatusCode EventCode { get; init; }
    public DateTimeOffset EventDate { get; init; }
    public string EventNote { get; init; }  = "";
}

#nullable enable
namespace SKD.Service;

public class KitStatusEventInput {
    public string KitNo { get; set; } = "";
    public KitStatusCode EventCode { get; set; }
    public DateTimeOffset EventDate { get; set; }
    public string EventNote { get; set; } = "";
    public string DealerCode { get; set; } = "";
}


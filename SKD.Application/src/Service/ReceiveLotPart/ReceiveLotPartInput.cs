#nullable enable

namespace SKD.Application.ReceiveLotPart;

public class ReceiveLotPartInput {
    public string LotNo { get; set; } = "";
    public string PartNo { get; set; } = "";
    public int Quantity { get; set; }
}

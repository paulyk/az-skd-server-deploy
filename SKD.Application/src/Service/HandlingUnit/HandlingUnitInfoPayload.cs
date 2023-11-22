#nullable enable

namespace SKD.Service;

public class HandlingUnitInfoPayload {
    public string? Code { get; set; }
    public string? InvoiceNo { get; set; }
    public string? LotNo { get; set; }
    public Guid ShipmentId { get; set; }
    public string PlantCode { get; set; } = "";
    public int ShipmentSequence { get; set; }
    public string? PcvCode { get; set; }
    public string? PcvDescription { get; set; }
    public int PartCount { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? ReceivedRemovedAt { get; set; }
    public ICollection<HU_Part> Parts { get; set; } = new List<HU_Part>();
}

public class HU_Part {
    public string PartNo { get; set; } = "";
    public string PartDesc { get; set; } = "";
    public int Quantity { get; set; }
}


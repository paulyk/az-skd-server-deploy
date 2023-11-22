#nullable enable
namespace SKD.Domain;

public class Shipment : EntityBase {
    public Guid PlantId { get; set; }
    public Plant Plant { get; set; } = null!;
    public int Sequence { get; set; }
    public string? Filename { get; set;  }
    public ICollection<ShipmentLot> ShipmentLots { get; set; } = new List<ShipmentLot>();
}

#nullable enable
namespace SKD.Domain;

public class Bom : EntityBase {

    private Bom() {}
    public Guid PlantId { get; private set; }
    public Plant Plant { get; set; } = null!;
    public int Sequence { get; private set; }
    public string? Filename { get; private set; }
    public bool LotPartQuantitiesMatchShipment { get; set; }
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();

    public static Bom Create(
        int sequence,
        string? filename,        
        Guid plantId
    ) {
        var bom = new Bom {
            Sequence = sequence,
            Filename = filename,        
            PlantId = plantId
        };
        return bom;
    }

    public void UpdateFilename(string? filename) {
        Filename = filename;
    }
}

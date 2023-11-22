namespace SKD.Domain;

public class Plant : EntityBase {

    private Plant() {}
    public string Code { get; private set; }
    public string Name { get; private set; }

    public string PartnerPlantCode { get; set; }
    public string PartnerPlantType { get; set; }

    public ICollection<Lot> Lots { get; set; } = new List<Lot>();
    public ICollection<Bom> Boms { get; set; } = new List<Bom>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public static Plant Create(string code, string name, string partnerPlantCode, string partnerPlantType) {
        var plant = new Plant {
            Code = code,
            Name = name,
            PartnerPlantCode = partnerPlantCode,
            PartnerPlantType = partnerPlantType
        };
        return plant;
    }
}

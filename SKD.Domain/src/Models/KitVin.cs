namespace SKD.Domain;

public class KitVin : EntityBase {

    private KitVin() { }
    public Guid KitId { get; private set; }
    public Kit Kit { get; set; }

    public string VIN { get; private set; }

    internal static  KitVin Create(Guid kitId, string vin) {
        return new KitVin {
            KitId = kitId,
            VIN = vin
        };
    }    
}

namespace SKD.Domain;

public class KitComponent : EntityBase {

    public Guid KitId { get; set; }
    public Kit Kit { get; set; }

    public Guid ComponentId { get; set; }
    public Component Component { get; set; }

    public Guid ProductionStationId { get; set; }
    public ProductionStation ProductionStation { get; set; }

    public virtual ICollection<ComponentSerial> ComponentSerials { get; set; } = new List<ComponentSerial>();
    public DateTime? VerifiedAt { get; set; }

    private KitComponent() { }

    public static KitComponent Create(Guid kitId, Guid componentId, Guid productionStationId) {
        var kitComponent = new KitComponent {
            KitId = kitId,
            ComponentId = componentId,
            ProductionStationId = productionStationId
        };
        return kitComponent;
    }
}

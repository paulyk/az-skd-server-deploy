#nullable enable

namespace SKD.Domain;

public partial class Kit : EntityBase {

    public virtual string VIN { get; private set; } = "";
    public string KitNo { get; set; } = "";

    public Guid LotId { get; set; }
    public Lot Lot { get; set; } = new Lot();

    public Guid? DealerId { get; private set; }
    public Dealer? Dealer { get; private set; }

    private readonly List<KitComponent> _kitComponents = new();
    public IReadOnlyCollection<KitComponent> KitComponents => _kitComponents.AsReadOnly();
    
    private readonly List<KitVin> _kitVins = new();
    public IReadOnlyCollection<KitVin> KitVins => _kitVins.AsReadOnly();

    public List<KitStatusEvent> KitStatusEvents { get; set; } = new();

    private Kit() {}

    public static Kit Create(string kitNo) {
        var kit = new Kit {
            KitNo = kitNo
        };
        return kit;
    }

    public void SetDealer(Guid dealerId) {
        DealerId = dealerId;
    }
    public void AssignVIN(string vin) {
        VIN = vin;
        var kitVin = KitVin.Create(this.Id, vin);
        _kitVins.Add(kitVin);
    }

 public void AssignPcvComponents(PCV pcv, IEnumerable<ComponentStation> componentStations) {
        // Check if PCV has any PcvComponents
        if (pcv.PcvComponents == null || !pcv.PcvComponents.Any()) {
            throw new Exception("PCV does not contain any PcvComponent");
        }

        // Remove KitComponents that are not in PCV.PcvComponents and have empty ComponentSerials
        _kitComponents.RemoveAll(kc => 
            !pcv.PcvComponents.Any(pc => pc.ComponentId == kc.ComponentId) && 
            (kc.ComponentSerials == null || !kc.ComponentSerials.Any())
        );

        /*
        Figure out with component stations have componetIds that are in PCV.PcvComponents
        */
        var selectedStations = componentStations
            .OrderBy(cs => cs.Station.Sequence)
            .Where(cs => pcv.PcvComponents.Any(pc => pc.ComponentId == cs.ComponentId))
            .ToList();
        
        // foreach component station, add KitComponent if it does not exist
        foreach (var componentStation in selectedStations) {
            
            var kitComponent = _kitComponents
                .Where(kc => kc.ComponentId == componentStation.ComponentId)
                .Where(kc => kc.ProductionStationId == componentStation.StationId)
                .FirstOrDefault();

            if (kitComponent == null) {
                kitComponent = KitComponent.Create(Id, componentStation.ComponentId, componentStation.StationId);
                _kitComponents.Add(kitComponent);
            }
        }
    }
    
}

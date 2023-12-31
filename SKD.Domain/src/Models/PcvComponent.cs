namespace SKD.Domain;

public class PcvComponent : EntityBase {

    public Guid PcvId { get; set; }
    public PCV Pcv { get; set; }

    public Guid ComponentId { get; set; }
    public Component Component { get; set; }

}

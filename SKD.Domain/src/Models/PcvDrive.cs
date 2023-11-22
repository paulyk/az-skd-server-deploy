#nullable enable
namespace SKD.Domain;

public class PcvDrive : EntityBase, IPcvMetaICategory {
    public String Code { get; set; } = "";
    public String Name { get; set; } = "";

    public ICollection<PCV> Pcvs { get; set; } = new List<PCV>();
}
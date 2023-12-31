#nullable enable

namespace SKD.Domain;

public class Dealer : EntityBase {
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public ICollection<Kit> Kits { get; set; } = new List<Kit>();

    public Dealer() : base() {

    }
}



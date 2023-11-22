#nullable enable

namespace SKD.Domain;

public partial class KitStatusEventType : EntityBase {
    public KitStatusCode Code { get; set; }
    public PartnerStatusCode PartnerStatusCode { get; set; }
    public string Description { get; set; } = "";
    public int Sequence { get; set; }
}

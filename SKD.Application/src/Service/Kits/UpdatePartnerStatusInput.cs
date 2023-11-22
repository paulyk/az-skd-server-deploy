#nullable enable

namespace SKD.Mode;
public class UpdatePartnerStatusInput {
    public string KitNo { get; set; } = null!;
    public KitStatusCode EventCode { get; set; }
}
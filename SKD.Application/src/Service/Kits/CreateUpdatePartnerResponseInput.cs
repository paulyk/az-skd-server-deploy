using SKD.KitStatusFeed;

namespace SKD.Service;

public class CreateUpdatePartnerResponseInput {
    public string KitNo { get; set; } = "";
    public KitStatusCode EventCode { get; set; }
    public PartnerStatusLayoutData ResponseData { get; set; } = new PartnerStatusLayoutData();
}
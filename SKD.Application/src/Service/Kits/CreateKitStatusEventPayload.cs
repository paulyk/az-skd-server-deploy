#nullable enable

namespace SKD.Service;

public class CreateKitStatusEventPayload {
    public CreateKitStatusEventPayload(string kitNo, KitStatusCode eventCode, DateTime eventDate) {
        this.KitNo = kitNo;
        this.EventCode = eventCode;
        this.EventDate = eventDate;
    }
    public string KitNo { get; set; } = "";
    public KitStatusCode EventCode { get; set; }
    public DateTime EventDate { get; set;  }
}
namespace SKD.Domain;
public class KitStatusEvent : EntityBase {
    public Guid KitStatusEventTypeId { get; set; }
    public KitStatusEventType EventType { get; set; }
    public DateTimeOffset EventDate { get; set; }
    public string EventNote { get; set; }

    public Guid KitId { get; set; }
    public Kit Kit { get; set; }

    public DateTimeOffset? PartnerStatusUpdatedAt { get; set; }
}
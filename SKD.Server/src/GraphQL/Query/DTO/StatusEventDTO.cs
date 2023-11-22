namespace SKD.Server;

public class StatusEventDTO {
    public KitStatusCode EventType { get; set; }
    public DateTimeOffset? EventDate { get; set; }
    public string? EventNote { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    public DateTimeOffset? PartnerStatusUpdatedAt { get; set; }
    public int Sequence { get; set; }
}

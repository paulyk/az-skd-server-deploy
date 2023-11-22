namespace SKD.Domain;

public class HandlingUnitReceived : EntityBase {
    public Guid HandlingUnitId { get; set; }
    public HandlingUnit HandlingUnit { get; set; }
}

namespace SKD.Server;

public class LotListDTO {
    public Guid Id { get; set; }
    public string PlantCode { get; set; } = "";
    public string LotNo { get; set; } = "";
    public int KitCount { get; set; }
    public KitStatusCode? TimelineStatus { get; set; } = null;
    public DateTime CreatedAt { get; set; }
}

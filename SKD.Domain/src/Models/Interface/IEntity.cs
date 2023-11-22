namespace SKD.Domain;

public interface IEntity {
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}
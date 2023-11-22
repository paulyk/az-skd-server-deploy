#nullable enable
namespace SKD.Service;

public class KitInfoDTO {
    public Guid Id { get; private set; }
    public string KitNo { get; private set; } = null!;
    public string LotNo { get; private set; } = null!;
    public string? VIN { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string Series { get; private set; } = null!;
    public KitStatusCode? KitStatusCode { get; private set; }
    public DateTimeOffset? EventDtate { get; private set; }
    public PartnerStatusCode? PartnerStatusCode { get; private set; }
    public bool PartnerStatusPending { get; private set; }

    public static KitInfoDTO Create(Kit kit) {

        if (kit.KitStatusEvents == null) {
            throw new Exception("kit.KitStatusEvents is null");
        }
        if (kit.Lot?.Pcv == null) {
            throw new Exception("kit.Lot.Pcv is null");
        }
        if (kit.KitStatusEvents.Count != 0 && kit.KitStatusEvents.First().EventType == null) {
            throw new Exception("kit.KitStatusEvents.First().EventType is null");
        }

        return new KitInfoDTO {
            Id = kit.Id,
            KitNo = kit.KitNo,
            VIN = kit.VIN,
            LotNo = kit.Lot.LotNo,
            Model = kit.Lot.Pcv.Model,
            Series = kit.Lot.Pcv.Series,
            EventDtate = kit.KitStatusEvents.Any()
                ? kit.KitStatusEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.EventDate)
                    .FirstOrDefault()
                : null,
            KitStatusCode = kit.KitStatusEvents.Any()
                ? kit.KitStatusEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.EventType.Code)
                    .FirstOrDefault()
                : null,
            PartnerStatusCode = kit.KitStatusEvents.Any()
                ? kit.KitStatusEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.EventType.PartnerStatusCode)
                    .FirstOrDefault()
                : null,
            PartnerStatusPending = kit.KitStatusEvents.Any()
                ? kit.KitStatusEvents
                    .Where(e => e.RemovedAt == null)
                    .OrderByDescending(e => e.EventType.Sequence)
                    .Select(e => e.PartnerStatusUpdatedAt == null)
                    .FirstOrDefault()
                : false
        };
    }
}
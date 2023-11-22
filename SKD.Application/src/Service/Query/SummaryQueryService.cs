#nullable enable

using SKD.Application.Common;

namespace SKD.Service;

public class SummaryQueryService : IAppService {
    private readonly SkdContext context;

    public SummaryQueryService(SkdContext ctx) {
        this.context = ctx;
    }

    /// <summary>
    /// Return kits where latest timline event matches provide KitStatusCode
    /// </summary>
    /// <param name="kitStatusCode"></param>
    /// <returns></returns>
    public IQueryable<Kit> KitsByCurrentKitStatus(
        KitStatusCode kitStatusCode
    ) => context.Kits.AsNoTracking()
        .Where(t => t.RemovedAt == null)
        .Where(t =>
            t.KitStatusEvents
                .OrderByDescending(t => t.EventType.Sequence)
                .Where(t => t.RemovedAt == null)
                .Select(x => x.EventType.Code)
                .FirstOrDefault() == kitStatusCode).AsQueryable();

    /// <summary>
    /// Group count of all kits by current kit status
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ItemCountDTO>> KitsByKitStatusSummary() {

        var groupedKitsByCurrentKitStatus = await context.Kits.AsNoTracking()
            .Include(t => t.KitStatusEvents).ThenInclude(t => t.EventType)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.KitStatusEvents.Where(t => t.RemovedAt == null).Any())
            .Select(t => new {
                t.KitNo,
                Event = t.KitStatusEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.EventType.Sequence)
                .First()
            })
            .Select(t => new {
                t.Event.EventType
            })
            .GroupBy(t => new {
                t.EventType.Code,
                t.EventType.Description,
                t.EventType.Sequence
            })
            .Select(g => new {
                g.Key.Code,
                Name = g.Key.Description,
                g.Key.Sequence,
                Count = g.Count()
            })
            .OrderBy(t => t.Sequence)
            .Select(t => new ItemCountDTO {
                Code = t.Code.ToString(),
                Name = t.Name,
                Count = t.Count
            })
            .ToListAsync();

        var kitStatusEventTypes = await context.KitStatusEventTypes.AsNoTracking()
            .OrderBy(t => t.Sequence).Where(t => t.RemovedAt == null)
            .Select(t => new ItemCountDTO {
                Code = t.Code.ToString(),
                Name = t.Description,
                Count = 0
            }).ToListAsync();

        kitStatusEventTypes.ForEach(t => {
            var existing = Enumerable.FirstOrDefault<ItemCountDTO>((IEnumerable<ItemCountDTO>)groupedKitsByCurrentKitStatus, (Func<ItemCountDTO, bool>)(x => x.Code == t.Code));
            if (existing != null) {
                t.Count = existing.Count;
            }
        });

        var noKitStatusEventsKitCount = await context.Kits
            .Where(t => t.RemovedAt == null)
            .Where(t => !t.KitStatusEvents.Any(t => t.RemovedAt == null))
            .CountAsync();

        if (noKitStatusEventsKitCount > 0) {
            kitStatusEventTypes.Insert(0, new ItemCountDTO {
                Code = "",
                Name = "Custom receive pending",
                Count = noKitStatusEventsKitCount
            });
        }

        return kitStatusEventTypes;
    }
}

#nullable enable

using Microsoft.EntityFrameworkCore.Storage;

using SKD.Application.Common;

namespace SKD.Service;

public class KitService {

    private readonly SkdContext context;
    private readonly DateTime currentDate;

    public KitService(SkdContext ctx, DateTime currentDate) {
        this.context = ctx;
        this.currentDate = currentDate;
    }

    #region create kit status event

    /// <summary>
    /// Create a Build Start status event for a kit
    /// </summary>
    /// <param name="kitNo"></param>
    /// <returns></returns>
    public async Task<MutationResult<KitStatusEvent>> CreateBuildStartEventAsync(string kitNo) {
        // get event date from first component serial date for 
        var componentSerialScanDate = context.ComponentSerials
            .Where(cs => cs.RemovedAt == null)
            .Where(cs => cs.KitComponent.Kit.KitNo == kitNo)
            .Select(cs => cs.CreatedAt).FirstOrDefault();

        return await CreateKitStatusEventAsync(new KitStatusEventInput {
            KitNo = kitNo,
            EventCode = KitStatusCode.BUILD_START,
            EventDate = componentSerialScanDate,
            EventNote = ""
        });
    }

    /// <summary>
    /// Create a status event for a kit
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<KitStatusEvent>> CreateKitStatusEventAsync(KitStatusEventInput input) {
        MutationResult<KitStatusEvent> result = new() {
            Errors = await ValidateCreateKitStatusEventAsync(input)
        };
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.KitStatusEvents).ThenInclude(t => t.EventType)
            .FirstAsync(t => t.KitNo == input.KitNo);

        // mark other status events of the same type as removed for this kit
        kit.KitStatusEvents
            .Where(t => t.EventType.Code == input.EventCode)
            .ToList().ForEach(status => {
                if (status.RemovedAt == null) {
                    status.RemovedAt = DateTime.UtcNow;
                }
            });

        // create status event and add to kit
        var newKitStatusEvent = new KitStatusEvent {
            EventType = await context.KitStatusEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventCode),
            EventDate = input.EventDate,
            EventNote = input.EventNote
        };

        // Set kit dealer code if provided
        if (!string.IsNullOrWhiteSpace(input.DealerCode)) {
            Dealer? dealer = await context.Dealers.FirstOrDefaultAsync(t => t.Code == input.DealerCode);
            kit.SetDealer(dealer?.Id ?? Guid.Empty);
        }

        kit.KitStatusEvents.Add(newKitStatusEvent);

        // save
        result.Payload = newKitStatusEvent;
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<List<Error>> ValidateCreateKitStatusEventAsync(KitStatusEventInput input) {
        var errors = new List<Error>();

        // kitNo
        var kit = await context.Kits.AsNoTracking()
            .Include(t => t.Lot)
            .Include(t => t.KitStatusEvents.Where(t => t.RemovedAt == null))
                .ThenInclude(t => t.EventType)
            .Include(t => t.Dealer)
            .Include(t => t.KitComponents.Where(t => t.RemovedAt == null))
                .ThenInclude(t => t.ComponentSerials.Where(t => t.RemovedAt == null))
            .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

        // kit not found
        if (kit == null) {
            errors.Add(new Error("KitNo", $"kit not found for kitNo: {input.KitNo}"));
            return errors;
        }

        // duplicate status event
        var duplicate = kit.KitStatusEvents
            .OrderByDescending(t => t.EventType.Sequence)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == input.EventCode)
            .Where(t => t.EventDate == input.EventDate)
            .Where(t => t.EventNote == input.EventNote)
            .FirstOrDefault();

        if (duplicate != null) {
            var dateStr = input.EventDate.Date.ToShortDateString();
            errors.Add(new Error("", $"duplicate kit status event: {input.EventCode} {dateStr} "));
            return errors;
        }

        // setup
        List<KitStatusEventType> kitStatusEventTypes = await context.KitStatusEventTypes.AsNoTracking()
            .Where(t => t.RemovedAt == null).ToListAsync();
        KitStatusEventType inputEventType = kitStatusEventTypes.First(t => t.Code == input.EventCode);
        AppSettingsDTO appSettings = await ApplicationSetting.GetKnownAppSettings(context);

        // Missing prior status event
        var priorEventType = kitStatusEventTypes.FirstOrDefault(t => t.Sequence == inputEventType.Sequence - 1);
        if (priorEventType != null) {
            var priorKitStatusEvent = kit.KitStatusEvents.FirstOrDefault(t => t.EventType.Code == priorEventType.Code);
            if (priorKitStatusEvent == null) {
                errors.Add(new Error("", $"Missing status event {priorEventType.Description}"));
                return errors;
            }
        }

        // Cannot set if the next status event in sequence already set
        var nextEventType = kitStatusEventTypes.FirstOrDefault(t => t.Sequence == inputEventType.Sequence + 1);
        if (nextEventType != null) {
            var nextKitStatusEvent = kit.KitStatusEvents.FirstOrDefault(t => t.EventType.Code == nextEventType.Code);
            if (nextKitStatusEvent != null) {
                errors.Add(new Error("", $"{nextEventType.Description} already set, cannot set {inputEventType.Description}"));
                return errors;
            }
        }

        // CUSTOM_RECEIVED 
        if (input.EventCode == KitStatusCode.CUSTOM_RECEIVED) {
            if (currentDate <= input.EventDate) {
                errors.Add(new Error("", $"Custom received date must precede current date by {appSettings.PlanBuildLeadTimeDays} days"));
                return errors;
            }
        }

        // PLAN_BUILD 
        if (input.EventCode == KitStatusCode.PLAN_BUILD) {
            var custom_receive_date = kit.KitStatusEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == KitStatusCode.CUSTOM_RECEIVED)
                .Select(t => t.EventDate).First();

            var custom_receive_plus_lead_time_date = custom_receive_date.AddDays(appSettings.PlanBuildLeadTimeDays);

            var plan_build_date = input.EventDate;
            if (custom_receive_plus_lead_time_date > plan_build_date) {
                errors.Add(new Error("", $"plan build must greater custom receive by {appSettings.PlanBuildLeadTimeDays} days"));
                return errors;
            }
        }

        // BUILD_START must have at least one component seraial scan
        if (input.EventCode == KitStatusCode.BUILD_START) {
            var anyComponentSerialScan = kit.KitComponents
                .Where(t => t.RemovedAt == null)
                .SelectMany(t => t.ComponentSerials)
                .Where(t => t.RemovedAt == null)
                .Any();

            if (!anyComponentSerialScan) {
                errors.Add(new Error("", $"Build Start status requires at least one component serial scan"));
                return errors;
            }
        }

        // WHOLESALE kit must be associated with dealer to proceed
        if (input.EventCode == KitStatusCode.WHOLE_SALE) {
            if (String.IsNullOrWhiteSpace(input.DealerCode)) {
                if (kit.Dealer == null) {
                    errors.Add(new Error("", $"Kit must be associated with dealer {kit.KitNo}"));
                    return errors;
                }
            }
            else {
                var dealer = await context.Dealers.AsNoTracking().FirstOrDefaultAsync(t => t.Code == input.DealerCode);
                if (dealer == null) {
                    errors.Add(new Error("", $"Dealer not found for code {input.DealerCode}"));
                    return errors;
                }
            }
        }

        // VIN Required for events sequence after PLAN BUILD
        var planBuildType = kitStatusEventTypes.First(t => t.Code == KitStatusCode.PLAN_BUILD);
        if (inputEventType.Sequence > planBuildType.Sequence && String.IsNullOrWhiteSpace(kit.VIN)) {
            errors.Add(new Error("", $"Kit does not have VIN, cannot save {input.EventCode} event"));
            return errors;
        }

        // Event date cannot be in the future for events for VERIFY_VIN onwards
        var verifyVinType = kitStatusEventTypes.First(t => t.Code == KitStatusCode.BUILD_START);
        if (inputEventType.Sequence > verifyVinType.Sequence) {
            if (input.EventDate.Date > currentDate.Date) {
                errors.Add(new Error("", $"Date cannot be in the future"));
                return errors;
            }
        }

        /* Remove feature: 2022-02-11, problem with ship file imports from Ford
        // shipment missing
        var hasAssociatedShipment = await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == kit.Lot.LotNo);
        if (!hasAssociatedShipment) {
            errors.Add(new Error("", $"shipment missing for lot: {kit.Lot.LotNo}"));
            return errors;
        }
        */

        return errors;
    }

    #endregion

    #region create lot status event
    public async Task<MutationResult<Lot>> CreateLotKitStatusEventsAsync(LotKitStatusEventInput input) {
        MutationResult<Lot> result = new() {
            Errors = await ValidateCreateLotKitStatusEventsAsync(input)
        };
        if (result.Errors.Count > 0) {
            return result;
        }

        var kitLot = await context.Lots
            .Include(t => t.Kits)
                .ThenInclude(t => t.KitStatusEvents)
                .ThenInclude(t => t.EventType)
            .FirstAsync(t => t.LotNo == input.LotNo);

        foreach (var kit in kitLot.Kits) {

            // mark other status events of the same type as removed for this kit
            kit.KitStatusEvents
                .Where(t => t.EventType.Code == input.EventCode)
                .ToList().ForEach(statusEvent => {
                    if (statusEvent.RemovedAt == null) {
                        statusEvent.RemovedAt = DateTime.UtcNow;
                    }
                });

            // create status event and add to kit
            var newKitStatusEvent = new KitStatusEvent {
                EventType = await context.KitStatusEventTypes.FirstOrDefaultAsync(t => t.Code == input.EventCode),
                EventDate = input.EventDate,
                EventNote = input.EventNote
            };

            kit.KitStatusEvents.Add(newKitStatusEvent);

        }

        // // save
        result.Payload = kitLot;
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<List<Error>> ValidateCreateLotKitStatusEventsAsync(LotKitStatusEventInput input) {
        var errors = new List<Error>();

        var lot = await context.Lots.AsNoTracking()
            .Include(t => t.Kits).ThenInclude(t => t.KitStatusEvents).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.LotNo == input.LotNo);

        // kit lot 
        if (lot == null) {
            errors.Add(new Error("VIN", $"lot not found for lotNo: {input.LotNo}"));
            return errors;
        }

        // duplicate 
        var duplicateKitStatusEventsFound = lot.Kits.SelectMany(t => t.KitStatusEvents)
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == input.EventCode)
            .Where(t => t.EventDate == input.EventDate)
            .ToList();

        if (duplicateKitStatusEventsFound.Count > 0) {
            var dateStr = input.EventDate.Date.ToShortDateString();
            errors.Add(new Error("", $"duplicate kit status event: {input.LotNo}, Type: {input.EventCode} Date: {dateStr} "));
            return errors;
        }

        // CUSTOM_RECEIVED 
        if (input.EventCode == KitStatusCode.CUSTOM_RECEIVED) {
            if (input.EventDate.Date >= currentDate) {
                errors.Add(new Error("", $"custom received date must be before current date"));
                return errors;
            }
            if (input.EventDate.Date < currentDate.AddMonths(-6)) {
                errors.Add(new Error("", $"custom received cannot be more than 6 months ago"));
                return errors;
            }
        }

        /* Remove feature: 2022-02-11, problem with ship file imports from Ford
        // shipment missing        
        var hasAssociatedShipment = await context.ShipmentLots.AnyAsync(t => t.Lot.LotNo == lot.LotNo);
        if (!hasAssociatedShipment) {
            errors.Add(new Error("", $"shipment missing for lot: {lot.LotNo}"));
            return errors;
        }
        */

        return errors;
    }

    #endregion

    #region change kit component production station
    public async Task<MutationResult<KitComponent>> ChangeKitComponentProductionStationAsync(KitComponentProductionStationInput input) {
        MutationResult<KitComponent> result = new() {
            Errors = await ValidateChangeKitComponentStationInputAsync(input)
        };
        if (result.Errors.Count > 0) {
            return result;
        }

        var kitComponent = await context.KitComponents.FirstAsync(t => t.Id == input.KitComponentId);
        var productionStation = await context.ProductionStations.FirstAsync(t => t.Code == input.ProductionStationCode);

        kitComponent.ProductionStation = productionStation;
        // // save
        await context.SaveChangesAsync();
        result.Payload = kitComponent;
        return result;
    }

    public async Task<List<Error>> ValidateChangeKitComponentStationInputAsync(KitComponentProductionStationInput input) {
        var errors = new List<Error>();

        var kitComponent = await context.KitComponents.FirstOrDefaultAsync(t => t.Id == input.KitComponentId);
        if (kitComponent == null) {
            errors.Add(new Error("", $"kit component not found for {input.KitComponentId}"));
            return errors;
        }

        var productionStation = await context.ProductionStations.FirstOrDefaultAsync(t => t.Code == input.ProductionStationCode);
        if (productionStation == null) {
            errors.Add(new Error("", $"production station not found {input.ProductionStationCode}"));
            return errors;
        }

        if (kitComponent.ProductionStationId == productionStation.Id) {
            errors.Add(new Error("", $"production station is already set to {input.ProductionStationCode}"));
            return errors;
        }

        return errors;
    }

    #endregion

    #region set kit vin

    /// <summary>
    /// Update a Kit's VIN
    /// Note: A Kit's VIN can change, but a history is kept of all VINs associated with a Kit
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<Kit>> UpdateKitVinAsync(KitVinInput input) {

        MutationResult<Kit> result = new() {
            Errors = await ValidateUpdateKitVinAsync(input)
        };
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.Lot)
            .Include(t => t.KitStatusEvents)
            .Include(t => t.KitVins)
            .FirstAsync(t => t.KitNo == input.KitNo);

        kit.KitVins.Where(t => t.RemovedAt == null).ToList().ForEach(kitVin => {
            kitVin.RemovedAt = DateTime.UtcNow;
        });

        kit.AssignVIN(input.VIN);

        // save 
        await context.SaveChangesAsync();

        result.Payload = kit;
        return result;

    }

    // validate set kit vin
    // return Task<List<Error>> 
    // Kit not null, kit current vin is different
    // Kit.KitStatusEvents latest event is not BUILD_START or greater
    public async Task<List<Error>> ValidateUpdateKitVinAsync(KitVinInput input) {
        var errors = new List<Error>();
        var kit = await context.Kits.AsNoTracking()
            .Include(t => t.Lot)
            .Include(t => t.KitStatusEvents.Where(te => te.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.KitNo == input.KitNo);

        if (kit == null) {
            errors.Add(new Error("", $"kit not found for {input.KitNo}"));
            return errors;
        }

        // VIN cannot have spaces in it
        if (input.VIN.Contains(' ')) {
            errors.Add(new Error("", $"vin cannot contain spaces"));
            return errors;
        }
        // VIN must be EntityFieldLen.VIN length
        if (input.VIN.Length != EntityFieldLen.VIN) {
            errors.Add(new Error("", $"vin must be {EntityFieldLen.VIN} characters"));
            return errors;
        }

        if (kit.VIN == input.VIN) {
            errors.Add(new Error("", $"kit vin is already set to {input.VIN}"));
            return errors;
        }

        // VIN in use by another kit
        var otherKit = await context.Kits.AsNoTracking().FirstOrDefaultAsync(k => k.VIN == input.VIN);
        if (otherKit != null) {
            errors.Add(new Error("", $"VIN is already in use by kit {otherKit.KitNo}"));
            return errors;
        }

        // latest event must be PLAN_BUILD 
        var latestEvent = kit.KitStatusEvents.OrderByDescending(t => t.EventType.Sequence).FirstOrDefault();
        if (latestEvent != null) {
            if (latestEvent.EventType.Code != KitStatusCode.PLAN_BUILD) {
                errors.Add(new Error("", $"kit latest event is not PLAN_BUILD"));
                return errors;
            }
        }

        return errors;
    }

    #endregion

    #region set kit status event PartnerUpdatedAt

    public async Task<MutationResult<Kit>> SetPartnerUpdatedAtAsync(SetPartnerUpdatedInput input) {
        var result = new MutationResult<Kit>() {
            Errors = await ValidateSetPartnerUpdatedAtAsync(input)
        };
        if (result.Errors.Count > 0) {
            return result;
        }

        var kit = await context.Kits
            .Include(t => t.KitStatusEvents.Where(te => te.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstAsync(t => t.KitNo == input.kitNo);

        var kitStatusEvent = kit.KitStatusEvents.First(t => t.EventType.PartnerStatusCode == input.kitStatusCode);
        kitStatusEvent.PartnerStatusUpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return result;
    }

    public async Task<List<Error>> ValidateSetPartnerUpdatedAtAsync(SetPartnerUpdatedInput input) {
        var errors = new List<Error>();
        var kit = await context.Kits
            .Include(t => t.KitStatusEvents.Where(te => te.RemovedAt == null)).ThenInclude(t => t.EventType)
            .FirstOrDefaultAsync(t => t.KitNo == input.kitNo);

        if (kit == null) {
            errors.Add(new Error("", $"kit not found for {input.kitNo}"));
            return errors;
        }

        // has kit.StatusEvents has one where   kitStatusCode
        var hasKitStatusCode = kit.KitStatusEvents.Any(t => t.EventType.PartnerStatusCode == input.kitStatusCode);
        if (!hasKitStatusCode) {
            errors.Add(new Error("", $"kit does not have status event for {input.kitStatusCode}"));
            return errors;
        }

        return errors;
    }

    #endregion

    #region auto generate build-start events

    /// <summary>
    /// Auto generate BUILD_START event for kits that have a PLAN_BUILD and any component serials
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<GenerateBuildStartEventsPayload>> GenerateBuildStartEventsAsync(GenerateBuildStartEventsInput input) {
        var result = new MutationResult<GenerateBuildStartEventsPayload>() {
            Errors = await ValidateGenerateBuildStartEventsAsync(input),
            Payload = new GenerateBuildStartEventsPayload {
                PlantCode = input.PlantCode
            }
        };

        if (result.Errors.Count > 0) {
            return result;
        }
        var kits = await GetBuildStartPendingKits();

        var buildStartEventType = await context.KitStatusEventTypes.FirstAsync(t => t.Code == KitStatusCode.BUILD_START);

        foreach (var kit in kits) {
            var firstComponentSerial = kit.KitComponents.SelectMany(kc => kc.ComponentSerials).OrderBy(cs => cs.CreatedAt).First();

            kit.KitStatusEvents.Add(new KitStatusEvent() {
                EventType = buildStartEventType,
                CreatedAt = DateTime.UtcNow,
                EventDate = firstComponentSerial.CreatedAt
            });

            result.Payload.KitNos.Add(kit.KitNo);
        }

        // save
        await context.SaveChangesAsync();

        return result;

        async Task<List<Kit>> GetBuildStartPendingKits() => await context.Kits
            .Include(k => k.Lot).ThenInclude(k => k.Pcv)
            .Include(k => k.KitStatusEvents.Where(te => te.RemovedAt == null)).ThenInclude(k => k.EventType)
            .Include(k => k.KitComponents).ThenInclude(k => k.ComponentSerials.Where(cs => cs.RemovedAt == null))
            .Where(k => k.Lot.Plant.Code == input.PlantCode)
            .Where(k =>
                k.KitStatusEvents
                .Where(e => e.RemovedAt == null)
                .OrderByDescending(e => e.EventType.Sequence)
                .Select(e => e.EventType.Code)
                .FirstOrDefault() == KitStatusCode.PLAN_BUILD
            )
            .Where(k =>
                k.KitComponents
                    .Where(kc => kc.RemovedAt == null)
                    .SelectMany(kc => kc.ComponentSerials)
                        .Where(c => c.RemovedAt == null)
                        .Any()
            ).ToListAsync();

    }
    public async Task<List<Error>> ValidateGenerateBuildStartEventsAsync(GenerateBuildStartEventsInput input) {
        var errors = new List<Error>();

        var plant = await context.Plants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == input.PlantCode);

        if (plant == null) {
            errors.Add(new Error("", $"plant not found for {input.PlantCode}"));
            return errors;
        }

        return errors;
    }


    #endregion

    #region sync Kit.KitComponents
    /// <summary>
    /// Update kit comoponent stations mappings to match the PCV components and  ComponentStation mappings template
    /// Only apply to kits with no ComponentSerial entries
    /// </summary>
    public async Task<MutationResult<SyncKitWithPcvComponentsResult>> SynchronizeAllKitsWithPCVComponents() {
        MutationResult<SyncKitWithPcvComponentsResult> result = new();

        List<Kit> kits = await GeAlltKitsThatAreNotBuildCompleted();

        List<ComponentStation> componentStations = await context.ComponentStations
            .Where(cs => cs.RemovedAt == null)
            .ToListAsync();

        foreach (Kit kit in kits) {
            PCV pcv = await GetPcvWithPcvComponents(kit.Lot.PcvId);
            kit.AssignPcvComponents(pcv, componentStations);
        }

        await context.SaveChangesAsync();

        result.Payload = new SyncKitWithPcvComponentsResult(kits.Select(t => t.KitNo).ToList());
        return result;

        async Task<List<Kit>> GeAlltKitsThatAreNotBuildCompleted() {
            var buildCompletedSquence = await context.KitStatusEventTypes
                .Where(t => t.Code == KitStatusCode.BUILD_COMPLETED)
                .Select(t => t.Sequence).FirstAsync();

            return await context.Kits
                .Include(t => t.Lot).ThenInclude(t => t.Pcv)
                .Include(t => t.KitComponents).ThenInclude(t => t.Component)
                .Include(t => t.KitComponents).ThenInclude(t => t.ProductionStation)
                .Include(t => t.KitComponents).ThenInclude(t => t.ComponentSerials)
                .Where(k => k.RemovedAt == null)
                .Where(k => k.KitStatusEvents
                    .Where(te => te.RemovedAt == null)
                    .OrderByDescending(te => te.EventType.Sequence)
                    .Select(te => te.EventType.Sequence)
                    .FirstOrDefault() < buildCompletedSquence
                )
                .ToListAsync();
        }

        async Task<PCV> GetPcvWithPcvComponents(Guid pcvId) {
            return await context.Pcvs
                            .Include(p => p.PcvComponents)
                            .Where(p => p.Id == pcvId).FirstAsync();
        }
    }


    public async Task<MutationResult<SyncKitComponentsResult>> SynchronizeKitWithPCVComponents(SyncKitComponentsInput input) {

        Kit? kit = await context.Kits
            .Include(t => t.Lot).ThenInclude(t => t.Pcv)
            .Include(t => t.KitComponents.Where(t => t.RemovedAt == null)).ThenInclude(t => t.Component)
            .Include(t => t.KitComponents.Where(t => t.RemovedAt == null)).ThenInclude(t => t.ProductionStation)
            .Where(t => t.KitNo == input.KitNo).FirstOrDefaultAsync();

        // if is null return error
        if (kit == null) {
            return new MutationResult<SyncKitComponentsResult> {
                Errors = new List<Error> {
                    new Error("", $"kit not found for {input.KitNo}")
                }
            };
        }

        List<ComponentStation> componentStations = await context.ComponentStations
            .Where(cs => cs.RemovedAt == null)
            .ToListAsync();

        // add KitComponent mappings for each selected kit
        PCV pcv = await context.Pcvs
            .Include(p => p.PcvComponents)
            .Where(p => p.Id == kit.Lot.PcvId).FirstAsync();

        kit.AssignPcvComponents(pcv, componentStations);
        await context.SaveChangesAsync();

        kit = await context.Kits
            .Include(t => t.KitComponents.Where(t => t.RemovedAt == null))
            .ThenInclude(t => t.Component)
            .Where(t => t.KitNo == input.KitNo)
            .FirstAsync();

        return new MutationResult<SyncKitComponentsResult> {
            Payload = new SyncKitComponentsResult(input.KitNo, kit.KitComponents.Select(t => t.Component.Code).ToList())
        };
    }

    #endregion

    #region queries 

    /// <summary>
    /// Gets kits where status is BUILD_START status should be created
    /// Current status is PLAN_BUILD and has component serial input
    /// </summary>
    /// <param name="plantCode"></param>
    /// <returns></returns>
    public async Task<List<KitPayload>> GetBuildStartPendingKits(
        string plantCode
    ) {

        var result = await context.Kits.AsNoTracking()
            .Include(k => k.Lot).ThenInclude(k => k.Pcv)
            .Include(k => k.KitStatusEvents).ThenInclude(k => k.EventType)
            .Where(k => k.Lot.Plant.Code == plantCode)
            // Where latest event is PLAN_BUILD
            .Where(k =>
                k.KitStatusEvents
                .Where(e => e.RemovedAt == null)
                .OrderByDescending(e => e.EventType.Sequence)
                .Select(e => e.EventType.Code)
                .FirstOrDefault() == KitStatusCode.PLAN_BUILD
            )
            // where has a component serial input
            .Where(k =>
                k.KitComponents
                    .Where(kc => kc.RemovedAt == null)
                    .SelectMany(kc => kc.ComponentSerials)
                        .Where(c => c.RemovedAt == null)
                        .Any()
                )
            .Select(k => KitPayload.Create(k))
            .ToListAsync();

        return result;
    }



    #endregion

}

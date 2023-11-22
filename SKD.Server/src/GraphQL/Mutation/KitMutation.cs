#pragma warning disable CA1822
namespace SKD.Server;


[ExtendObjectType<Mutation>]
public class KitMutation {


    /// <summary>
    /// Create a timeline event for a kit
    /// </summary>
    /// <param name="input"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<KitStatusEvent>> CreateKitTimelineEventAsync(
        [Service] KitService service,
        KitStatusEventInput input
    ) => await service.CreateKitStatusEventAsync(input);


    /// <summary>
    /// Create build start event for a kit
    /// </summary>
    /// <param name="kitNo"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<KitStatusEvent>> CreateBuildStartEventAsync(
        [Service] KitService service,
        string kitNo
    ) => await service.CreateBuildStartEventAsync(kitNo);


    /// <summary>
    /// Create csutom received event for all kits in a lot
    /// </summary>
    /// <param name="input"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<Lot>> CreateLotTimelineEvent(
        [Service] KitService service,
        LotKitStatusEventInput input
    ) => await service.CreateLotKitStatusEventsAsync(input);

    /// <summary>
    /// Update kit component stations mappings to match the component station mappings template
    /// where kit does not have a BUILD_COMPLETE timeline event
    /// </summary>
    public async Task<SKD.Service.MutationResult<SyncKitWithPcvComponentsResult>> SyncKitsWithPcvComponents(
        [Service] KitService service
    ) => await service.SynchronizeAllKitsWithPCVComponents();


    public async Task<SKD.Service.MutationResult<SyncKitComponentsResult>> SynchronizeKitWithPCVComponents(
        [Service] KitService service,
        SyncKitComponentsInput input
    ) => await service.SynchronizeKitWithPCVComponents(input);
}
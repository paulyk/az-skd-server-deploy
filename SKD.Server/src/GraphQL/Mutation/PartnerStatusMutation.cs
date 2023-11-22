#pragma warning disable CA1822
using SKD.KitStatusFeed;


namespace SKD.Server;

[ExtendObjectType<Mutation>]
public class PartnerStatusMutation {


    /// <summary>
    /// Given a Kit number update the partner status with the kit's current statuses
    /// Does NOT set the KitTimelineEvent.PartnerStatusUpdatedAt
    /// </summary>
    /// <param name="partnerStatusService"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<UpdatePartnerStatusPayload>> UpdatePartnerStatusAsync(
        [Service] PartnerStatusService partnerStatusService,
        UpdatePartnerStatusInput input
    ) => await partnerStatusService.UpdatePartneStatusAsync(input);

    /// <summary>
    /// Update each KitTimelineEvent.PartnerStatusUpdatedAt based on the partners kit status.
    /// </summary>
    /// <param name="partnerStatusService"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<SKD.Service.MutationResult<UpdatePartnerStatusPayload>> SyncKitToPartnerStatusAsync(
        [Service] PartnerStatusService partnerStatusService,
        UpdatePartnerStatusInput input
    ) => await partnerStatusService.SyncKitToPartnerStatusAsync(input);

    /// <summary>
    /// Gets a VIN from the KitStatusFeedService and sets / updates  it to the kit
    /// Does not throw an error if the VIN is not found or if the VIN has not changed
    /// </summary>
    /// <param name="service"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<SKD.Service.MutationResult<UpdateKitVinPayload>> UpdateKitVinAsync    (
        [Service] PartnerStatusService service,
        UpdateKitVinInput input
    ) => await service.UpdateKitVinAsync(input);

}

namespace SKD.KitStatusFeed;

public class KitStatusFeedService {

    private readonly HttpClient _client;

    public KitStatusFeedService(
        IHttpClientFactory clientFactory
    ) {
        _client = clientFactory.CreateClient("KitStatusFeedService");
    }

    public async Task<KitStatusFeedResponse<KitCurrentStatusResponse>> GetCurrentStatusAsync(
        KitCurrentStatusRequest input
    ) {
        var result = new KitStatusFeedResponse<KitCurrentStatusResponse>();

        try {

            HttpResponseMessage response = await _client
                .PostAsJsonAsync(KitStatusFeedApiEndpoints.GetCurrentStatus, input);
            result.IsSuccess = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode) {
                result.Data = await response.Content.ReadFromJsonAsync<KitCurrentStatusResponse>();
            } else {
                KitStatusFeedErrorResponse? payload = await response.Content.ReadFromJsonAsync<KitStatusFeedErrorResponse>();
            }
        } catch (Exception ex) {
            throw new KitStatusFeedException("Error during operation KitStatusFeeg: get-current-status", ex);
        }

        return result;
    }

    public async Task<string> GetCurrentStatusCodeAsync(
        string kitNo
    ) {

        HttpResponseMessage response = await _client.PostAsJsonAsync(KitStatusFeedApiEndpoints.GetCurrentStatus, new KitCurrentStatusRequest {
            KitNumber = kitNo
        });

        if (!response.IsSuccessStatusCode) {
            throw new KitStatusFeedException("Error during operation KitStatusFeeg: get-current-status");
        }

        KitCurrentStatusResponse? data = await response.Content.ReadFromJsonAsync<KitCurrentStatusResponse>();

        return data?.Status ?? "";
    }

    public async Task<KitStatusFeedResponse<KitPVinResponse>> GetPvinAsync(
        KitPVinRequest input
    ) {
        KitStatusFeedResponse<KitPVinResponse> result = new();

        try {

            HttpResponseMessage response = await _client.PostAsJsonAsync(KitStatusFeedApiEndpoints.GetPhysicalVin, input);
            result.IsSuccess = response.IsSuccessStatusCode;

            if (response.IsSuccessStatusCode) {

                result.Data = await response.Content.ReadFromJsonAsync<KitPVinResponse>();

            } else {

                KitStatusFeedErrorResponse? errorPayload = await response.Content.ReadFromJsonAsync<KitStatusFeedErrorResponse>();
                throw new KitStatusFeedException("Error during operation KitStatusFeed: get-pvin", new Exception(errorPayload?.Error.Messages.FirstOrDefault()));
            }

        } catch (Exception ex) {
            throw new KitStatusFeedException("Error during operation KitStatusFeed: get-pvin", ex);
        }

        return result;
    }

    public async Task<KitStatusFeedResponse<KitProcessPartnerStatusResponse>> ProcessPartnerStatusAsync(
        KitProcessPartnerStatusRequest input
    ) {
        KitStatusFeedResponse<KitProcessPartnerStatusResponse> result = new ();

        try {

            HttpResponseMessage response = await _client.PostAsJsonAsync(KitStatusFeedApiEndpoints.ProcessPartnerStatus, input);

            result.IsSuccess = response.IsSuccessStatusCode;

            if (result.IsSuccess) {

                result.Data = await response.Content.ReadFromJsonAsync<KitProcessPartnerStatusResponse>();

            } else {

                KitStatusFeedErrorResponse? data = await response.Content.ReadFromJsonAsync<KitStatusFeedErrorResponse>();

            }

        } catch (Exception ex) {
            throw new KitStatusFeedException("Error during operation KitStatusFeed: process-partner-status", ex);
        }

        return result;
    }
}
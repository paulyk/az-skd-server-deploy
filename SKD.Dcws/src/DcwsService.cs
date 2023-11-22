using ServiceReference;
using static ServiceReference.HTTPDataCollectionSoapClient;

namespace SKD.Dcws;
public class DcwsService {
    private readonly HTTPDataCollectionSoapClient client;
    private readonly DCWSServiceOptions _serviceOptions;
    public DcwsService(DCWSServiceOptions serviceOptions) {
        var config = EndpointConfiguration.HTTPDataCollectionSoap;
        client = new HTTPDataCollectionSoapClient(config, serviceOptions.DcwsServiceAddress);
        _serviceOptions = serviceOptions;
    }

    public async Task<string> GetServiceVersion() {
        var result = await client.GetVersionAsync();
        return result.Body.GetVersionResult.DCWSCOMVersion;
    }
    public async Task<bool> CanConnectToService() {
        await client.CheckConnectivityAsync();
        return true;
    }

    public async Task<SubmitDcwsComponentRespnse> SubmitDcwsComponent(SubmitDcwsComponentInput input) {
        var result = await client.SaveCDCComponentAsync(
            vin: input.VIN,
            componentTypeCode: input.ComponentTypeCode,
            scan1: input.Serial1,
            scan2: input.Serial2,
            //
            acceptIfComponentNotRequired: _serviceOptions.AcceptIfComponentNotRequired,
            acceptIfInvalidScan: _serviceOptions.AcceptIfInvalidScan,
            acceptIfKnownBadComponent: _serviceOptions.AcceptIfKnownBadComponent,
            acceptIfNotVerified: _serviceOptions.AcceptIfNotVerified,
            acceptIfPartAlreadyInstalled: _serviceOptions.AcceptIfPartAlreadyInstalled,
            acceptIfVINNotFound: _serviceOptions.AcceptIfVINNotFound,
            acceptIfWrongComponent: _serviceOptions.AcceptIfWrongComponent
        );

        var processExecption = result.Body.SaveCDCComponentResult.ProcessException;
        return new SubmitDcwsComponentRespnse {
            VIN = input.VIN,
            ComponentTypeCode = input.ComponentTypeCode,
            Serial1 = input.Serial1,
            Serial2 = input.Serial2,
            ProcessExceptionCode = processExecption
        };
    }
}

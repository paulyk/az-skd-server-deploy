namespace SKD.Dcws;

public record DCWSServiceOptions(
    string DcwsServiceAddress = "",
    bool AcceptIfComponentNotRequired = false,
    bool AcceptIfInvalidScan = false,
    bool AcceptIfKnownBadComponent = false,
    bool AcceptIfNotVerified = false,
    bool AcceptIfPartAlreadyInstalled = true,
    bool AcceptIfVINNotFound = true,
    bool AcceptIfWrongComponent = false
);


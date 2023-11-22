namespace SKD.Domain;

public enum KitStatusCode {
    CUSTOM_RECEIVED = 1,       // FPCR
    PLAN_BUILD,                // FPBP
    BUILD_START,                // FPBS  
    BUILD_COMPLETED,           // FPBC
    GATE_RELEASED,             // FPGR
    WHOLE_SALE                 // FPWS
}

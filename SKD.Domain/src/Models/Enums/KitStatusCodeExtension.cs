namespace SKD.Domain;

public static class KitStatusCodeExtensions {


    public static KitStatusCode ToPartnerStatusCode(this PartnerStatusCode code) {
        return (KitStatusCode)(int)code;
    }

    public static PartnerStatusCode ToKitTimelineCode(this KitStatusCode code) {
        return (PartnerStatusCode)(int)code;
    }

}

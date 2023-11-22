namespace SKD.Common;
public partial class ValidationUtil {

    public static bool Valid_KitNo(string kitNo) {
        var regex = KitNoRegex();
        var result = regex.Match(kitNo ?? "");
        return result.Success;
    }

    public static bool Valid_LotNo(string lotNo) {
        var regex = LotNoRegex();
        var result = regex.Match(lotNo ?? "");
        return result.Success;
    }

    [GeneratedRegex("[A-Z0-9]{17}")]
    private static partial Regex KitNoRegex();
    [GeneratedRegex("[A-Z0-9]{15}")]
    private static partial Regex LotNoRegex();
}

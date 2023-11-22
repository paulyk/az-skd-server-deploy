
namespace SKD.Queries;

public record LotPartDiffQueryResult(
    string FirstLotNo,
    string SecondLotNo,
    List<LotPartDiffItem> FirstLotOnlyParts,
    List<LotPartDiffItem> SecondLotOnlyParts,
    string ErrorMessage = ""
);

public record LotPartDiffItem(
    string PartNo,
    string PartDesc
);

using SKD.Application.Common;

namespace SKD.Queries;

public class LotPartDiffService: IAppService {
    // _context SkdContext
    private readonly SkdContext _context;
    public LotPartDiffService(SkdContext context) {
        _context = context;
    }

    public async Task<LotPartDiffQueryResult> HandleAsync(LotPartDiffInput input) {

        string validationError = ValidateLotPartDiff(input);
        if (validationError != null) {
            return new LotPartDiffQueryResult(
                input.FirstLotNo,
                input.SecondLotNo,
                new List<LotPartDiffItem>(),
                new List<LotPartDiffItem>(),
                validationError
            );
        }

        List<Part> priorLotParts = await GetLotParts(input.FirstLotNo);
        List<Part> nextLotParts = await GetLotParts(input.SecondLotNo);

        List<LotPartDiffItem> lotOneOnlyParts = priorLotParts.Except(nextLotParts).ToList()
            .Select(t => new LotPartDiffItem(
            t.PartNo,
            t.PartDesc
        )).ToList();

        List<LotPartDiffItem> lotTwoOnlyParts = nextLotParts.Except(priorLotParts).ToList()
        .Select(t => new LotPartDiffItem(
            t.PartNo,
            t.PartDesc
        )).ToList();

        return new LotPartDiffQueryResult(
            input.FirstLotNo,
            input.SecondLotNo,
            lotOneOnlyParts,
            lotTwoOnlyParts
        );
    }

    private async Task<List<Part>> GetLotParts(string lotNo) {
        return await _context.Lots
                    .Where(t => t.LotNo == lotNo)
                    .SelectMany(t => t.LotParts)
                    .Select(p => p.Part)
                    .ToListAsync();
    }

    public string ValidateLotPartDiff(LotPartDiffInput input) {

        // Valid Lot No is 17 chars
        if (input.FirstLotNo.Length != 17 || input.SecondLotNo.Length != 17) {
            return "lot no must be 17 characters";
        }

        // both lots must exist
        string[] lots = new string[] { input.FirstLotNo, input.SecondLotNo };
        List<string> notFoundLots = lots.Where(t => !_context.Lots.Any(l => l.LotNo == t)).ToList();
        if (notFoundLots.Count != 0) {
            return $"the following lots were not found: {string.Join(",", notFoundLots)}";
        }

        // PcvCode slice must be the same (7 chars)
        string priorLotPvcCode = input.FirstLotNo[..7];
        string nextLotPvcCode = input.SecondLotNo[..7];
        if (priorLotPvcCode != nextLotPvcCode) {
            return $"prior lot pcv code {priorLotPvcCode} must be the same as subsequent lot pcv code {nextLotPvcCode}";
        }

        return null;
    }

}
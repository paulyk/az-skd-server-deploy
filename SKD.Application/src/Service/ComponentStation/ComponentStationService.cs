#nullable enable

using SKD.Application.Common;

namespace SKD.Service;

public class ComponentStationService: IAppService {

    private readonly SkdContext context;

    public ComponentStationService(SkdContext context) {
        this.context = context;
    }

    /// <summary>
    /// Assign production stations to components
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<MutationResult<SetComponentStationMappingsPayload>> SetComponentStationMappings(
        ComponentStationMappingsInput input
    ) {
        var result = new MutationResult<SetComponentStationMappingsPayload> {
            Errors = await ValidateSetComponentStationMappings(input)
        };
        if (result.Errors.Count != 0) {
            return result;
        }

        var allComponentStations = await context.ComponentStations
            .Include(t => t.Component)
            .Include(t => t.Station)
            .ToListAsync();
        var allComponents = await context.Components.ToListAsync();
        var allProductionStations = await context.ProductionStations.ToListAsync();

        // remove exsiting component station mappings where component code is in input
        var componentCodes = input.Mappings.Select(m => m.ComponentCode).Distinct().ToList();
        var componentStationsToRemove = allComponentStations
            .Where(cs => componentCodes.Contains(cs.Component.Code))
            .ToList();
        context.ComponentStations.RemoveRange(componentStationsToRemove);

        // add new mappings
        foreach (var mapping in input.Mappings) {
            var component = allComponents.Single(c => c.Code == mapping.ComponentCode);
            var station = allProductionStations.Single(s => s.Code == mapping.StationCode);
            context.ComponentStations.Add(new ComponentStation {
                Component = component,
                Station = station,
                SaveCDCComponent = mapping.SaveCDCComponent,
            });
        }

        // save changes
        await context.SaveChangesAsync();
        result.Payload = new SetComponentStationMappingsPayload {
            Mappings = input.Mappings
        };
        return result;
    }


    private async Task<List<Error>> ValidateSetComponentStationMappings(ComponentStationMappingsInput input) {
        var errors = new List<Error>();
        await Task.Delay(100);

        if (input.Mappings.Count == 0) {
            errors.Add(new Error("No mappings provided"));
            return errors;
        }

        if (
            input.Mappings.Any(m => string.IsNullOrWhiteSpace(m.ComponentCode))
            || input.Mappings.Any(m => string.IsNullOrWhiteSpace(m.StationCode))
        ) {
            errors.Add(new Error("Component or Station code cannot be blank"));
        }

        var inputComponentCodes = input.Mappings.Select(m => m.ComponentCode).Distinct().ToList();
        var foundComponentCodes = await context.Components
            .Where(t => t.RemovedAt == null)
            .Where(c => inputComponentCodes.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync();
        var missingComponentCodes = inputComponentCodes.Except(foundComponentCodes).ToList();
        // error if missing
        if (missingComponentCodes.Count > 0) {
            errors.Add(new Error($"Component codes not found: {string.Join(", ", missingComponentCodes)}"));
        }

        // missing production station codes
        var inputStationCodes = input.Mappings.Select(m => m.StationCode).Distinct().ToList();
        var foundStationCodes = await context.ProductionStations
            .Where(t => t.RemovedAt == null)
            .Where(s => inputStationCodes.Contains(s.Code))
            .Select(s => s.Code)
            .ToListAsync();
        var missingStationCodes = inputStationCodes.Except(foundStationCodes).ToList();
        // error if missing
        if (missingStationCodes.Count > 0) {
            errors.Add(new Error($"Station codes not found: {string.Join(", ", missingStationCodes)}"));
        }

        return errors;
    }

    /// <summary>
    /// Remove all component station mappings
    /// </summary>
    /// <returns></returns>
    public async Task<MutationResult<RemoveAllComponentStationMappingsPayload>> RemoveAllComponentStationMappings() {
        var result = new MutationResult<RemoveAllComponentStationMappingsPayload>();
        var componentStations = await context.ComponentStations.ToListAsync();
        context.ComponentStations.RemoveRange(componentStations);
        await context.SaveChangesAsync();
        result.Payload = new RemoveAllComponentStationMappingsPayload {
            RemovedCount = componentStations.Count
        };
        return result;
    }
}


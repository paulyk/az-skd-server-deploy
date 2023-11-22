using SKD.Domain;
namespace SKD.Seed {
    internal class SeedDataGenerator {
        private readonly SkdContext ctx;

        public SeedDataGenerator(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task Seed_KitStatusCodes() {

            // in order by when they should occur
            var eventTypes = new List<KitStatusEventType> {
                new KitStatusEventType {
                    Code = KitStatusCode.CUSTOM_RECEIVED,
                },
                new KitStatusEventType {
                    Code = KitStatusCode.PLAN_BUILD,
                },
                new KitStatusEventType {
                    Code = KitStatusCode.BUILD_COMPLETED,
                },
                new KitStatusEventType {
                    Code = KitStatusCode.GATE_RELEASED,
                },
                new KitStatusEventType {
                    Code = KitStatusCode.WHOLE_SALE,
                },
            };

            var sequence = 1;
            eventTypes.ForEach(eventType => {
                eventType.Description = UnderscoreToPascalCase(eventType.Code.ToString());
                eventType.Sequence = sequence++;
            });

            ctx.KitStatusEventTypes.AddRange(eventTypes);
            await ctx.SaveChangesAsync();
        }

        public async Task Seed_ProductionStations(ICollection<ProductionStation_Mock_DTO> data) {
            var stations = data.ToList().Select(x => new ProductionStation() {
                Code = x.Code,
                Name = x.Name,
                Sequence = x.SortOrder,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.ProductionStations.AddRange(stations);
            await ctx.SaveChangesAsync();
        }
        public async Task Seed_Components(ICollection<Component_MockData_DTO> componentData) {
            var components = componentData.ToList().Select(x => new Component() {
                Code = x.Code,
                Name = x.Name,
                CreatedAt = Util.RandomDateTime(DateTime.UtcNow)
            });

            ctx.Components.AddRange(components);
            await ctx.SaveChangesAsync();
        }

        private static string UnderscoreToPascalCase(string input) {
            var str = input.Split("_").Aggregate((x, y) => x + "  " + y);
            return str.Substring(0, 1).ToUpper() + str[1..].ToLower();
        }

    }
}
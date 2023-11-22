using SKD.Domain;
using Microsoft.EntityFrameworkCore;

namespace SKD.Seed {
    public class SeedDataService {

        readonly SkdContext ctx;
        public SeedDataService(SkdContext ctx) {
            this.ctx = ctx;
        }

        public async Task GenerateReferenceData() {

            // drop & create
            var dbService = new DbService(ctx);
            await dbService.MigrateDb();

            if (await ctx.KitStatusEventTypes.AnyAsync()) {
                // already seeded
                return;
            }

            // seed            
            var seedData = new SeedData();

            var generator = new SeedDataGenerator(ctx);
            await generator.Seed_KitStatusCodes();
            await generator.Seed_Components(seedData.Component_MockData);
            await generator.Seed_ProductionStations(seedData.ProductionStation_MockData);
        }
    }
}

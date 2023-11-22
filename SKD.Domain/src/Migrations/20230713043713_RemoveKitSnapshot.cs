using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Domain.src.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKitSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kit_snapshot");

            migrationBuilder.DropTable(
                name: "partner_status_ack");

            migrationBuilder.DropTable(
                name: "kit_snapshot_run");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kit_snapshot_run",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RunDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kit_snapshot_run", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kit_snapshot_run_plant_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kit_snapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    KitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KitSnapshotRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KitTimeLineEventTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildCompleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeStatusCode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DealerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngineSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GateRelease = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrginalPlanBuild = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanBuild = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VIN = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    VerifyVIN = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Wholesale = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kit_snapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kit_snapshot_kit_KitId",
                        column: x => x.KitId,
                        principalTable: "kit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_kit_snapshot_kit_snapshot_run_KitSnapshotRunId",
                        column: x => x.KitSnapshotRunId,
                        principalTable: "kit_snapshot_run",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_kit_snapshot_kit_timeline_event_type_KitTimeLineEventTypeId",
                        column: x => x.KitTimeLineEventTypeId,
                        principalTable: "kit_timeline_event_type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partner_status_ack",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 36, nullable: false),
                    KitSnapshotRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAccepted = table.Column<int>(type: "int", nullable: false),
                    TotalProcessed = table.Column<int>(type: "int", nullable: false),
                    TotalRejected = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_status_ack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_partner_status_ack_kit_snapshot_run_KitSnapshotRunId",
                        column: x => x.KitSnapshotRunId,
                        principalTable: "kit_snapshot_run",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kit_snapshot_KitId",
                table: "kit_snapshot",
                column: "KitId");

            migrationBuilder.CreateIndex(
                name: "IX_kit_snapshot_KitSnapshotRunId_KitId",
                table: "kit_snapshot",
                columns: new[] { "KitSnapshotRunId", "KitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kit_snapshot_KitTimeLineEventTypeId",
                table: "kit_snapshot",
                column: "KitTimeLineEventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_kit_snapshot_run_PlantId_RunDate",
                table: "kit_snapshot_run",
                columns: new[] { "PlantId", "RunDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kit_snapshot_run_PlantId_Sequence",
                table: "kit_snapshot_run",
                columns: new[] { "PlantId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_partner_status_ack_KitSnapshotRunId",
                table: "partner_status_ack",
                column: "KitSnapshotRunId",
                unique: true);
        }
    }
}

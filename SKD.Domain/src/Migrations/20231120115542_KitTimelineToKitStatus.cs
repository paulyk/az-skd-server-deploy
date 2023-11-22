using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SKD.Domain.src.Migrations
{
    /// <inheritdoc />
    public partial class KitTimelineToKitStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kit_timeline_event_kit_timeline_event_type_KitTimelineEventTypeId",
                table: "kit_timeline_event");

            migrationBuilder.RenameColumn(
                name: "KitTimelineEventTypeId",
                table: "kit_timeline_event",
                newName: "KitStatusEventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_KitTimelineEventTypeId",
                table: "kit_timeline_event",
                newName: "IX_kit_timeline_event_KitStatusEventTypeId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PartnerStatusUpdatedAt",
                table: "kit_timeline_event",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EventDate",
                table: "kit_timeline_event",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_kit_timeline_event_EventDate",
                table: "kit_timeline_event",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_kit_timeline_event_RemovedAt",
                table: "kit_timeline_event",
                column: "RemovedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_kit_timeline_event_kit_timeline_event_type_KitStatusEventTypeId",
                table: "kit_timeline_event",
                column: "KitStatusEventTypeId",
                principalTable: "kit_timeline_event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kit_timeline_event_kit_timeline_event_type_KitStatusEventTypeId",
                table: "kit_timeline_event");

            migrationBuilder.DropIndex(
                name: "IX_kit_timeline_event_EventDate",
                table: "kit_timeline_event");

            migrationBuilder.DropIndex(
                name: "IX_kit_timeline_event_RemovedAt",
                table: "kit_timeline_event");

            migrationBuilder.RenameColumn(
                name: "KitStatusEventTypeId",
                table: "kit_timeline_event",
                newName: "KitTimelineEventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_kit_timeline_event_KitStatusEventTypeId",
                table: "kit_timeline_event",
                newName: "IX_kit_timeline_event_KitTimelineEventTypeId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PartnerStatusUpdatedAt",
                table: "kit_timeline_event",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "kit_timeline_event",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddForeignKey(
                name: "FK_kit_timeline_event_kit_timeline_event_type_KitTimelineEventTypeId",
                table: "kit_timeline_event",
                column: "KitTimelineEventTypeId",
                principalTable: "kit_timeline_event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

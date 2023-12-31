﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace SKD.Domain.src.Migrations
{
    public partial class OriginalSerial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Original_Serial1",
                table: "component_serial",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Original_Serial1",
                table: "component_serial");
        }
    }
}

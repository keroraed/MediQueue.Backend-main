using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediQueue.Repository.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffFieldsToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HospitalId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "AspNetUsers");
        }
    }
}

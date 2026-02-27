using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediQueue.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationFeeAndPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConsultationFee",
                table: "ClinicProfiles",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethods",
                table: "ClinicProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationFee",
                table: "ClinicProfiles");

            migrationBuilder.DropColumn(
                name: "PaymentMethods",
                table: "ClinicProfiles");
        }
    }
}

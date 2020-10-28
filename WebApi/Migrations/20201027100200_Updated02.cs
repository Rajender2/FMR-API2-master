using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.Migrations
{
    public partial class Updated02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_City_State_Stateid",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_State_Country_Countryid",
                table: "State");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddress_Address_Addressid",
                table: "UserAddress");

            migrationBuilder.RenameColumn(
                name: "Stateid",
                table: "City",
                newName: "StateId");

            migrationBuilder.RenameIndex(
                name: "IX_City_Stateid",
                table: "City",
                newName: "IX_City_StateId");

            migrationBuilder.AlterColumn<int>(
                name: "Addressid",
                table: "UserAddress",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Countryid",
                table: "State",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "FormTemplate",
                nullable: true,
                oldClrType: typeof(bool));

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "FormTemplate",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "StateId",
                table: "City",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumePath",
                table: "Assessment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Address",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JobMCQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    JobOrderId = table.Column<int>(nullable: false),
                    AddedOn = table.Column<DateTime>(nullable: false),
                    AddedById = table.Column<long>(nullable: false),
                    OrderById = table.Column<int>(nullable: true),
                    QuestionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobMCQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobMCQuestion_FormTemplate_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "FormTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobOrderDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    JobOrderId = table.Column<int>(nullable: false),
                    DocumentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrderDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOrderDocuments_DocumentTemplate_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "DocumentTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobOrderDocuments_JobOrder_JobOrderId",
                        column: x => x.JobOrderId,
                        principalTable: "JobOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormTemplate_CompanyId",
                table: "FormTemplate",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobMCQuestion_QuestionId",
                table: "JobMCQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderDocuments_DocumentId",
                table: "JobOrderDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrderDocuments_JobOrderId",
                table: "JobOrderDocuments",
                column: "JobOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_City_State_StateId",
                table: "City",
                column: "StateId",
                principalTable: "State",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormTemplate_Company_CompanyId",
                table: "FormTemplate",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_State_Country_Countryid",
                table: "State",
                column: "Countryid",
                principalTable: "Country",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddress_Address_Addressid",
                table: "UserAddress",
                column: "Addressid",
                principalTable: "Address",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_City_State_StateId",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_FormTemplate_Company_CompanyId",
                table: "FormTemplate");

            migrationBuilder.DropForeignKey(
                name: "FK_State_Country_Countryid",
                table: "State");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddress_Address_Addressid",
                table: "UserAddress");

            migrationBuilder.DropTable(
                name: "JobMCQuestion");

            migrationBuilder.DropTable(
                name: "JobOrderDocuments");

            migrationBuilder.DropIndex(
                name: "IX_FormTemplate_CompanyId",
                table: "FormTemplate");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "FormTemplate");

            migrationBuilder.DropColumn(
                name: "ResumePath",
                table: "Assessment");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Address");

            migrationBuilder.RenameColumn(
                name: "StateId",
                table: "City",
                newName: "Stateid");

            migrationBuilder.RenameIndex(
                name: "IX_City_StateId",
                table: "City",
                newName: "IX_City_Stateid");

            migrationBuilder.AlterColumn<int>(
                name: "Addressid",
                table: "UserAddress",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Countryid",
                table: "State",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "FormTemplate",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Stateid",
                table: "City",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_City_State_Stateid",
                table: "City",
                column: "Stateid",
                principalTable: "State",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_State_Country_Countryid",
                table: "State",
                column: "Countryid",
                principalTable: "Country",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddress_Address_Addressid",
                table: "UserAddress",
                column: "Addressid",
                principalTable: "Address",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

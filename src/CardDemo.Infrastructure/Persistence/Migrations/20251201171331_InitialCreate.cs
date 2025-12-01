using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardDemo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddressLine3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StateCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhoneNumber1 = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PhoneNumber2 = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SSN = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    GovernmentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    FICOScore = table.Column<int>(type: "int", nullable: false),
                    EFTAccountId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PrimaryCardHolder = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCategories",
                columns: table => new
                {
                    CategoryCode = table.Column<int>(type: "int", nullable: false),
                    CategoryDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategories", x => x.CategoryCode);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ActiveStatus = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false, defaultValue: "Y"),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashCreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OpenDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "date", nullable: false),
                    ReissueDate = table.Column<DateTime>(type: "date", nullable: true),
                    CurrentCycleCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    CurrentCycleDebit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0.00m),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Accounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    TypeCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    TypeDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryCode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.TypeCode);
                    table.ForeignKey(
                        name: "FK_TransactionTypes_TransactionCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "TransactionCategories",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CardNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CardType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EmbossedName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ActiveStatus = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false, defaultValue: "Y"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CardNumber);
                    table.ForeignKey(
                        name: "FK_Cards_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CategoryCode = table.Column<int>(type: "int", nullable: false),
                    TransactionSource = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    MerchantName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MerchantCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MerchantZip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OrigTransactionId = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedFlag = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false, defaultValue: "N"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Cards_CardNumber",
                        column: x => x.CardNumber,
                        principalTable: "Cards",
                        principalColumn: "CardNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionCategories_CategoryCode",
                        column: x => x.CategoryCode,
                        principalTable: "TransactionCategories",
                        principalColumn: "CategoryCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionTypes_TransactionType",
                        column: x => x.TransactionType,
                        principalTable: "TransactionTypes",
                        principalColumn: "TypeCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ActiveStatus",
                table: "Accounts",
                column: "ActiveStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CustomerId",
                table: "Accounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_AccountId",
                table: "Cards",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ActiveStatus",
                table: "Cards",
                column: "ActiveStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GovernmentId",
                table: "Customers",
                column: "GovernmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SSN",
                table: "Customers",
                column: "SSN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId_TransactionDate",
                table: "Transactions",
                columns: new[] { "AccountId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CardNumber",
                table: "Transactions",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryCode",
                table: "Transactions",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ProcessedFlag",
                table: "Transactions",
                column: "ProcessedFlag");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "Transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionType",
                table: "Transactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_CategoryCode",
                table: "TransactionTypes",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                table: "Users",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "TransactionCategories");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}

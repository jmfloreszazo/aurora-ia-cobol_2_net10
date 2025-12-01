using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CardDemoDbContext context)
    {
        // Seed Transaction Categories
        if (!await context.TransactionCategories.AnyAsync())
        {
            var categories = new[]
            {
                new TransactionCategory { CategoryCode = 1001, CategoryDescription = "Groceries" },
                new TransactionCategory { CategoryCode = 1002, CategoryDescription = "Gas & Fuel" },
                new TransactionCategory { CategoryCode = 1003, CategoryDescription = "Restaurants" },
                new TransactionCategory { CategoryCode = 1004, CategoryDescription = "Entertainment" },
                new TransactionCategory { CategoryCode = 1005, CategoryDescription = "Shopping" },
                new TransactionCategory { CategoryCode = 1006, CategoryDescription = "Travel" },
                new TransactionCategory { CategoryCode = 1007, CategoryDescription = "Healthcare" },
                new TransactionCategory { CategoryCode = 1008, CategoryDescription = "Utilities" },
                new TransactionCategory { CategoryCode = 1009, CategoryDescription = "Insurance" },
                new TransactionCategory { CategoryCode = 1010, CategoryDescription = "Other" }
            };

            await context.TransactionCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Seed Transaction Types
        if (!await context.TransactionTypes.AnyAsync())
        {
            var transactionTypes = new[]
            {
                new TransactionType { TypeCode = "01", TypeDescription = "Purchase", CategoryCode = 1010 },
                new TransactionType { TypeCode = "02", TypeDescription = "Cash Advance", CategoryCode = 1010 },
                new TransactionType { TypeCode = "03", TypeDescription = "Balance Transfer", CategoryCode = 1010 },
                new TransactionType { TypeCode = "04", TypeDescription = "Payment", CategoryCode = 1010 },
                new TransactionType { TypeCode = "05", TypeDescription = "Refund", CategoryCode = 1010 },
                new TransactionType { TypeCode = "06", TypeDescription = "Fee", CategoryCode = 1010 },
                new TransactionType { TypeCode = "07", TypeDescription = "Interest Charge", CategoryCode = 1010 }
            };

            await context.TransactionTypes.AddRangeAsync(transactionTypes);
            await context.SaveChangesAsync();
        }

        // Seed Default Admin User
        if (!await context.Users.AnyAsync(u => u.UserId == "ADMIN"))
        {
            var adminUser = new User
            {
                UserId = "ADMIN",
                // Password: Admin@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                UserType = UserRole.ADMIN,
                IsActive = true,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // Seed Sample User
        if (!await context.Users.AnyAsync(u => u.UserId == "USER01"))
        {
            var sampleUser = new User
            {
                UserId = "USER01",
                // Password: User@123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FirstName = "John",
                LastName = "Doe",
                UserType = UserRole.USER,
                IsActive = true,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            await context.Users.AddAsync(sampleUser);
            await context.SaveChangesAsync();
        }

        // Seed Sample Customer
        if (!await context.Customers.AnyAsync())
        {
            var sampleCustomer = new Customer
            {
                FirstName = "John",
                MiddleName = "A",
                LastName = "Doe",
                DateOfBirth = new DateTime(1985, 5, 15),
                GovernmentId = "DL123456789",
                SSN = "123456789",
                PhoneNumber1 = "(555) 123-4567",
                PhoneNumber2 = "(555) 987-6543",
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Apt 4B",
                StateCode = "NY",
                ZipCode = "10001",
                CountryCode = "USA",
                FICOScore = 750,
                PrimaryCardHolder = true
            };

            await context.Customers.AddAsync(sampleCustomer);
            await context.SaveChangesAsync();

            // Seed Sample Account for the Customer
            var sampleAccount = new Account
            {
                CustomerId = sampleCustomer.CustomerId,
                ActiveStatus = "Y",
                CurrentBalance = 2500.00m,
                CreditLimit = 10000.00m,
                CashCreditLimit = 2000.00m,
                OpenDate = DateTime.UtcNow.AddYears(-2),
                ExpirationDate = DateTime.UtcNow.AddYears(3),
                ReissueDate = DateTime.UtcNow.AddYears(-1),
                CurrentCycleCredit = 150.00m,
                CurrentCycleDebit = 500.00m,
                ZipCode = "10001",
                GroupId = "GRP001"
            };

            await context.Accounts.AddAsync(sampleAccount);
            await context.SaveChangesAsync();

            // Seed Sample Card for the Account
            var sampleCard = new Card
            {
                CardNumber = "4111111111111111",
                AccountId = sampleAccount.AccountId,
                CardType = "VISA",
                EmbossedName = "JOHN A DOE",
                ExpirationDate = "12/2028",
                ActiveStatus = "Y"
            };

            await context.Cards.AddAsync(sampleCard);
            await context.SaveChangesAsync();

            // Seed Sample Transactions
            var sampleTransactions = new[]
            {
                new Transaction
                {
                    TransactionId = "TXN0000000001",
                    AccountId = sampleAccount.AccountId,
                    CardNumber = sampleCard.CardNumber,
                    TransactionType = "01",
                    CategoryCode = 1001,
                    TransactionSource = "Online",
                    Description = "Grocery Store Purchase",
                    Amount = 125.50m,
                    MerchantId = "MERCH001",
                    MerchantName = "SuperMart",
                    MerchantCity = "New York",
                    MerchantZip = "10001",
                    OrigTransactionId = null,
                    TransactionDate = DateTime.UtcNow.AddDays(-5),
                    ProcessedFlag = "Y"
                },
                new Transaction
                {
                    TransactionId = "TXN0000000002",
                    AccountId = sampleAccount.AccountId,
                    CardNumber = sampleCard.CardNumber,
                    TransactionType = "01",
                    CategoryCode = 1002,
                    TransactionSource = "POS",
                    Description = "Gas Station",
                    Amount = 45.00m,
                    MerchantId = "MERCH002",
                    MerchantName = "Shell Gas Station",
                    MerchantCity = "New York",
                    MerchantZip = "10002",
                    OrigTransactionId = null,
                    TransactionDate = DateTime.UtcNow.AddDays(-3),
                    ProcessedFlag = "Y"
                },
                new Transaction
                {
                    TransactionId = "TXN0000000003",
                    AccountId = sampleAccount.AccountId,
                    CardNumber = sampleCard.CardNumber,
                    TransactionType = "01",
                    CategoryCode = 1003,
                    TransactionSource = "Online",
                    Description = "Restaurant Dining",
                    Amount = 85.75m,
                    MerchantId = "MERCH003",
                    MerchantName = "The Italian Kitchen",
                    MerchantCity = "New York",
                    MerchantZip = "10003",
                    OrigTransactionId = null,
                    TransactionDate = DateTime.UtcNow.AddDays(-2),
                    ProcessedFlag = "Y"
                },
                new Transaction
                {
                    TransactionId = "TXN0000000004",
                    AccountId = sampleAccount.AccountId,
                    CardNumber = sampleCard.CardNumber,
                    TransactionType = "04",
                    CategoryCode = 1010,
                    TransactionSource = "Online",
                    Description = "Account Payment",
                    Amount = -500.00m,
                    MerchantId = "BANK001",
                    MerchantName = "CardDemo Bank",
                    MerchantCity = "New York",
                    MerchantZip = "10001",
                    OrigTransactionId = null,
                    TransactionDate = DateTime.UtcNow.AddDays(-1),
                    ProcessedFlag = "Y"
                }
            };

            await context.Transactions.AddRangeAsync(sampleTransactions);
            await context.SaveChangesAsync();
        }
    }
}

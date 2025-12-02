using CardDemo.Domain.Entities;
using CardDemo.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CardDemoDbContext context)
    {
        // Seed Transaction Categories
        await SeedTransactionCategoriesAsync(context);

        // Seed Transaction Types
        await SeedTransactionTypesAsync(context);

        // Seed Users
        await SeedUsersAsync(context);

        // Seed Customers, Accounts, Cards and Transactions
        await SeedCustomersWithAccountsAsync(context);
    }

    private static async Task SeedTransactionCategoriesAsync(CardDemoDbContext context)
    {
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
    }

    private static async Task SeedTransactionTypesAsync(CardDemoDbContext context)
    {
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
    }

    private static async Task SeedUsersAsync(CardDemoDbContext context)
    {
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

        // Seed Additional Test Users
        if (!await context.Users.AnyAsync(u => u.UserId == "USER02"))
        {
            var testUsers = new[]
            {
                new User
                {
                    UserId = "USER02",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    FirstName = "Jane",
                    LastName = "Smith",
                    UserType = UserRole.USER,
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                },
                new User
                {
                    UserId = "USER03",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    FirstName = "Robert",
                    LastName = "Johnson",
                    UserType = UserRole.USER,
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                },
                new User
                {
                    UserId = "LOCKED",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Locked@123"),
                    FirstName = "Locked",
                    LastName = "Account",
                    UserType = UserRole.USER,
                    IsActive = true,
                    IsLocked = true,
                    FailedLoginAttempts = 5
                },
                new User
                {
                    UserId = "INACTIVE",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Inactive@123"),
                    FirstName = "Inactive",
                    LastName = "User",
                    UserType = UserRole.USER,
                    IsActive = false,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                }
            };

            await context.Users.AddRangeAsync(testUsers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCustomersWithAccountsAsync(CardDemoDbContext context)
    {
        // Check if customers already exist
        if (await context.Customers.AnyAsync())
        {
            return;
        }

        // Create multiple customers with different profiles
        var customers = new List<Customer>
        {
            new Customer
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
            },
            new Customer
            {
                FirstName = "Jane",
                MiddleName = "B",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 8, 22),
                GovernmentId = "DL987654321",
                SSN = "987654321",
                PhoneNumber1 = "(555) 234-5678",
                PhoneNumber2 = null,
                AddressLine1 = "456 Oak Avenue",
                AddressLine2 = null,
                StateCode = "CA",
                ZipCode = "90210",
                CountryCode = "USA",
                FICOScore = 820,
                PrimaryCardHolder = true
            },
            new Customer
            {
                FirstName = "Robert",
                MiddleName = "C",
                LastName = "Johnson",
                DateOfBirth = new DateTime(1978, 12, 3),
                GovernmentId = "DL456789123",
                SSN = "456789123",
                PhoneNumber1 = "(555) 345-6789",
                PhoneNumber2 = "(555) 876-5432",
                AddressLine1 = "789 Pine Road",
                AddressLine2 = "Suite 100",
                StateCode = "TX",
                ZipCode = "75001",
                CountryCode = "USA",
                FICOScore = 680,
                PrimaryCardHolder = true
            },
            new Customer
            {
                FirstName = "Maria",
                MiddleName = "D",
                LastName = "Garcia",
                DateOfBirth = new DateTime(1995, 3, 10),
                GovernmentId = "DL321654987",
                SSN = "321654987",
                PhoneNumber1 = "(555) 456-7890",
                PhoneNumber2 = null,
                AddressLine1 = "321 Elm Street",
                AddressLine2 = "Unit 5",
                StateCode = "FL",
                ZipCode = "33101",
                CountryCode = "USA",
                FICOScore = 700,
                PrimaryCardHolder = true
            },
            new Customer
            {
                FirstName = "Michael",
                MiddleName = "E",
                LastName = "Brown",
                DateOfBirth = new DateTime(1982, 7, 28),
                GovernmentId = "DL654987321",
                SSN = "654987321",
                PhoneNumber1 = "(555) 567-8901",
                PhoneNumber2 = "(555) 765-4321",
                AddressLine1 = "567 Maple Drive",
                AddressLine2 = null,
                StateCode = "IL",
                ZipCode = "60601",
                CountryCode = "USA",
                FICOScore = 720,
                PrimaryCardHolder = true
            }
        };

        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();

        // Create accounts for each customer
        var accounts = new List<Account>();
        var cards = new List<Card>();
        var transactions = new List<Transaction>();
        var random = new Random(42); // Fixed seed for reproducible data

        foreach (var customer in customers)
        {
            // Each customer gets 1-2 accounts
            var numAccounts = customer.CustomerId % 2 == 0 ? 2 : 1;
            
            for (int i = 0; i < numAccounts; i++)
            {
                var creditLimit = (random.Next(5, 25) * 1000m); // $5,000 to $25,000
                var currentBalance = Math.Round(creditLimit * (decimal)(random.NextDouble() * 0.6), 2); // 0-60% utilization

                var account = new Account
                {
                    CustomerId = customer.CustomerId,
                    ActiveStatus = "Y",
                    CurrentBalance = currentBalance,
                    CreditLimit = creditLimit,
                    CashCreditLimit = creditLimit * 0.2m,
                    OpenDate = DateTime.UtcNow.AddYears(-random.Next(1, 5)),
                    ExpirationDate = DateTime.UtcNow.AddYears(random.Next(2, 5)),
                    ReissueDate = DateTime.UtcNow.AddYears(-1),
                    CurrentCycleCredit = Math.Round(creditLimit * 0.05m, 2),
                    CurrentCycleDebit = Math.Round(currentBalance * 0.3m, 2),
                    ZipCode = customer.ZipCode,
                    GroupId = $"GRP{customer.CustomerId:D3}"
                };

                accounts.Add(account);
            }
        }

        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();

        // Create cards for each account
        var cardTypes = new[] { "VISA", "MASTERCARD", "AMEX", "DISCOVER" };
        var cardNumberBase = 4111000000000000L;

        foreach (var account in accounts)
        {
            // Each account gets 1-2 cards
            var numCards = random.Next(1, 3);
            var customer = customers.First(c => c.CustomerId == account.CustomerId);

            for (int i = 0; i < numCards; i++)
            {
                cardNumberBase++;
                var card = new Card
                {
                    CardNumber = cardNumberBase.ToString(),
                    AccountId = account.AccountId,
                    CardType = cardTypes[random.Next(cardTypes.Length)],
                    EmbossedName = $"{customer.FirstName} {customer.MiddleName} {customer.LastName}".ToUpper(),
                    ExpirationDate = account.ExpirationDate.ToString("MM/yyyy"),
                    ActiveStatus = "Y"
                };

                cards.Add(card);
            }
        }

        await context.Cards.AddRangeAsync(cards);
        await context.SaveChangesAsync();

        // Create transactions for each card
        var merchants = new[]
        {
            ("MERCH001", "SuperMart", "New York", "10001", 1001),
            ("MERCH002", "Shell Gas Station", "Los Angeles", "90001", 1002),
            ("MERCH003", "The Italian Kitchen", "Chicago", "60601", 1003),
            ("MERCH004", "AMC Theaters", "Miami", "33101", 1004),
            ("MERCH005", "Best Buy Electronics", "Dallas", "75001", 1005),
            ("MERCH006", "Delta Airlines", "Atlanta", "30301", 1006),
            ("MERCH007", "CVS Pharmacy", "Boston", "02101", 1007),
            ("MERCH008", "Electric Company", "Seattle", "98101", 1008),
            ("MERCH009", "State Farm Insurance", "Phoenix", "85001", 1009),
            ("MERCH010", "Amazon.com", "Seattle", "98101", 1005),
            ("MERCH011", "Walmart", "Denver", "80201", 1001),
            ("MERCH012", "Starbucks", "Portland", "97201", 1003)
        };

        var transactionCounter = 1;

        foreach (var card in cards)
        {
            // Each card gets 5-15 transactions
            var numTransactions = random.Next(5, 16);

            for (int i = 0; i < numTransactions; i++)
            {
                var merchant = merchants[random.Next(merchants.Length)];
                var daysAgo = random.Next(1, 90);
                var amount = Math.Round((decimal)(random.NextDouble() * 200 + 10), 2); // $10 to $210
                var isPayment = random.Next(10) == 0; // 10% chance of payment

                var transaction = new Transaction
                {
                    TransactionId = $"TXN{transactionCounter:D10}",
                    AccountId = card.AccountId,
                    CardNumber = card.CardNumber,
                    TransactionType = isPayment ? "04" : "01",
                    CategoryCode = isPayment ? 1010 : merchant.Item5,
                    TransactionSource = random.Next(2) == 0 ? "Online" : "POS",
                    Description = isPayment ? "Account Payment" : $"Purchase at {merchant.Item2}",
                    Amount = isPayment ? -amount * 2 : amount,
                    MerchantId = isPayment ? "BANK001" : merchant.Item1,
                    MerchantName = isPayment ? "CardDemo Bank" : merchant.Item2,
                    MerchantCity = isPayment ? "New York" : merchant.Item3,
                    MerchantZip = isPayment ? "10001" : merchant.Item4,
                    OrigTransactionId = null,
                    TransactionDate = DateTime.UtcNow.AddDays(-daysAgo),
                    ProcessedFlag = "Y"
                };

                transactions.Add(transaction);
                transactionCounter++;
            }
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();

        Console.WriteLine($"[DatabaseSeeder] Seeded: {customers.Count} customers, {accounts.Count} accounts, {cards.Count} cards, {transactions.Count} transactions");
    }
}

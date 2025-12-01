using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardDemo.Application.Common.Interfaces;

public interface ICardDemoDbContext
{
    DbSet<User> Users { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Card> Cards { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<TransactionType> TransactionTypes { get; }
    DbSet<TransactionCategory> TransactionCategories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using CardDemo.Application.Common.Interfaces;
using CardDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CardDemo.Infrastructure.Persistence;

public class CardDemoDbContext : DbContext, ICardDemoDbContext
{
    public CardDemoDbContext(DbContextOptions<CardDemoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

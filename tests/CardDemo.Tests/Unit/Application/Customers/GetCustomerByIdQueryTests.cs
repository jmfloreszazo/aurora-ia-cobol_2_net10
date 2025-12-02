using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Customers.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Customers;

public class GetCustomerByIdQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetCustomerByIdQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "John", 
            LastName = "Doe",
            MiddleName = "Michael",
            DateOfBirth = new DateTime(1980, 5, 15),
            SSN = "123-45-6789",
            GovernmentId = "A123456",
            PhoneNumber1 = "555-1234",
            PhoneNumber2 = "555-5678",
            AddressLine1 = "123 Main St",
            AddressLine2 = "Apt 101",
            StateCode = "CA",
            ZipCode = "90210",
            CountryCode = "USA",
            FICOScore = 750,
            Accounts = new List<Account> { new Account(), new Account() }
        };

        var customers = new List<Customer> { customer };
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetCustomerByIdQueryHandler(_mockContext.Object);
        var query = new GetCustomerByIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.MiddleName.Should().Be("Michael");
        result.FullName.Should().Be("John Michael Doe");
        result.FICOScore.Should().Be(750);
        result.NumberOfAccounts.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerNotFound()
    {
        // Arrange
        var customers = new List<Customer>();
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetCustomerByIdQueryHandler(_mockContext.Object);
        var query = new GetCustomerByIdQuery(999);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectCustomer_WhenMultipleCustomersExist()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer 
            { 
                CustomerId = 1, 
                FirstName = "John", 
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 5, 15),
                SSN = "123-45-6789",
                GovernmentId = "A123456",
                PhoneNumber1 = "555-1234",
                AddressLine1 = "123 Main St",
                StateCode = "CA",
                ZipCode = "90210",
                CountryCode = "USA",
                FICOScore = 750,
                Accounts = new List<Account>()
            },
            new Customer 
            { 
                CustomerId = 2, 
                FirstName = "Jane", 
                LastName = "Smith",
                DateOfBirth = new DateTime(1985, 8, 20),
                SSN = "987-65-4321",
                GovernmentId = "B654321",
                PhoneNumber1 = "555-5678",
                AddressLine1 = "456 Oak Ave",
                StateCode = "NY",
                ZipCode = "10001",
                CountryCode = "USA",
                FICOScore = 800,
                Accounts = new List<Account>()
            }
        };

        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetCustomerByIdQueryHandler(_mockContext.Object);
        var query = new GetCustomerByIdQuery(2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(2);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Handle_ShouldIncludeAllCustomerDetails()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "Alice", 
            LastName = "Johnson",
            MiddleName = null,
            DateOfBirth = new DateTime(1990, 3, 25),
            SSN = "555-55-5555",
            GovernmentId = "C789012",
            PhoneNumber1 = "555-9999",
            PhoneNumber2 = null,
            AddressLine1 = "789 Pine Rd",
            AddressLine2 = null,
            StateCode = "TX",
            ZipCode = "75001",
            CountryCode = "USA",
            FICOScore = 820,
            Accounts = new List<Account>()
        };

        var customers = new List<Customer> { customer };
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetCustomerByIdQueryHandler(_mockContext.Object);
        var query = new GetCustomerByIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(1);
        result.FirstName.Should().Be("Alice");
        result.MiddleName.Should().BeNull();
        result.LastName.Should().Be("Johnson");
        result.FullName.Should().Be("Alice Johnson");
        result.DateOfBirth.Should().Be(new DateTime(1990, 3, 25));
        result.SSN.Should().Be("555-55-5555");
        result.GovernmentId.Should().Be("C789012");
        result.PhoneNumber1.Should().Be("555-9999");
        result.PhoneNumber2.Should().BeNull();
        result.AddressLine1.Should().Be("789 Pine Rd");
        result.AddressLine2.Should().BeNull();
        result.StateCode.Should().Be("TX");
        result.ZipCode.Should().Be("75001");
        result.CountryCode.Should().Be("USA");
        result.FICOScore.Should().Be(820);
        result.NumberOfAccounts.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCountAccountsCorrectly()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "Bob", 
            LastName = "Wilson",
            DateOfBirth = new DateTime(1975, 11, 10),
            SSN = "111-22-3333",
            GovernmentId = "D111222",
            PhoneNumber1 = "555-0000",
            AddressLine1 = "100 Elm St",
            StateCode = "FL",
            ZipCode = "33101",
            CountryCode = "USA",
            FICOScore = 680,
            Accounts = new List<Account>
            {
                new Account { AccountId = 1 },
                new Account { AccountId = 2 },
                new Account { AccountId = 3 },
                new Account { AccountId = 4 },
                new Account { AccountId = 5 }
            }
        };

        var customers = new List<Customer> { customer };
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetCustomerByIdQueryHandler(_mockContext.Object);
        var query = new GetCustomerByIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.NumberOfAccounts.Should().Be(5);
    }
}

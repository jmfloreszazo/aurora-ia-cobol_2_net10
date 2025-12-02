using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Customers.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Customers;

public class GetAllCustomersQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetAllCustomersQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedCustomers_WhenCustomersExist()
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
                Accounts = new List<Account> { new Account() }
            },
            new Customer 
            { 
                CustomerId = 2, 
                FirstName = "Jane", 
                LastName = "Smith",
                MiddleName = "Marie",
                DateOfBirth = new DateTime(1985, 8, 20),
                SSN = "987-65-4321",
                GovernmentId = "B654321",
                PhoneNumber1 = "555-5678",
                AddressLine1 = "456 Oak Ave",
                StateCode = "NY",
                ZipCode = "10001",
                CountryCode = "USA",
                FICOScore = 800,
                Accounts = new List<Account> { new Account(), new Account() }
            }
        };

        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCustomersExist()
    {
        // Arrange
        var customers = new List<Customer>();
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        var customers = Enumerable.Range(1, 25).Select(i => new Customer
        {
            CustomerId = i,
            FirstName = $"First{i}",
            LastName = $"Last{i}",
            DateOfBirth = new DateTime(1980, 1, 1),
            SSN = $"123-45-{i:D4}",
            GovernmentId = $"ID{i}",
            PhoneNumber1 = "555-0000",
            AddressLine1 = $"{i} Main St",
            StateCode = "CA",
            ZipCode = "90210",
            CountryCode = "USA",
            FICOScore = 700 + i,
            Accounts = new List<Account>()
        }).ToList();

        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(2, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldOrderByLastNameThenFirstName()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer 
            { 
                CustomerId = 1, 
                FirstName = "Zack", 
                LastName = "Alpha",
                DateOfBirth = new DateTime(1980, 1, 1),
                SSN = "111-11-1111",
                GovernmentId = "A1",
                PhoneNumber1 = "555-1111",
                AddressLine1 = "1 Main St",
                StateCode = "CA",
                ZipCode = "90210",
                CountryCode = "USA",
                FICOScore = 700,
                Accounts = new List<Account>()
            },
            new Customer 
            { 
                CustomerId = 2, 
                FirstName = "Amy", 
                LastName = "Zebra",
                DateOfBirth = new DateTime(1985, 1, 1),
                SSN = "222-22-2222",
                GovernmentId = "A2",
                PhoneNumber1 = "555-2222",
                AddressLine1 = "2 Main St",
                StateCode = "NY",
                ZipCode = "10001",
                CountryCode = "USA",
                FICOScore = 750,
                Accounts = new List<Account>()
            },
            new Customer 
            { 
                CustomerId = 3, 
                FirstName = "Adam", 
                LastName = "Alpha",
                DateOfBirth = new DateTime(1990, 1, 1),
                SSN = "333-33-3333",
                GovernmentId = "A3",
                PhoneNumber1 = "555-3333",
                AddressLine1 = "3 Main St",
                StateCode = "TX",
                ZipCode = "75001",
                CountryCode = "USA",
                FICOScore = 800,
                Accounts = new List<Account>()
            }
        };

        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        // Should be ordered: Adam Alpha, Zack Alpha, Amy Zebra
        result.Items[0].FirstName.Should().Be("Adam");
        result.Items[1].FirstName.Should().Be("Zack");
        result.Items[2].FirstName.Should().Be("Amy");
    }

    [Fact]
    public async Task Handle_ShouldIncludeFullNameWithMiddleName()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "John", 
            MiddleName = "Michael",
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
        };

        var customers = new List<Customer> { customer };
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("John Michael Doe");
    }

    [Fact]
    public async Task Handle_ShouldIncludeFullNameWithoutMiddleName()
    {
        // Arrange
        var customer = new Customer 
        { 
            CustomerId = 1, 
            FirstName = "John", 
            MiddleName = null,
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
        };

        var customers = new List<Customer> { customer };
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_ShouldIncludeNumberOfAccounts()
    {
        // Arrange
        var customer = new Customer 
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
            Accounts = new List<Account> 
            { 
                new Account { AccountId = 1 }, 
                new Account { AccountId = 2 },
                new Account { AccountId = 3 }
            }
        };

        var customers = new List<Customer> { customer };
        var mockDbSet = customers.BuildMockDbSet();
        _mockContext.Setup(c => c.Customers).Returns(mockDbSet.Object);

        var handler = new GetAllCustomersQueryHandler(_mockContext.Object);
        var query = new GetAllCustomersQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().NumberOfAccounts.Should().Be(3);
    }
}

using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Cards.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Cards;

public class GetCardsByAccountQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetCardsByAccountQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCards_WhenAccountHasCards()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        
        var cards = new List<Card>
        {
            new Card 
            { 
                CardNumber = "4111111111111111",
                AccountId = 1,
                Account = account,
                CardType = "VISA",
                EmbossedName = "JOHN DOE",
                ExpirationDate = "12/2025",
                ActiveStatus = "Y"
            },
            new Card 
            { 
                CardNumber = "5500000000000004",
                AccountId = 1,
                Account = account,
                CardType = "MASTERCARD",
                EmbossedName = "JOHN DOE",
                ExpirationDate = "6/2026",
                ActiveStatus = "Y"
            }
        };

        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.AccountId.Should().Be(1));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAccountHasNoCards()
    {
        // Arrange
        var cards = new List<Card>();
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(999);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMaskCardNumber_WhenCardHasValidNumber()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111",
            AccountId = 1,
            Account = account,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2025",
            ActiveStatus = "Y"
        };

        var cards = new List<Card> { card };
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().MaskedCardNumber.Should().Be("**** **** **** 1111");
        result.First().CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyAccountCards_WhenMultipleAccountsExist()
    {
        // Arrange
        var account1 = new Account { AccountId = 1, CustomerId = 1 };
        var account2 = new Account { AccountId = 2, CustomerId = 2 };
        
        var cards = new List<Card>
        {
            new Card 
            { 
                CardNumber = "4111111111111111",
                AccountId = 1,
                Account = account1,
                CardType = "VISA",
                EmbossedName = "JOHN DOE",
                ExpirationDate = "12/2025",
                ActiveStatus = "Y"
            },
            new Card 
            { 
                CardNumber = "5500000000000004",
                AccountId = 2,
                Account = account2,
                CardType = "MASTERCARD",
                EmbossedName = "JANE SMITH",
                ExpirationDate = "6/2026",
                ActiveStatus = "Y"
            },
            new Card 
            { 
                CardNumber = "4222222222222222",
                AccountId = 1,
                Account = account1,
                CardType = "VISA",
                EmbossedName = "JOHN DOE",
                ExpirationDate = "3/2027",
                ActiveStatus = "Y"
            }
        };

        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.AccountId.Should().Be(1));
        result.Select(c => c.CardNumber).Should().NotContain("5500000000000004");
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCardDetails()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        
        var card = new Card 
        { 
            CardNumber = "4111111111111111",
            AccountId = 1,
            Account = account,
            CardType = "VISA PLATINUM",
            EmbossedName = "ALICE JOHNSON",
            ExpirationDate = "12/2025",
            ActiveStatus = "Y"
        };

        var cards = new List<Card> { card };
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var dto = result.First();
        dto.CardNumber.Should().Be("4111111111111111");
        dto.MaskedCardNumber.Should().Be("**** **** **** 1111");
        dto.AccountId.Should().Be(1);
        dto.CardType.Should().Be("VISA PLATINUM");
        dto.EmbossedName.Should().Be("ALICE JOHNSON");
        dto.ExpirationDate.Should().Be("12/2025");
        dto.ActiveStatus.Should().Be("Y");
    }

    [Fact]
    public async Task Handle_ShouldHandleShortCardNumber()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        
        var card = new Card 
        { 
            CardNumber = "123",
            AccountId = 1,
            Account = account,
            CardType = "TEST",
            EmbossedName = "TEST USER",
            ExpirationDate = "12/2025",
            ActiveStatus = "Y"
        };

        var cards = new List<Card> { card };
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        // Card number less than 4 characters should return the card number as is
        result.First().MaskedCardNumber.Should().Be("123");
    }

    [Fact]
    public async Task Handle_ShouldIncludeInactiveCards()
    {
        // Arrange
        var account = new Account { AccountId = 1, CustomerId = 1 };
        
        var cards = new List<Card>
        {
            new Card 
            { 
                CardNumber = "4111111111111111",
                AccountId = 1,
                Account = account,
                CardType = "VISA",
                EmbossedName = "JOHN DOE",
                ExpirationDate = "12/2025",
                ActiveStatus = "Y"
            },
            new Card 
            { 
                CardNumber = "4222222222222222",
                AccountId = 1,
                Account = account,
                CardType = "VISA",
                EmbossedName = "JOHN DOE",
                ExpirationDate = "12/2023",
                ActiveStatus = "N"
            }
        };

        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardsByAccountQueryHandler(_mockContext.Object);
        var query = new GetCardsByAccountQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.ActiveStatus == "Y");
        result.Should().Contain(c => c.ActiveStatus == "N");
    }
}

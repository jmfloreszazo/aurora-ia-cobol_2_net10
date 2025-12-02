using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Cards.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Cards;

public class GetAllCardsQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetAllCardsQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedCards_WhenCardsExist()
    {
        // Arrange
        var cards = new List<Card>
        {
            new Card { CardNumber = "4111111111111111", AccountId = 1, CardType = "VISA", EmbossedName = "JOHN DOE", ExpirationDate = "12/2028", ActiveStatus = "Y" },
            new Card { CardNumber = "5500000000000004", AccountId = 1, CardType = "MASTERCARD", EmbossedName = "JANE DOE", ExpirationDate = "06/2027", ActiveStatus = "Y" },
            new Card { CardNumber = "340000000000009", AccountId = 2, CardType = "AMEX", EmbossedName = "BOB SMITH", ExpirationDate = "03/2026", ActiveStatus = "N" }
        };

        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetAllCardsQueryHandler(_mockContext.Object);
        var query = new GetAllCardsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldMaskCardNumbers()
    {
        // Arrange
        var cards = new List<Card>
        {
            new Card { CardNumber = "4111111111111111", AccountId = 1, CardType = "VISA", EmbossedName = "JOHN DOE", ExpirationDate = "12/2028", ActiveStatus = "Y" }
        };

        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetAllCardsQueryHandler(_mockContext.Object);
        var query = new GetAllCardsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().MaskedCardNumber.Should().Contain("****");
        result.Items.First().MaskedCardNumber.Should().EndWith("1111");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCardsExist()
    {
        // Arrange
        var cards = new List<Card>();
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetAllCardsQueryHandler(_mockContext.Object);
        var query = new GetAllCardsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

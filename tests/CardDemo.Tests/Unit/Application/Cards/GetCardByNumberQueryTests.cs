using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Cards.Queries;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Cards;

public class GetCardByNumberQueryTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public GetCardByNumberQueryTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCard_WhenCardExists()
    {
        // Arrange
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2028",
            ActiveStatus = "Y"
        };

        var cards = new List<Card> { card };
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardByNumberQueryHandler(_mockContext.Object);
        var query = new GetCardByNumberQuery("4111111111111111");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CardNumber.Should().Be("4111111111111111");
        result.CardType.Should().Be("VISA");
        result.EmbossedName.Should().Be("JOHN DOE");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCardNotFound()
    {
        // Arrange
        var cards = new List<Card>();
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardByNumberQueryHandler(_mockContext.Object);
        var query = new GetCardByNumberQuery("9999999999999999");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMaskCardNumber()
    {
        // Arrange
        var card = new Card
        {
            CardNumber = "4111111111111111",
            AccountId = 1,
            CardType = "VISA",
            EmbossedName = "JOHN DOE",
            ExpirationDate = "12/2028",
            ActiveStatus = "Y"
        };

        var cards = new List<Card> { card };
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new GetCardByNumberQueryHandler(_mockContext.Object);
        var query = new GetCardByNumberQuery("4111111111111111");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.MaskedCardNumber.Should().Contain("****");
        result.MaskedCardNumber.Should().EndWith("1111");
    }
}

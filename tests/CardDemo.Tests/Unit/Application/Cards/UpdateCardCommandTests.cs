using CardDemo.Application.Common.Interfaces;
using CardDemo.Application.Features.Cards.Commands;
using CardDemo.Domain.Entities;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace CardDemo.Tests.Unit.Application.Cards;

public class UpdateCardCommandTests
{
    private readonly Mock<ICardDemoDbContext> _mockContext;

    public UpdateCardCommandTests()
    {
        _mockContext = new Mock<ICardDemoDbContext>();
    }

    [Fact]
    public async Task Handle_ShouldUpdateEmbossedName_WhenValidRequest()
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
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCardCommandHandler(_mockContext.Object);
        var command = new UpdateCardCommand("4111111111111111", null, "JOHN A DOE", null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EmbossedName.Should().Be("JOHN A DOE");
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateActiveStatus_WhenValidRequest()
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
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCardCommandHandler(_mockContext.Object);
        var command = new UpdateCardCommand("4111111111111111", null, null, null, "N");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ActiveStatus.Should().Be("N");
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCardNotFound()
    {
        // Arrange
        var cards = new List<Card>();
        var mockDbSet = cards.BuildMockDbSet();
        _mockContext.Setup(c => c.Cards).Returns(mockDbSet.Object);

        var handler = new UpdateCardCommandHandler(_mockContext.Object);
        var command = new UpdateCardCommand("9999999999999999", null, "NEW NAME", null, null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateExpirationDate_WhenValidRequest()
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
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCardCommandHandler(_mockContext.Object);
        var command = new UpdateCardCommand("4111111111111111", null, null, "12/2030", null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ExpirationDate.Should().Be("12/2030");
    }
}

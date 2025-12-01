using CardDemo.Application.Common.DTOs;
using CardDemo.Application.Common.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CardDemo.Tests.Unit.Application;

public class AuthValidatorsTests
{
    [Fact]
    public void LoginRequestValidator_ShouldPass_WhenValidData()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest("USER01", "Password@123");

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginRequestValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest("", "Password@123");

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void LoginRequestValidator_ShouldFail_WhenPasswordIsEmpty()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest("USER01", "");

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_ShouldPass_WhenValidData()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(
            "USER02",
            "Password@123",
            "Password@123",
            "John",
            "Doe",
            "USER"
        );

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterRequestValidator_ShouldFail_WhenPasswordTooShort()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(
            "USER02",
            "Pass1",
            "Pass1",
            "John",
            "Doe",
            "USER"
        );

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_ShouldFail_WhenPasswordMissingUppercase()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(
            "USER02",
            "password@123",
            "password@123",
            "John",
            "Doe",
            "USER"
        );

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_ShouldFail_WhenPasswordMissingLowercase()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(
            "USER02",
            "PASSWORD@123",
            "PASSWORD@123",
            "John",
            "Doe",
            "USER"
        );

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_ShouldFail_WhenPasswordMissingDigit()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest(
            "USER02",
            "Password@",
            "Password@",
            "John",
            "Doe",
            "USER"
        );

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    // Note: Current validator doesn't require special characters in password
    // Only requires: minimum 8 chars, uppercase, lowercase, and digit
}

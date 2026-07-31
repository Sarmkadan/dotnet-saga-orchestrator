using Xunit;
public class RetryPolicyValidationTests
{
    // Test methods go here
    [Fact]
    public void Validate_MaxRetries_Must_Be_Non_Negative()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxRetries = -1
        };

        // Act
        var problems = RetryPolicyValidation.Validate(policy);

        // Assert
        Assert.Single(problems, $"MaxRetries must be non-negative, but was -1.");
    }

    [Fact]
    public void Validate_InitialDelayMs_Must_Be_Non_Negative()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            InitialDelayMs = -1
        };

        // Act
        var problems = RetryPolicyValidation.Validate(policy);

        // Assert
        Assert.Single(problems, $"InitialDelayMs must be non-negative, but was -1.");
    }

    [Fact]
    public void Validate_BackoffMultiplier_Must_Be_At_Least_One()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            BackoffMultiplier = 0.5
        };

        // Act
        var problems = RetryPolicyValidation.Validate(policy);

        // Assert
        Assert.Single(problems, $"BackoffMultiplier must be >= 1.0, but was 0.5.");
    }

    [Fact]
    public void Validate_MaxDelayMs_Must_Be_Non_Negative()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxDelayMs = -1
        };

        // Act
        var problems = RetryPolicyValidation.Validate(policy);

        // Assert
        Assert.Single(problems, $"MaxDelayMs must be non-negative, but was -1.");
    }

    [Fact]
    public void Validate_MaxDelayMs_Must_Be_At_Least_InitialDelayMs()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            InitialDelayMs = 10,
            MaxDelayMs = 5
        };

        // Act
        var problems = RetryPolicyValidation.Validate(policy);

        // Assert
        Assert.Single(problems, $"MaxDelayMs (5) must be >= InitialDelayMs (10).");
    }
}
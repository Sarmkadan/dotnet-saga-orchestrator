#nullable enable

using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Application.Validators;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Contains unit tests for validating <see cref="SagaDefinitionValidator"/> and <see cref="SagaRequestValidator"/> classes.
/// Tests cover validation of saga definitions, saga step definitions, and saga creation requests.
/// </summary>
public class SagaDefinitionValidatorTests
{
    /// <summary>
    /// Creates a valid <see cref="SagaStepDefinition"/> instance for testing purposes.
    /// </summary>
    /// <param name="name">The step name. Defaults to "PaymentStep".</param>
    /// <returns>A configured <see cref="SagaStepDefinition"/> with valid properties.</returns>
    private static SagaStepDefinition CreateValidStep(string name = "PaymentStep") =>
        new SagaStepDefinition(name, "payment-svc", "http://payment-svc/charge", "http://payment-svc/refund")
        {
            Order = 1,
            TimeoutSeconds = 30,
            MaxRetries = 3
        };

    /// <summary>
    /// Creates a valid <see cref="SagaDefinition"/> instance for testing purposes.
    /// </summary>
    /// <param name="name">The saga name. Defaults to "OrderSaga".</param>
    /// <returns>A configured <see cref="SagaDefinition"/> with valid properties and one step.</returns>
    private static SagaDefinition CreateValidDefinition(string name = "OrderSaga") =>
        new SagaDefinition(name, "Order processing saga")
        {
            Steps = new List<SagaStepDefinition> { CreateValidStep() }
        };

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAsync(SagaDefinition)"/> does not throw when validating a valid saga definition.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidDefinition_DoesNotThrow()
    {
        var validator = new SagaDefinitionValidator();
        var definition = CreateValidDefinition();

        var act = () => validator.ValidateAsync(definition);

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAsync(SagaDefinition)"/> throws <see cref="InvalidSagaDefinitionException"/> when validating an invalid saga definition.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidDefinition_Throws()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("", "")
        {
            Steps = new List<SagaStepDefinition>()
        };

        var act = () => validator.ValidateAsync(definition);

        await act.Should().ThrowAsync<InvalidSagaDefinitionException>();
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when the saga name is null or empty.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_NullName_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("", "description")
        {
            Steps = new List<SagaStepDefinition> { CreateValidStep() }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*name*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when the saga name exceeds 255 characters.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_NameTooLong_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var longName = new string('x', 256);
        var definition = new SagaDefinition(longName, "description")
        {
            Steps = new List<SagaStepDefinition> { CreateValidStep() }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*exceed 255*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when the saga has no steps.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_NoSteps_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition>()
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*at least one step*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when the saga has more than 100 steps.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_TooManySteps_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = Enumerable.Range(1, 101)
                .Select(i => CreateValidStep($"Step{i}"))
                .ToList()
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*cannot have more than 100*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has an invalid name.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_InvalidStepName_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("", "svc", "http://svc/action", "http://svc/comp")
        {
            TimeoutSeconds = 30,
            MaxRetries = 3
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*Step 1*").And.ContainMatch("*Name*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has an invalid service URL.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_InvalidServiceUrl_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("PayStep", "svc", "not-a-url", "http://svc/comp")
        {
            TimeoutSeconds = 30,
            MaxRetries = 3
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*URL is not valid*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has an invalid compensation URL.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_InvalidCompensationUrl_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("PayStep", "svc", "http://svc/action", "invalid-url")
        {
            TimeoutSeconds = 30,
            MaxRetries = 3
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*Compensation URL*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has a timeout of zero seconds.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_TimeoutZero_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("PayStep", "svc", "http://svc/action", "http://svc/comp")
        {
            TimeoutSeconds = 0,
            MaxRetries = 3
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*greater than 0*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has a timeout exceeding 3600 seconds.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_TimeoutTooLarge_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("PayStep", "svc", "http://svc/action", "http://svc/comp")
        {
            TimeoutSeconds = 3601,
            MaxRetries = 3
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*cannot exceed 3600*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has negative retry count.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_NegativeRetries_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("PayStep", "svc", "http://svc/action", "http://svc/comp")
        {
            TimeoutSeconds = 30,
            MaxRetries = -1
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*cannot be negative*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when a step has more than 10 retries.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_TooManyRetries_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = new SagaStepDefinition("PayStep", "svc", "http://svc/action", "http://svc/comp")
        {
            TimeoutSeconds = 30,
            MaxRetries = 11
        };
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*cannot exceed 10*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when multiple steps have the same order value.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_DuplicateStepOrder_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step1 = CreateValidStep("Step1");
        var step2 = CreateValidStep("Step2");
        step2.Order = step1.Order;

        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step1, step2 }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*same order*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when step orders do not start from 1.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_OrderDoesNotStartAtOne_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var step = CreateValidStep();
        step.Order = 5;

        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*start from 1*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns all validation errors when multiple issues are present.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_MultipleErrors_ReturnsAll()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("", "") // missing name
        {
            Steps = new List<SagaStepDefinition>() // missing steps
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().HaveCountGreaterThan(1);
        errors.Should().ContainMatch("*name*").And.ContainMatch("*step*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAsync(SagaDefinition)"/> throws <see cref="InvalidSagaDefinitionException"/> with a message containing all validation errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_ThrowsWithAllErrors_InExceptionMessage()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("", "")
        {
            Steps = new List<SagaStepDefinition>()
        };

        var act = () => validator.ValidateAsync(definition);

        var exception = await act.Should().ThrowAsync<InvalidSagaDefinitionException>();
        exception.Which.Message.Should().Match("*is invalid*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> throws NullReferenceException when the saga definition is null.
    /// The validator does not explicitly check for null definition parameter.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_NullDefinition_Throws()
    {
        var validator = new SagaDefinitionValidator();
        SagaDefinition? definition = null;

        var act = () => validator.ValidateAndGetErrorsAsync(definition!);

        await act.Should().ThrowAsync<NullReferenceException>();
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> returns an error when the saga has empty steps list.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_EmptySteps_ReturnsError()
    {
        var validator = new SagaDefinitionValidator();
        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition>()
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().ContainMatch("*at least one step*");
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAndGetErrorsAsync(SagaDefinition)"/> does not return errors for duplicate step names.
    /// The validator does not check for duplicate step names, only duplicate order numbers.
    /// </summary>
    [Fact]
    public async Task ValidateAndGetErrorsAsync_DuplicateStepNames_NoErrorsReturned()
    {
        var validator = new SagaDefinitionValidator();
        var step1 = CreateValidStep("DuplicateStep");
        var step2 = CreateValidStep("DuplicateStep");
        step2.Order = 2;

        var definition = new SagaDefinition("OrderSaga", "description")
        {
            Steps = new List<SagaStepDefinition> { step1, step2 }
        };

        var errors = await validator.ValidateAndGetErrorsAsync(definition);

        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="SagaDefinitionValidator.ValidateAsync(SagaDefinition)"/> does not throw when validating a valid saga definition.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_ValidDefinition_DoesNotThrow()
    {
        var validator = new SagaDefinitionValidator();
        var definition = CreateValidDefinition();

        var act = () => validator.ValidateAsync(definition);

        await act.Should().NotThrowAsync();
    }
}

/// <summary>
/// Contains unit tests for validating <see cref="SagaRequestValidator"/> class.
/// Tests cover validation of saga creation requests.
/// </summary>
public class SagaRequestValidatorTests
{
    /// <summary>
    /// Tests that <see cref="SagaRequestValidator.ValidateCreateSagaAsync(CreateSagaRequest)"/> does not throw when validating a valid create saga request.
    /// </summary>
    [Fact]
    public async Task ValidateCreateSagaAsync_WithValidRequest_DoesNotThrow()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "def_123", Data = "{}" };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that <see cref="SagaRequestValidator.ValidateCreateSagaAsync(CreateSagaRequest)"/> throws <see cref="ArgumentException"/> when DefinitionId is missing.
    /// </summary>
    [Fact]
    public async Task ValidateCreateSagaAsync_MissingDefinitionId_Throws()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "", Data = "{}" };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*DefinitionId*");
    }

    /// <summary>
    /// Tests that <see cref="SagaRequestValidator.ValidateCreateSagaAsync(CreateSagaRequest)"/> throws <see cref="ArgumentException"/> when DefinitionId exceeds 255 characters.
    /// </summary>
    [Fact]
    public async Task ValidateCreateSagaAsync_DefinitionIdTooLong_Throws()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = new string('x', 256), Data = "{}" };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*exceed 255*");
    }

    /// <summary>
    /// Tests that <see cref="SagaRequestValidator.ValidateCreateSagaAsync(CreateSagaRequest)"/> throws <see cref="ArgumentException"/> when Data exceeds 10000 characters.
    /// </summary>
    [Fact]
    public async Task ValidateCreateSagaAsync_DataTooLarge_Throws()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "def_123", Data = new string('x', 10001) };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*cannot exceed 10000*");
    }

    /// <summary>
    /// Tests that <see cref="SagaRequestValidator.ValidateCreateSagaAsync(CreateSagaRequest)"/> succeeds when Data is exactly 10000 characters (the maximum allowed).
    /// </summary>
    [Fact]
    public async Task ValidateCreateSagaAsync_ValidLargeData_Succeeds()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "def_123", Data = new string('x', 10000) };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().NotThrowAsync();
    }
}

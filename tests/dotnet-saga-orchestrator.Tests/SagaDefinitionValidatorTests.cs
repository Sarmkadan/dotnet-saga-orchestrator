#nullable enable

using SagaOrchestrator.Application.Validators;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

public class SagaDefinitionValidatorTests
{
    private static SagaStepDefinition CreateValidStep(string name = "PaymentStep") =>
        new SagaStepDefinition(name, "payment-svc", "http://payment-svc/charge", "http://payment-svc/refund")
        {
            TimeoutSeconds = 30,
            MaxRetries = 3
        };

    private static SagaDefinition CreateValidDefinition(string name = "OrderSaga") =>
        new SagaDefinition(name, "Order processing saga")
        {
            Steps = new List<SagaStepDefinition> { CreateValidStep() }
        };

    [Fact]
    public async Task ValidateAsync_WithValidDefinition_DoesNotThrow()
    {
        var validator = new SagaDefinitionValidator();
        var definition = CreateValidDefinition();

        var act = () => validator.ValidateAsync(definition);

        await act.Should().NotThrowAsync();
    }

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
        exception.Which.Message.Should().ContainMatch("*is invalid*");
    }
}

public class SagaRequestValidatorTests
{
    [Fact]
    public async Task ValidateCreateSagaAsync_WithValidRequest_DoesNotThrow()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "def_123", Data = "{}" };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateCreateSagaAsync_MissingDefinitionId_Throws()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "", Data = "{}" };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*DefinitionId*");
    }

    [Fact]
    public async Task ValidateCreateSagaAsync_DefinitionIdTooLong_Throws()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = new string('x', 256), Data = "{}" };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*exceed 255*");
    }

    [Fact]
    public async Task ValidateCreateSagaAsync_DataTooLarge_Throws()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "def_123", Data = new string('x', 10001) };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*cannot exceed 10000*");
    }

    [Fact]
    public async Task ValidateCreateSagaAsync_ValidLargeData_Succeeds()
    {
        var validator = new SagaRequestValidator();
        var request = new CreateSagaRequest { DefinitionId = "def_123", Data = new string('x', 10000) };

        var act = () => validator.ValidateCreateSagaAsync(request);

        await act.Should().NotThrowAsync();
    }
}

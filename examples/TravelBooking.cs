#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;

/// Travel booking saga example (hotel, flight, car)
/// Demonstrates: multiple independent service calls and parallel compensation
public class TravelBookingExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSagaOrchestrator();

        var serviceProvider = services.BuildServiceProvider();
        var definitionService = serviceProvider.GetRequiredService<SagaDefinitionService>();
        var orchestrationService = serviceProvider.GetRequiredService<SagaOrchestrationService>();
        var logger = serviceProvider.GetRequiredService<ILogger<TravelBookingExample>>();

        try
        {
            logger.LogInformation("=== Travel Booking Saga ===\n");

            var definition = await definitionService.CreateDefinitionAsync(
                "Travel Booking",
                "Book hotel, flight, and car rental for travel");

            // Step 1: Book hotel
            var hotelStep = new SagaStepDefinition(
                "Book Hotel",
                "hotel-service",
                "http://localhost:6001/api/hotels/book",
                "http://localhost:6001/api/hotels/cancel");
            hotelStep.SetTimeout(45);
            hotelStep.SetRetryPolicy(3, 1000);
            await definitionService.AddStepAsync(definition.Id, hotelStep);

            // Step 2: Book flight
            var flightStep = new SagaStepDefinition(
                "Book Flight",
                "flight-service",
                "http://localhost:6002/api/flights/book",
                "http://localhost:6002/api/flights/cancel");
            flightStep.SetTimeout(60);
            flightStep.SetRetryPolicy(2, 2000);
            await definitionService.AddStepAsync(definition.Id, flightStep);

            // Step 3: Book car rental
            var carStep = new SagaStepDefinition(
                "Book Car Rental",
                "car-service",
                "http://localhost:6003/api/cars/book",
                "http://localhost:6003/api/cars/cancel");
            carStep.SetTimeout(45);
            carStep.SetRetryPolicy(3, 1000);
            await definitionService.AddStepAsync(definition.Id, carStep);

            logger.LogInformation("✓ Created travel booking definition\n");

            var validation = definitionService.ValidateDefinition(definition);
            if (!validation.IsValid)
            {
                logger.LogError("✗ Validation failed");
                return;
            }

            var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

            var saga = await orchestrationService.CreateSagaAsync(
                retrievedDef,
                maxRetries: 3,
                timeoutSeconds: 600);

            logger.LogInformation($"✓ Travel saga created: {saga.Id}");
            logger.LogInformation("Booking details:");
            logger.LogInformation("  Destination: Paris, France");
            logger.LogInformation("  Dates: 2026-06-15 to 2026-06-22");
            logger.LogInformation("  Hotel: 4-star");
            logger.LogInformation("  Flight: Round-trip economy");
            logger.LogInformation("  Car: Mid-size sedan\n");

            await orchestrationService.StartSagaAsync(saga.Id);
            logger.LogInformation("✓ Processing bookings...\n");

            // Execute all bookings
            for (int i = 0; i < 3; i++)
            {
                var step = await orchestrationService.ExecuteNextStepAsync(saga.Id);
                if (step != null)
                {
                    logger.LogInformation($"✓ {step.Name}: {step.Status}");
                }
            }

            var finalSaga = await orchestrationService.GetSagaAsync(saga.Id);

            if (finalSaga.Status == SagaStatus.Completed)
            {
                logger.LogInformation("\n✓ All bookings confirmed!");
                logger.LogInformation("  Confirmation emails sent");
                logger.LogInformation("  Vouchers generated");
                logger.LogInformation("  Payment processed");
            }
            else if (finalSaga.Status == SagaStatus.Failed)
            {
                logger.LogInformation($"\n✗ Booking failed");
                logger.LogInformation("Initiating cancellation...\n");

                // Use parallel compensation for independent services
                await orchestrationService.CompensateSagaAsync(
                    saga.Id,
                    CompensationStrategy.Parallel);

                logger.LogInformation("✓ All bookings cancelled");
                logger.LogInformation("  Refunds initiated");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Booking error");
        }
    }
}

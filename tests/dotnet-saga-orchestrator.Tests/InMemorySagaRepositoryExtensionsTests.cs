using System;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace dotnet_saga_orchestrator.Tests
{
    public class InMemorySagaRepositoryExtensionsTests
    {
        private static Saga CreateSaga(string name = "order-saga", SagaStatus status = SagaStatus.Pending, string? definitionId = null)
        {
            var saga = new Saga
            {
                Status = status
            };
            saga.Definition.Name = name;
            saga.Definition.Id = definitionId ?? saga.Definition.Id ?? Guid.NewGuid().ToString();
            return saga;
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_ReturnsSaga_WhenExists()
        {
            var repository = new InMemorySagaRepository();
            var saga = CreateSaga();
            await repository.CreateAsync(saga);

            var result = await repository.GetByCorrelationIdAsync(saga.CorrelationId);

            Assert.NotNull(result);
            Assert.Equal(saga.Id, result!.Id);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_ReturnsNull_WhenNotFound()
        {
            var repository = new InMemorySagaRepository();

            var result = await repository.GetByCorrelationIdAsync(Guid.NewGuid().ToString());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_ThrowsArgumentException_WhenCorrelationIdIsNullOrEmpty()
        {
            var repository = new InMemorySagaRepository();

            // Call the extension method explicitly: the repository also exposes an instance
            // method with the same signature, which would otherwise shadow the extension.
            await Assert.ThrowsAsync<ArgumentException>(
                () => InMemorySagaRepositoryExtensions.GetByCorrelationIdAsync(repository, string.Empty));
        }

        [Fact]
        public async Task GetByStatusAsync_ReturnsOnlyMatchingSagas()
        {
            var repository = new InMemorySagaRepository();
            var running = CreateSaga(status: SagaStatus.Running);
            var failed = CreateSaga(status: SagaStatus.Failed);
            await repository.CreateAsync(running);
            await repository.CreateAsync(failed);

            var result = await InMemorySagaRepositoryExtensions.GetByStatusAsync(repository, SagaStatus.Running);

            Assert.Single(result);
            Assert.Equal(running.Id, result[0].Id);
        }

        [Fact]
        public async Task GetByStatusAsync_ReturnsEmptyList_WhenNoneMatch()
        {
            var repository = new InMemorySagaRepository();
            await repository.CreateAsync(CreateSaga(status: SagaStatus.Pending));

            var result = await InMemorySagaRepositoryExtensions.GetByStatusAsync(repository, SagaStatus.Completed);

            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchByDefinitionIdAsync_ReturnsMatchingSagas()
        {
            var repository = new InMemorySagaRepository();
            var saga = CreateSaga(definitionId: "def-1");
            await repository.CreateAsync(saga);
            await repository.CreateAsync(CreateSaga(definitionId: "def-2"));

            var result = await repository.SearchByDefinitionIdAsync("def-1");

            Assert.Single(result);
            Assert.Equal(saga.Id, result[0].Id);
        }

        [Fact]
        public async Task SearchByDefinitionIdAsync_ThrowsArgumentException_WhenDefinitionIdIsEmpty()
        {
            var repository = new InMemorySagaRepository();

            await Assert.ThrowsAsync<ArgumentException>(() => repository.SearchByDefinitionIdAsync(string.Empty));
        }

        [Fact]
        public async Task SearchByNameAsync_IsCaseInsensitive()
        {
            var repository = new InMemorySagaRepository();
            var saga = CreateSaga(name: "Order-Saga");
            await repository.CreateAsync(saga);

            var result = await repository.SearchByNameAsync("order-saga");

            Assert.Single(result);
            Assert.Equal(saga.Id, result[0].Id);
        }

        [Fact]
        public async Task SearchByNameAsync_ReturnsEmptyList_WhenNoMatch()
        {
            var repository = new InMemorySagaRepository();
            await repository.CreateAsync(CreateSaga(name: "checkout-saga"));

            var result = await repository.SearchByNameAsync("shipping-saga");

            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchByNameAsync_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            InMemorySagaRepository? repository = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository!.SearchByNameAsync("anything"));
        }

        [Fact]
        public async Task GetTimedOutSagasAsync_ReturnsSagasPastTimeout()
        {
            var repository = new InMemorySagaRepository();
            var timedOut = CreateSaga(status: SagaStatus.Running);
            timedOut.TimeoutSeconds = 1;
            timedOut.StartedAt = DateTime.UtcNow.AddMinutes(-5);
            var stillRunning = CreateSaga(status: SagaStatus.Running);
            stillRunning.TimeoutSeconds = 300;
            stillRunning.StartedAt = DateTime.UtcNow;
            await repository.CreateAsync(timedOut);
            await repository.CreateAsync(stillRunning);

            var result = await repository.GetTimedOutSagasAsync();

            Assert.Single(result);
            Assert.Equal(timedOut.Id, result[0].Id);
        }

        [Fact]
        public async Task GetRetryableSagasAsync_ReturnsOnlyFailedSagasBelowMaxRetries()
        {
            var repository = new InMemorySagaRepository();
            var retryable = CreateSaga(status: SagaStatus.Failed);
            retryable.RetryCount = 1;
            retryable.MaxRetries = 3;
            var exhausted = CreateSaga(status: SagaStatus.Failed);
            exhausted.RetryCount = 3;
            exhausted.MaxRetries = 3;
            await repository.CreateAsync(retryable);
            await repository.CreateAsync(exhausted);

            var result = await repository.GetRetryableSagasAsync();

            Assert.Single(result);
            Assert.Equal(retryable.Id, result[0].Id);
        }

        [Fact]
        public async Task GetFailedSagasAfterAsync_ThrowsArgumentOutOfRangeException_WhenDateIsInFuture()
        {
            var repository = new InMemorySagaRepository();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => repository.GetFailedSagasAfterAsync(DateTime.UtcNow.AddDays(1)));
        }

        [Fact]
        public async Task GetFailedSagasAfterAsync_ReturnsFailedSagas()
        {
            var repository = new InMemorySagaRepository();
            var failed = CreateSaga(status: SagaStatus.Failed);
            await repository.CreateAsync(failed);
            await repository.CreateAsync(CreateSaga(status: SagaStatus.Completed));

            var result = await repository.GetFailedSagasAfterAsync(DateTime.UtcNow.AddDays(-1));

            Assert.Single(result);
            Assert.Equal(failed.Id, result[0].Id);
        }

        [Fact]
        public async Task CountByStatusAsync_ReturnsCorrectCount()
        {
            var repository = new InMemorySagaRepository();
            await repository.CreateAsync(CreateSaga(status: SagaStatus.Pending));
            await repository.CreateAsync(CreateSaga(status: SagaStatus.Pending));
            await repository.CreateAsync(CreateSaga(status: SagaStatus.Completed));

            var count = await repository.CountByStatusAsync(SagaStatus.Pending);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task CountAllAsync_ReturnsZero_WhenRepositoryIsEmpty()
        {
            var repository = new InMemorySagaRepository();

            var count = await repository.CountAllAsync();

            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountAllAsync_ReturnsTotalNumberOfSagas()
        {
            var repository = new InMemorySagaRepository();
            await repository.CreateAsync(CreateSaga());
            await repository.CreateAsync(CreateSaga());

            var count = await repository.CountAllAsync();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task ExistsByCorrelationIdAsync_ReturnsTrue_WhenSagaExists()
        {
            var repository = new InMemorySagaRepository();
            var saga = CreateSaga();
            await repository.CreateAsync(saga);

            var exists = await repository.ExistsByCorrelationIdAsync(saga.CorrelationId);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByCorrelationIdAsync_ReturnsFalse_WhenSagaDoesNotExist()
        {
            var repository = new InMemorySagaRepository();

            var exists = await repository.ExistsByCorrelationIdAsync(Guid.NewGuid().ToString());

            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCorrelationIdAsync_ThrowsArgumentNullException_WhenCorrelationIdIsNull()
        {
            var repository = new InMemorySagaRepository();

            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.ExistsByCorrelationIdAsync(null!));
        }
    }
}

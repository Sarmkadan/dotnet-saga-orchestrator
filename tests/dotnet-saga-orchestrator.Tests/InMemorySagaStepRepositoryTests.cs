using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests
{
    public class InMemorySagaStepRepositoryTests
    {
        private static SagaStep CreateSampleStep(
            string id = "step-1",
            string sagaId = "saga-1",
            int order = 1,
            SagaStepStatus status = SagaStepStatus.Pending)
        {
            return new SagaStep
            {
                Id = id,
                SagaId = sagaId,
                Order = order,
                Status = status
            };
        }

        [Fact]
        public async Task CreateAsync_ShouldAddAndReturnStep()
        {
            // Arrange
            var repo = new InMemorySagaStepRepository();
            var step = CreateSampleStep();

            // Act
            var created = await repo.CreateAsync(step);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(step.Id, created!.Id);
            var fetched = await repo.GetByIdAsync(step.Id);
            Assert.Same(step, fetched);
        }

        [Fact]
        public async Task CreateAsync_NullStep_ThrowsArgumentNullException()
        {
            // Arrange
            var repo = new InMemorySagaStepRepository();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.CreateAsync(null!));
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            var repo = new InMemorySagaStepRepository();
            var result = await repo.GetByIdAsync("non-existent");
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyExistingStep()
        {
            var repo = new InMemorySagaStepRepository();
            var step = CreateSampleStep(status: SagaStepStatus.Pending);
            await repo.CreateAsync(step);

            // Change status
            step.Status = SagaStepStatus.Completed;
            var updated = await repo.UpdateAsync(step);

            Assert.NotNull(updated);
            Assert.Equal(SagaStepStatus.Completed, updated!.Status);
            var fetched = await repo.GetByIdAsync(step.Id);
            Assert.Equal(SagaStepStatus.Completed, fetched!.Status);
        }

        [Fact]
        public async Task UpdateAsync_NullStep_ThrowsArgumentNullException()
        {
            var repo = new InMemorySagaStepRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_ValidId_RemovesStepAndReturnsTrue()
        {
            var repo = new InMemorySagaStepRepository();
            var step = CreateSampleStep();
            await repo.CreateAsync(step);

            var deleted = await repo.DeleteAsync(step.Id);
            Assert.True(deleted);

            var afterDelete = await repo.GetByIdAsync(step.Id);
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task DeleteAsync_NullOrEmptyId_ReturnsFalse()
        {
            var repo = new InMemorySagaStepRepository();

            var resultNull = await repo.DeleteAsync(null!);
            var resultEmpty = await repo.DeleteAsync(string.Empty);

            Assert.False(resultNull);
            Assert.False(resultEmpty);
        }

        [Fact]
        public async Task GetBySagaIdAsync_ReturnsStepsOrderedByOrder()
        {
            var repo = new InMemorySagaStepRepository();

            var step1 = CreateSampleStep(id: "s1", sagaId: "sagaA", order: 2);
            var step2 = CreateSampleStep(id: "s2", sagaId: "sagaA", order: 1);
            var step3 = CreateSampleStep(id: "s3", sagaId: "sagaB", order: 1);

            await repo.CreateAsync(step1);
            await repo.CreateAsync(step2);
            await repo.CreateAsync(step3);

            var sagaASteps = await repo.GetBySagaIdAsync("sagaA");

            Assert.Equal(2, sagaASteps.Count);
            Assert.Equal("s2", sagaASteps[0].Id); // order 1
            Assert.Equal("s1", sagaASteps[1].Id); // order 2
        }

        [Fact]
        public async Task GetByStatusAsync_ReturnsMatchingSteps()
        {
            var repo = new InMemorySagaStepRepository();

            var pending = CreateSampleStep(id: "p1", status: SagaStepStatus.Pending);
            var completed = CreateSampleStep(id: "c1", status: SagaStepStatus.Completed);
            await repo.CreateAsync(pending);
            await repo.CreateAsync(completed);

            var pendingSteps = await repo.GetByStatusAsync(SagaStepStatus.Pending);
            Assert.Single(pendingSteps);
            Assert.Equal("p1", pendingSteps[0].Id);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllStoredSteps()
        {
            var repo = new InMemorySagaStepRepository();

            var stepA = CreateSampleStep(id: "a");
            var stepB = CreateSampleStep(id: "b");
            await repo.CreateAsync(stepA);
            await repo.CreateAsync(stepB);

            var all = await repo.GetAllAsync();
            Assert.Equal(2, all.Count);
            var ids = new HashSet<string>(all.ConvertAll(s => s.Id));
            Assert.Contains("a", ids);
            Assert.Contains("b", ids);
        }

        [Fact]
        public async Task GetByOrderAsync_ReturnsCorrectStepOrNull()
        {
            var repo = new InMemorySagaStepRepository();

            var step = CreateSampleStep(id: "order1", sagaId: "sagaX", order: 5);
            await repo.CreateAsync(step);

            var found = await repo.GetByOrderAsync("sagaX", 5);
            Assert.NotNull(found);
            Assert.Equal(step.Id, found!.Id);

            var notFound = await repo.GetByOrderAsync("sagaX", 99);
            Assert.Null(notFound);
        }
    }
}

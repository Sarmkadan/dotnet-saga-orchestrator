using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Tests
{
    public class InMemoryCompensationTransactionRepositoryTests
    {
        private readonly InMemoryCompensationTransactionRepository _repository;

        public InMemoryCompensationTransactionRepositoryTests()
        {
            _repository = new InMemoryCompensationTransactionRepository();
        }

        private CompensationTransaction CreateTestTransaction(string id = "comp-1", string sagaId = "saga-1")
        {
            var transaction = new CompensationTransaction { Id = id };
            transaction.Initialize(sagaId, "step-1", "step-name", 1, "http://url");
            return transaction;
        }

        [Fact]
        public async Task CreateAsync_ShouldAddTransaction()
        {
            var transaction = CreateTestTransaction();
            var result = await _repository.CreateAsync(transaction);
            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result!.Id);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowExceptionIfIdExists()
        {
            var transaction = CreateTestTransaction();
            await _repository.CreateAsync(transaction);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.CreateAsync(transaction));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTransaction()
        {
            var transaction = CreateTestTransaction();
            await _repository.CreateAsync(transaction);
            var result = await _repository.GetByIdAsync(transaction.Id);
            Assert.NotNull(result);
            Assert.Equal(transaction.Id, result!.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTransaction()
        {
            var transaction = CreateTestTransaction();
            await _repository.CreateAsync(transaction);
            transaction.Status = CompensationStatus.Completed;
            var result = await _repository.UpdateAsync(transaction);
            Assert.NotNull(result);
            Assert.Equal(CompensationStatus.Completed, result!.Status);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTransaction()
        {
            var transaction = CreateTestTransaction();
            await _repository.CreateAsync(transaction);
            var deleted = await _repository.DeleteAsync(transaction.Id);
            Assert.True(deleted);
            var result = await _repository.GetByIdAsync(transaction.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBySagaIdAsync_ShouldReturnTransactions()
        {
            var sagaId = "saga-1";
            await _repository.CreateAsync(CreateTestTransaction("c1", sagaId));
            await _repository.CreateAsync(CreateTestTransaction("c2", sagaId));
            var result = await _repository.GetBySagaIdAsync(sagaId);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTransactions()
        {
            await _repository.CreateAsync(CreateTestTransaction("c1"));
            await _repository.CreateAsync(CreateTestTransaction("c2"));
            var result = await _repository.GetAllAsync();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnTransactionsByStatus()
        {
            var t1 = CreateTestTransaction("c1");
            t1.Status = CompensationStatus.Completed;
            await _repository.CreateAsync(t1);
            
            var t2 = CreateTestTransaction("c2");
            t2.Status = CompensationStatus.Pending;
            await _repository.CreateAsync(t2);

            var result = await _repository.GetByStatusAsync(CompensationStatus.Completed);
            Assert.Single(result);
            Assert.Equal("c1", result[0].Id);
        }
    }
}

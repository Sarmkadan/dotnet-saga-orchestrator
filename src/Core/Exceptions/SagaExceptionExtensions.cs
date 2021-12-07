using System;

namespace SagaOrchestrator.Core.Exceptions
{
    public static class SagaExceptionExtensions
    {
        public static bool IsSagaNotFound(this SagaException ex) =>
            ex.ErrorCode == "SAGA_NOT_FOUND";

        public static bool IsSagaTimeout(this SagaException ex) =>
            ex.ErrorCode == "SAGA_TIMEOUT";

        public static string GetDetailedMessage(this SagaException ex) =>
            $"Saga Id: {ex.SagaId}, Error Code: {ex.ErrorCode}, Message: {ex.Message}";
    }
}

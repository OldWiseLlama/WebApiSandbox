using System;
using System.Threading;

namespace WebApi
{
    public class AppContext
    {
        private static readonly AsyncLocal<Guid> CorrelationIdStore = new();

        public static void SetCorrelationId(Guid correlationId)
        {
            CorrelationIdStore.Value = correlationId;
        }

        public static Guid GetCorrelationId()
        {
            return CorrelationIdStore.Value;
        }
    }
}
using System;
using System.Runtime.Serialization;

namespace Dna.Ecommerce.LiveIntegration.Exceptions
{
  /// <summary>
  /// Custom exception class for communication errors with the ERP.
  /// </summary>
  public class LiveIntegrationException : Exception
  {
    public LiveIntegrationException()
    {
    }

    public LiveIntegrationException(string message) : base(message)
    {
    }

    public LiveIntegrationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected LiveIntegrationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
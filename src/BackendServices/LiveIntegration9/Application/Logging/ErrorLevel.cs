namespace Dna.Ecommerce.LiveIntegration.Logging
{
  /// <summary>
  /// Specifies the error level for logging.
  /// </summary>
  public enum ErrorLevel
  {
    /// <summary>
    ///Response and request content.
    /// </summary>
    DebugInfo,

    /// <summary>
    /// Web service connection errors.
    /// </summary>
    ConnectionError,

    /// <summary>
    /// Response is empty or response format is broken.
    /// </summary>
    ResponseError,

    /// <summary>
    /// General error.
    /// </summary>
    Error,

    /// <summary>
    /// Email send is used for getting errors appeared after last email sending.
    /// </summary>
    EmailSend
  }
}
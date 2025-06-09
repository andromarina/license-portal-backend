namespace WinLicenseBackend
{
    /// <summary>
    /// An exception whose Message is safe to show to end-users,
    /// with Details available for diagnostics.
    /// </summary>
    public class FriendlyException : Exception
    {
        /// <summary>What the user should see (localized).</summary>
        public string UserMessage { get; }

        /// <summary>Optional “roll-up” of inner exception details for logging or an “Advanced” view.</summary>
        public string Details => InnerException?.ToString();

        public FriendlyException(string userMessage)
            : base(userMessage)
        {
            UserMessage = userMessage;
        }

        public FriendlyException(string userMessage, Exception inner)
            : base(userMessage, inner)
        {
            UserMessage = userMessage;
        }
    }

}

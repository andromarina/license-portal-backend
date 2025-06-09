namespace WinLicenseBackend
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;

    public class GlobalExceptionFilter : IExceptionFilter
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            _logger.Error(ex, "Unhandled exception");

            // If it's already a FriendlyException, reuse the message; otherwise use a generic one
            var userMessage = ex is FriendlyException fe
                ? fe.UserMessage
                : ex.Message;

            if(String.IsNullOrEmpty(userMessage))
            {
                userMessage = "Sorry, something went wrong. Please try again.";
            }

            // Optionally include details for admins (via config flag)
            var details = ex is FriendlyException fex ? fex.Details : null;

            context.Result = new JsonResult(new { Message = userMessage, Details = details })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }

}

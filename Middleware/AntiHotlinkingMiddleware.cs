namespace Content_Delivery.Middleware
{

    public class AntiHotlinkingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<AntiHotlinkingMiddleware> _logger;

        public AntiHotlinkingMiddleware(RequestDelegate next, IConfiguration config, ILogger<AntiHotlinkingMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var referer = context.Request.Headers["Referer"].ToString();
            var allowedOrigins = _config.GetSection("AllowedOrigins").Get<string[]>();

            //if (context.Request.Method.ToUpper() == "GET" &&
            //    (string.IsNullOrEmpty(referer) ||
            //    !allowedOrigins.Any(origin => referer.StartsWith(origin))))
            //{
            if (!allowedOrigins.Any(origin => origin.StartsWith(context.Request.Headers.Host)))
            {
                _logger.LogWarning($"Blocked hotlinked request from {context.Connection.RemoteIpAddress}");
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Hotlinking not allowed");
            }
            else
            {
                await _next(context);
            }
        }
    }
}

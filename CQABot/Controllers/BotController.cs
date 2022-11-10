using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CQABot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly Dictionary<string, IBotFrameworkHttpAdapter> _adapters = new Dictionary<string, IBotFrameworkHttpAdapter>();
        private readonly IBot _bot;
        private readonly ILogger<BotController> _logger;

        public BotController(
            IConfiguration configuration,
            IEnumerable<IBotFrameworkHttpAdapter> adapters,
            IBot bot,
            ILogger<BotController> logger)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _logger = logger;

            var adapterSettings = configuration.GetSection(AdapterSettings.AdapterSettingsKey).Get<List<AdapterSettings>>() ?? new List<AdapterSettings>();
            adapterSettings.Add(AdapterSettings.CoreBotAdapterSettings);

            foreach (var adapter in adapters ?? throw new ArgumentNullException(nameof(adapters)))
            {
                var settings = adapterSettings.FirstOrDefault(s => s.Enabled && s.Type == adapter.GetType().FullName);

                if (settings != null)
                {
                    _adapters.Add(settings.Route, adapter);
                }
            }
        }


        [HttpPost]
        [HttpGet]
        [Route("api/{route}")]
        public async Task PostAsync(string route)
        {
            if (string.IsNullOrEmpty(route))
            {
                _logger.LogError($"PostAsync: No route provided.");
                throw new ArgumentNullException(nameof(route));
            }

            if (_adapters.TryGetValue(route, out IBotFrameworkHttpAdapter adapter))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogInformation($"PostAsync: routed '{route}' to {adapter.GetType().Name}");
                }

                // Delegate the processing of the HTTP POST to the appropriate adapter.
                // The adapter will invoke the bot.
                _logger.LogInformation($"-----PostAsync: PostAsync to {Request.Scheme} {Request.Host} {Request.Path} {Response.StatusCode}. {Request.QueryString}");
               /* foreach (var header in Request.Headers) 
                { 
                   
                   _logger.LogInformation($"-----PostAsync: PostAsync to Header: {header.Key} Value {header.Value}.");

                }

                foreach (var cookie in Request.Cookies)
                {

                    _logger.LogInformation($"-----PostAsync: PostAsync to Header: {cookie.Key} Value {cookie.Value}.");

                }


                //Get request stream and reset the position of this stream
                // Leave stream open so next middleware can read it

                Request.EnableBuffering();

                // Leave the body open so the next middleware can read it.
                using (var reader = new StreamReader(
                    Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024*5,
                    leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    // Do some processing with body�
                    _logger.LogInformation($"-----PostAsync: PostAsync to Body: {body}.");

                    // Reset the request body stream position so the next middleware can read it
                    Request.Body.Position = 0;
                }*/

                await adapter.ProcessAsync(Request, Response, _bot).ConfigureAwait(false);
              
               
            }
            else
            {
                _logger.LogError($"PostAsync: No adapter registered and enabled for route {route}.");
                throw new KeyNotFoundException($"No adapter registered and enabled for route {route}.");
            }
        }
    }
}

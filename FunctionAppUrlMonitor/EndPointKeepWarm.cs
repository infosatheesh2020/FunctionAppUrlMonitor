using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System.Threading;

namespace FunctionAppUrlMonitor
{
    public static class EndPointKeepWarm
    {
        private static HttpClient _httpClient = new HttpClient();
        private static string _endPointsToHit = Environment.GetEnvironmentVariable("EndPointUrls");
        private static string AppInsights_Instrumentation_key = Environment.GetEnvironmentVariable("AppInsights_Instrumentation_key");

        private static TelemetryConfiguration configuration = new TelemetryConfiguration { InstrumentationKey = AppInsights_Instrumentation_key };
        private static TelemetryClient client = new TelemetryClient(configuration);

        [FunctionName("EndPointKeepWarm")]
        // run every 1 minute..
        public static async Task Run([TimerTrigger("%MyTimerExpression%")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"Run(): EndPointKeepWarm function executed at: {DateTime.Now}. Past due? {myTimer.IsPastDue}");

            if (!string.IsNullOrEmpty(_endPointsToHit) | !string.IsNullOrEmpty(AppInsights_Instrumentation_key))
            {
                string[] endPoints = _endPointsToHit.Split(';');
                foreach (string endPoint in endPoints)
                {
                    string tidiedUrl = endPoint.Trim();
                    if (!tidiedUrl.EndsWith("/"))
                    {
                        tidiedUrl += "/";
                    }
                    log.Info($"Run(): About to hit URL: '{tidiedUrl}'");

                    HttpResponseMessage response = await hitUrl(tidiedUrl, log);
                }
            }
            else
            {
                log.Error($"Run(): No URLs specified in environment variable 'EndPointUrls' or 'AppInsights_Instrumentation_key' not specified. Expected a single URL or multiple URLs " +
                    "separated with a semi-colon (;). Please add this config to use the tool.");
            }

            log.Info($"Run(): Completed..");
        }

        private static async Task<HttpResponseMessage> hitUrl(string url, TraceWriter log)
        {
            HttpResponseMessage response = null;
            AvailabilityTelemetry telemetry = new AvailabilityTelemetry();
            telemetry.Name = url;
            telemetry.Timestamp = DateTime.Now;
            try
            {
                response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    log.Info($"hitUrl(): Successfully hit URL: '{url}'");
                }
                else
                {
                    log.Error($"hitUrl(): Failed to hit URL: '{url}'. Response: {(int)response.StatusCode + " : " + response.ReasonPhrase}");
                }

                telemetry.Success = response.IsSuccessStatusCode;
                telemetry.Message = response.ReasonPhrase;
            }
            catch (Exception ex)
            {
                log.Error($"hitUrl(): Failed to hit URL (Exception) : '{url}'");
                telemetry.Success = false;
                telemetry.Message = "URL not reachable";
            }
            finally
            {
                client.TrackAvailability(telemetry);
            }
            return response;
        }


    }
}

# FunctionAppUrlMonitor

This dotnet function app hits the specified URLs for availability and send those information as Availability metrics of Azure Application Insights.

This application can be used to monitor internal URLs within Virtual network and Application Insights can't reach the endpoint for availability tests.

## Instructions

1. Deploy this function to an Azure Function App

2. Add below Application properties to inject environment variables necessary for functioning of application

  - <b>EndPointUrls</b> separated by semi-colon(Eg: "https://www.bing.com;https://www.microsoft.com")
  - <b>AppInsights_Instrumentation_key</b>
  - <b>MyTimerExpression</b> which specifies the cron frequency to collect telemetry (Eg: "0 */10 * * * *")

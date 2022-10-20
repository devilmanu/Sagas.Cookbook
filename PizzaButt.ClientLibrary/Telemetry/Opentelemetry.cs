﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Hosting;
using PizzaButt.ClientLibrary.Constants;

namespace PizzaButt.ClientLibrary.Telemetry
{
 

    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddCustomTracing(
            this TracerProviderBuilder builder,
            IWebHostEnvironment webHostEnvironment) =>
            builder
                .SetResourceBuilder(GetResourceBuilder(webHostEnvironment))
                .AddAspNetCoreInstrumentation(
                    options =>
                    {
                        options.Enrich = Enrich;
                        options.RecordException = true;
                    })
                .AddConsoleExporter(
                    options => options.Targets = ConsoleExporterOutputTargets.Console | ConsoleExporterOutputTargets.Debug);

        public static TracerProviderBuilder AddIf(
            this TracerProviderBuilder builder,
            bool condition,
            Func<TracerProviderBuilder, TracerProviderBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(action);

            if (condition)
            {
                builder = action(builder);
            }

            return builder;
        }

        private static ResourceBuilder GetResourceBuilder(IWebHostEnvironment webHostEnvironment) =>
            ResourceBuilder
                .CreateEmpty()
                .AddService(
                    webHostEnvironment.ApplicationName,
                    serviceVersion: null)
                .AddAttributes(
                    new KeyValuePair<string, object>[]
                    {
                        new(OpenTelemetryAttributeName.Deployment.Environment, webHostEnvironment.EnvironmentName),
                        new(OpenTelemetryAttributeName.Host.Name, Environment.MachineName),
                    })
                .AddEnvironmentVariableDetector();

        /// <summary>
        /// Enrich spans with additional request and response meta data.
        /// See https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/semantic_conventions/http.md.
        /// </summary>
        private static void Enrich(Activity activity, string eventName, object obj)
        {
            if (obj is HttpRequest request)
            {
                var context = request.HttpContext;
                activity.AddTag(OpenTelemetryAttributeName.Http.Flavor, OpenTelemetryHttpFlavour.GetHttpFlavour(request.Protocol));
                activity.AddTag(OpenTelemetryAttributeName.Http.Scheme, request.Scheme);
                activity.AddTag(OpenTelemetryAttributeName.Http.ClientIP, context.Connection.RemoteIpAddress);
                activity.AddTag(OpenTelemetryAttributeName.Http.RequestContentLength, request.ContentLength);
                activity.AddTag(OpenTelemetryAttributeName.Http.RequestContentType, request.ContentType);

                var user = context.User;
                if (user.Identity?.Name is not null)
                {
                    activity.AddTag(OpenTelemetryAttributeName.EndUser.Id, user.Identity.Name);
                    activity.AddTag(OpenTelemetryAttributeName.EndUser.Scope, string.Join(',', user.Claims.Select(x => x.Value)));
                }
            }
            else if (obj is HttpResponse response)
            {
                activity.AddTag(OpenTelemetryAttributeName.Http.ResponseContentLength, response.ContentLength);
                activity.AddTag(OpenTelemetryAttributeName.Http.ResponseContentType, response.ContentType);
            }
        }

        public static class OpenTelemetryHttpFlavour
        {
            public const string Http10 = "1.0";
            public const string Http11 = "1.1";
            public const string Http20 = "2.0";
            public const string Http30 = "3.0";

            public static string GetHttpFlavour(string protocol)
            {
                if (HttpProtocol.IsHttp10(protocol))
                {
                    return Http10;
                }
                else if (HttpProtocol.IsHttp11(protocol))
                {
                    return Http11;
                }
                else if (HttpProtocol.IsHttp2(protocol))
                {
                    return Http20;
                }
                else if (HttpProtocol.IsHttp3(protocol))
                {
                    return Http30;
                }

                throw new InvalidOperationException($"Protocol {protocol} not recognised.");
            }
        }
    }

}
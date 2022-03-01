'use strict';

import { context, trace } from '@opentelemetry/api';
import { registerInstrumentations } from '@opentelemetry/instrumentation'
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web'
import { ConsoleSpanExporter, SimpleSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { Resource } from '@opentelemetry/resources'
import { SemanticResourceAttributes }  from '@opentelemetry/semantic-conventions'
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
//import { OTLPTraceExporter }  from '@opentelemetry/exporter-trace-otlp-grpc';
import { ZoneContextManager } from '@opentelemetry/context-zone';

const EXPORTER = process.env.EXPORTER || '';

export const tracer = (serviceName) => {
    const provider = new WebTracerProvider({
        resource: new Resource({
            [SemanticResourceAttributes.SERVICE_NAME]: serviceName,
        }),
    });

    provider.addSpanProcessor(new SimpleSpanProcessor(new OTLPTraceExporter()));
    provider.addSpanProcessor(new SimpleSpanProcessor(new ConsoleSpanExporter()));

    // Initialize the OpenTelemetry APIs to use the NodeTracerProvider bindings
    provider.register({
        // Changing default contextManager to use ZoneContextManager - supports asynchronous operations - optional
        contextManager: new ZoneContextManager(),
    });

    registerInstrumentations({
        // // when boostraping with lerna for testing purposes
        instrumentations: [
            new FetchInstrumentation(),
        ],
    });

    return trace.getTracer('http-example');
};
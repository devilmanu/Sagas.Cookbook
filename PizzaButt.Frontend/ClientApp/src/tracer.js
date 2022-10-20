import { ConsoleSpanExporter, SimpleSpanProcessor } from '@opentelemetry/sdk-trace-base';
//import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { B3Propagator } from '@opentelemetry/propagator-b3';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { UserInteractionInstrumentation } from '@opentelemetry/instrumentation-user-interaction';


const provider = new WebTracerProvider();

const collectorOptions = {
    url: 'http://localhost:4318/v1/traces', // url is optional and can be omitted - default is http://localhost:4318/v1/traces
    headers: {}, // an optional object containing custom headers to be sent with each request
    concurrencyLimit: 10, // an optional limit on pending requests
};

// Note: For production consider using the "BatchSpanProcessor" to reduce the number of requests
// to your exporter. Using the SimpleSpanProcessor here as it sends the spans immediately to the
// exporter without delay
provider.addSpanProcessor(new SimpleSpanProcessor(new ConsoleSpanExporter()));
const exporter = new OTLPTraceExporter(collectorOptions);
provider.addSpanProcessor(new BatchSpanProcessor(exporter, {
    // The maximum queue size. After the size is reached spans are dropped.
    maxQueueSize: 100,
    // The maximum batch size of every export. It must be smaller or equal to maxQueueSize.
    maxExportBatchSize: 10,
    // The interval between two consecutive exports
    scheduledDelayMillis: 500,
    // How long the export can run before it is cancelled
    exportTimeoutMillis: 30000,
}));
//provider.register({
//    contextManager: new ZoneContextManager(),
//    propagator: new B3Propagator(),
//});

//registerInstrumentations({
//    instrumentations: [
//        new UserInteractionInstrumentation(),
//        new FetchInstrumentation()
//    ],
//});
provider.register();

export const webTracerWithZone = provider.getTracer('example-tracer-web');
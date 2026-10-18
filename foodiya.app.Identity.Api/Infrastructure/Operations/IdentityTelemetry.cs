using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace foodiya.app.Identity.Infrastructure.Operations;

public sealed class IdentityTelemetry(IMeterFactory meterFactory)
{
    public const string SourceName = "foodiya.app.Identity.Operations";
    public const string MeterName = SourceName;
    public static readonly ActivitySource Activities = new(SourceName);
    private readonly Counter<long> _results = meterFactory.Create(SourceName)
        .CreateCounter<long>("identity.operations", description: "Operations by bounded operation and outcome labels.");

    public void Record(string operation, string outcome, long count = 1) =>
        _results.Add(count, new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("outcome", outcome));
}

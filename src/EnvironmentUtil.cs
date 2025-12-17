using Serilog;
using Soenneker.Extensions.String;
using Soenneker.Utils.AtomicNullableBools;
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Utils.Environment;

/// <summary>
/// A utility library for useful environment related functionality
/// </summary>
public static class EnvironmentUtil
{
    private const string _pipelineEnvironmentVar = "PipelineEnvironment";
    private const string _unknownMachineName = "Unknown";

    private static readonly AtomicNullableBool _isPipeline = new();

    /// <summary>
    /// Set the Environment variable "PipelineEnvironment" to "true" for this to return true.
    /// </summary>
    /// <remarks>Syntactic sugar for cached env lookup</remarks>
    [Pure]
    public static bool IsPipeline
    {
        get
        {
            // Fast path: already computed
            bool? cached = _isPipeline.Value;
            if (cached.HasValue)
                return cached.Value;

            // Compute once
            string? pipelineEnv = System.Environment.GetEnvironmentVariable(_pipelineEnvironmentVar);
            bool value = bool.TryParse(pipelineEnv, out bool parsed) && parsed;

            // Publish only if still unknown (races are fine)
            _isPipeline.TrySet(value);

            return value;
        }
    }

    /// <summary>
    /// If we're in a pipeline environment, Task.Delay (and log)
    /// </summary>
    public static Task PipelineDelay(int millisecondsDelay, CancellationToken cancellationToken = default)
    {
        if (!IsPipeline || millisecondsDelay <= 0)
            return Task.CompletedTask;

        Log.Information("Pipeline delaying for {ms}ms...", millisecondsDelay);

        return Task.Delay(millisecondsDelay, cancellationToken);
    }

    /// <summary>
    /// Exception safe
    /// </summary>
    /// <returns>"Unknown" if exception</returns>
    [Pure]
    public static string GetMachineName()
    {
        try
        {
            return System.Environment.MachineName;
        }
        catch (Exception e)
        {
            Log.Warning(e, "Could not get the machine name from the environment, returning \"Unknown\"");
            return _unknownMachineName;
        }
    }

    /// <summary>
    /// Throws if the environment variable is null or empty
    /// </summary>
    [Pure]
    public static string GetVariableStrict(string variable)
    {
        variable.ThrowIfNullOrEmpty(nameof(variable));

        string? result = System.Environment.GetEnvironmentVariable(variable);
        result.ThrowIfNullOrEmpty(variable);

        return result!;
    }
}

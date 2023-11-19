using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Soenneker.Extensions.String;

namespace Soenneker.Utils.Environment;

/// <summary>
/// A utility library for useful environment related functionality
/// </summary>
public static class EnvironmentUtil
{
    // Init needs to be done outside of ctor because Fact evaluates before the ctor of the test
    private static readonly Lazy<bool> _isPipelineLazy = new(() =>
    {
        string? pipelineEnv = System.Environment.GetEnvironmentVariable("PipelineEnvironment");

        _ = bool.TryParse(pipelineEnv, out bool isPipeline);

        return isPipeline;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Set the Environment variable "PipelineEnvironment" to "true" for this to return true. <para/>
    /// </summary>
    /// <remarks>Syntactic sugar for lazy instance</remarks>
    [Pure]
    public static bool IsPipeline => _isPipelineLazy.Value;

    /// <summary>
    /// If we're in a pipeline environment, Task.Delay (and log)
    /// </summary>
    public static Task PipelineDelay(int millisecondsDelay)
    {
        if (IsPipeline)
        {
            Log.Information("Pipeline delaying for {ms}ms...", millisecondsDelay);
            return Task.Delay(millisecondsDelay);
        }

        return Task.CompletedTask;
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
            return "Unknown";
        }
    }

    /// <summary>
    /// Throws if the environment variable is null or empty, typically used if there is a hard requirement this variable exists
    /// </summary>
    [Pure]
    public static string GetVariableStrict(string variable)
    {
        variable.ThrowIfNullOrEmpty(nameof(variable));

        string? result = System.Environment.GetEnvironmentVariable(variable);

        result.ThrowIfNullOrEmpty(nameof(variable));

        return result!;
    }
}
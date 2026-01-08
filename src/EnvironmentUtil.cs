using Serilog;
using Serilog.Events;
using Soenneker.Atomics.NullableBools;
using Soenneker.Atomics.Strings;
using Soenneker.Extensions.String;
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
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

    private static AtomicNullableBool _isPipeline = new();

    // Cache machine name once; if it throws, cache "Unknown" once.
    private static readonly AtomicString _machineName = new();

    /// <summary>
    /// Set the Environment variable "PipelineEnvironment" to "true" for this to return true.
    /// </summary>
    /// <remarks>Syntactic sugar for cached env lookup</remarks>
    [Pure]
    public static bool IsPipeline
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            bool? cached = _isPipeline.Value;

            if (cached.HasValue)
                return cached.Value;

            return ComputeIsPipeline();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ComputeIsPipeline()
    {
        bool? cached = _isPipeline.Value;
        if (cached.HasValue)
            return cached.Value;

        string? value = System.Environment.GetEnvironmentVariable(_pipelineEnvironmentVar);

        bool isTrue = value is not null && (value.Length == 4
            ? value[0] is 't' or 'T' && value[1] is 'r' or 'R' && value[2] is 'u' or 'U' && value[3] is 'e' or 'E'
            : bool.TryParse(value, out bool parsed) && parsed);

        _isPipeline.TrySet(isTrue);

        return isTrue;
    }

    /// <summary>
    /// If we're in a pipeline environment, Task.Delay (and log)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task PipelineDelay(int millisecondsDelay, CancellationToken cancellationToken = default)
    {
        if (millisecondsDelay <= 0 || !IsPipeline)
            return Task.CompletedTask;

        if (Log.IsEnabled(LogEventLevel.Information))
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
        string? cached = _machineName.Value;
        if (cached is not null)
            return cached;

        return CacheMachineName();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string CacheMachineName()
    {
        string? cached = _machineName.Value;
        if (cached is not null)
            return cached;

        string result;
        try
        {
            result = System.Environment.MachineName;
        }
        catch (Exception e)
        {
            if (Log.IsEnabled(LogEventLevel.Warning))
                Log.Warning(e, "Could not get the machine name from the environment, returning \"Unknown\"");

            result = _unknownMachineName;
        }

        _machineName.TrySet(result);
        return _machineName.Value!;
    }

    /// <summary>
    /// Throws if the environment variable is null or empty
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetVariableStrict(string variable)
    {
        variable.ThrowIfNullOrEmpty(nameof(variable));

        string? result = System.Environment.GetEnvironmentVariable(variable);
        result.ThrowIfNullOrEmpty(variable);

        return result!;
    }
}
[![](https://img.shields.io/nuget/v/Soenneker.Utils.Environment.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Environment/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.environment/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.environment/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Utils.Environment.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Environment/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.environment/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.environment/actions/workflows/codeql.yml)

# Soenneker.Utils.Environment

Static helpers for strict environment-variable lookup, cached pipeline detection, and exception-safe machine-name access.

## Installation

```bash
dotnet add package Soenneker.Utils.Environment
```

## Pipeline detection and delay

Set `PipelineEnvironment=true` before the process first reads `EnvironmentUtil.IsPipeline`:

```csharp
if (EnvironmentUtil.IsPipeline)
{
    await EnvironmentUtil.PipelineDelay(2_000, cancellationToken);
}
```

`IsPipeline` parses `true` case-insensitively and caches the result for the life of the process. Changes to the environment variable after the first read are not observed.

`PipelineDelay()` waits only when pipeline mode is enabled and the requested delay is positive. It writes an information event through Serilog's global `Log` when that level is enabled. Outside pipeline mode it returns immediately.

## Required variables

```csharp
string token = EnvironmentUtil.GetVariableStrict("SERVICE_TOKEN");
```

`GetVariableStrict()` throws when the variable name or resolved value is null or empty. It does not trim values, so a whitespace-only value is returned unchanged. Avoid including secret values in exception messages or logs at the call site.

## Machine name

```csharp
string machineName = EnvironmentUtil.GetMachineName();
```

The machine name is cached on first access. If the platform lookup throws, the method logs a warning through Serilog and caches the literal value `Unknown` instead.

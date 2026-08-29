[![](https://img.shields.io/nuget/v/Soenneker.Utils.Environment.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Environment/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.environment/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.utils.environment/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Utils.Environment.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Utils.Environment/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.utils.environment/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.utils.environment/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Utils.Environment
A utility library for useful environment related functionality.

## Installation

```bash
dotnet add package Soenneker.Utils.Environment
```

## Quick start

```csharp
using Soenneker.Utils.Environment;
```

Call the static `EnvironmentUtil` methods directly; no dependency-injection registration is required.

## Common operations

- `PipelineDelay()` - If we're in a pipeline environment, Task.Delay (and log).
- `GetMachineName()` - Exception safe Returns "Unknown" if exception.
- `GetVariableStrict()` - Throws if the environment variable is null or empty.

# Project coding standards

## Git branch naming
- Branch names must be lowercase to avoid ref collisions on case-insensitive filesystems.
- Use only `a-z`, `0-9`, `.`, `_`, `/`, and `-` characters.
- Prefer slash-delimited prefixes, e.g. `feature/new-round-audio`, `fix/choco-pack-step`.

## .NET guidelines
- Use `slnx` solutions in favor of `sln`. 

## C# guidelines
- All code is targeted at C# 13. The most recent syntax and techniques should always be preferred.
- All `using` statements should be `global` and contained within to `GlobalUsings.cs`. 
    - Global `using` aliases may be used to resolve problematic name collisions.
    - Namespaces should be systems in the the following order:
        1. `System`
        1. `Microsoft`
        1. Other third-parties
        1. `GameShowPro`
        1. Local namespaces
- All code is in a nullable context.
- Nullable value types are used as appropriate.
- Use optional chaining `?.` and nullish coalescing `??` operators.
- Always use file-scoped namespaces.
- Prefer Linq implementations unless
    - There is a clear performance penalty
    - There is an edge case where there is a clearer or more terse way
- Non-trivial lambda functions should be defined as local functions instead.
- Functions that are only used by one parent function should always be a local function.
- Use simple using statements without braces where possible.
- Use collection expressions and collection literals where possible, e.g.:
    - `ImmutableList<Widget> widgets = [.. list];` instead of extensions methods like `ImmutableList<Widget> widgets = list.ToImmutableList();`.
    - `List<Widget> widgets = [];` instead of `List<Widget> widgets = new();`.
- Always prefer `Range` and `Index` operators for taking subsets of an `IEnumerable<T>`, e.g.:
    - `widgets[start..end]` instead of `widgets.GetRange(start, end - start)`.
    - `widgets[^1..]` instead of `widgets.GetRange(widgets.Count - 1, 1)`.
- Use a FrozenDictionary type for any dictionary that does not need to be changed after creation.
- Use ImmutableArray for any collection that does not need to be changed after creation.
- Use ImmutableList for any collection that may need to be changed after creation.
- Always declare variables as `readonly` if they will not be changed after initialization.
- Use async whenever appropriate.
- Unless there is a synchronous overload, do not use the `Async` suffix for async methods.
- "Fire and forget" async calls should always use the appropriately named utility class to make the intention clear. This should happen as far up the call stack as possible, minimizing the chance of calling a synchronous method without realizing it will trigger asynchronous work. All intermediate classes should be async.
- Never explicitly initialize module-level variables to a value that is already that type's default.
    - E.g. do not use `private int _count = 0;` or `private string _name = "";`. Instead, just use `private int _count;` or `private string _name;`.
- Always use `nameof` to refer to member names instead of hardcoding strings.
- Do not use var under any circumstances. Always use explicit types.
    - Use Target-typed new where ever possible. 
        - Never `var example = new Widget();`. Always `Widget example = new();`
- Always use the explicit discard (`_`) character when ignoring the return or `out` parameter of a method.
- Primary constructors should be used unless there is a need to run logic during construction that can't be achieved without primary constructors.
    - Parameters in primary constructors that will not be changed should be used in the class directly rather than assigning them to readonly fields.
- Address all code analyzer messages with codes using prefixes "IDE", "CA"," RCS", and "GSP". Use the Code Fix offered by the analyzer, if it has one.
    - **Not all IDE auto-fixers are safe.** IDE0010 (populate switch) and IDE0072 (populate switch expression) produce broken code when applied blindly. IDE0058 (explicit discard) conflicts with parameters named `_` — rename them first.
    - Prefer `dotnet format style --severity hidden --verify-no-changes` to discover diagnostics, then apply safe diagnostics selectively with `--diagnostics`.
    - Editorconfig should only globally suppress diagnostics that contradict project standards (e.g. `IDE0160` is suppressed because we mandate file-scoped namespaces, but the diagnostic wants block-scoped). Suppress individual instances with inline `#pragma`.
- All regex statements should use generated regex.

## Native code
- All native code declarations should be confined to static classes named to make their purposes clear.
    - Native code declaration classes should not contain logic, unless strictly necessary.
    - All declarations should use `LibraryImport` wherever possible.

## Naming Conventions
- Static types are always prefixed with a `s_` followed by camelCase.
- Private class fields are always prefixed with `_` followed by camelCase.
- Methods, types, Constants, and property names always use PascalCase.
- Interface names always begin with `I` followed by PascalCase. 

### Razor page guidelines
- If the page subscribes to an event from an injected dependency, it should also implement `IAsyncDisposable` and unsubscribe in the `DisposeAsync` method. If no other cleanup is required, the `DisposeAsync` method can finish by returning `ValueTask.CompletedTask;`.
- Services injected into the page should be named like any other private class field, i.e. prefixed with `_` followed by camelCase.

## Error Handling
- Use try/catch blocks for async operations
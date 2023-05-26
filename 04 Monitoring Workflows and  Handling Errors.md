# Monitoring Workflows and Handling Errors

-   Durable Functions REST API
     - Check on workflow progress
-   Storage Account contents
     - Understand “Task Hubs”
-   Handling Errors
     - Exceptions in activity functions
     - Retrying activities
     - Cancelling workflows

# Durable Functions REST API

## Get Orchestration Status API

```html
http://localhost:7071/runtime/webhooks/durabletask/instance
s/4d2b7bb4d7724733982eb0070ff9f54c?taskHub=DurableFunctions
Hub&connection=Storage&code=anZaCbawHp/ufQ2EBukjiml1P1s480n
PImkQ/r5rN2V5XDJ/l/F7lA==&showHistory=true&showHistoryOutpu
t=true
```

&showHistory=true&showHistoryOutpu
t=true

## Looking Inside Task Hubs

- Durable Functions stores state in Azure Storage
- Azure Storage Explorer
    - https://azure.microsoft.com/en-us/features/storageexplorer/
    - Explore contents of online storage accounts
    - Explore local storage emulator contents
- Default Task Hub name
    - Same as the Function App name in Azure
    - “TestHubName” when running outside of Azure
    - Can be configured in host.json

## Handling Errors

Exceptions in Activity Functions

```cs
public static object TranscodeVideo([ActivityTrigger] string input) 
{
    try
    {
        var output = PerformTranscode(input);
        return new { Success = true,
        Output = output };
    } 
    catch (Exception e) 
    {
        return new { Success = false,
        Error = e.Message };
    }
}
```

```cs
public static object TranscodeVideo([ActivityTrigger] string input) 
{
    // don’t catch exceptions, allow them to propagate up to 
    // the orchestrator function
    return PerformTranscode(input);
}
```

Handling Exceptions in Orchestrator Functions

```cs
public static async Task<object> MyOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext ctx) 
{
    try
    {
        var a = await ctx.CallSubOrchestratorAsync<string>("A_1"); 
        var b = await ctx.CallSubOrchestratorAsync<string>("A_2", a);
        var c = await ctx.CallSubOrchestratorAsync<string>("A_3", b);
        return new { Success = true, Output = c }; 
    } 
    catch (Exception e) 
    {
        // log exception, perform cleanup
        return new { Success = false, Error = e.Message }; 
    }
}
```

```cs
public static async Task<object> MyOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext ctx) 
{
    try
    {
        var a = await ctx.CallSubOrchestratorAsync<string>("A_1"); 
        var b = await ctx.CallSubOrchestratorAsync<string>("A_2", a);
        var c = await ctx.CallSubOrchestratorAsync<string>("A_3", b);
        return new { Success = true, Output = c }; 
    } 
    catch (Exception e) 
    {
        await ctx.CallSubOrchestratorAsync<string>("A_Cleanup");
        return new { Success = false, Error = e.Message }; 
    }
}
```

Handling Errors
- Throw an exception from an activity
- Catch it in our orchestrator

## Retrying Activities in Orchestrator Functions

```cs
public static async Task<object> MyOrchestrator(
[OrchestrationTrigger] IDurableOrchestrationContext ctx) 
{
    for (var attempt = 0; attempt < 3; attempt++)
    {
        try
        { 
            await ctx.CallSubOrchestratorAsync<string>("A_1"); 
            break; // success
        }
        catch (Exception e) 
        {
            await Task.Delay(5000); // NO – DON’T DO THIS!!!
        }
    } 
// ... continue if successful or cleanup
}
```

```cs
public static async Task<object> MyOrchestrator(
[OrchestrationTrigger] IDurableOrchestrationContext ctx) 
{
    // retry four up to 4 times 
    // with 5 second delay between each attempt... 
    var a = await ctx.CallActivityWithRetryAsync<string>(
    "ExtractThumbnail"
    ,
    new RetryOptions(TimeSpan.FromSeconds(5), 4), 
    transcodedLocation); 
    // new RetryOptions(TimeSpan.FromSeconds(5), 4)
    // {Handle = ex => ex is InvalidOperationException},
    // transcodedLocation);

    // if we get here ExtractThumbnail was called successfully
}
```

## Cancelling Workflows

Call REST API to terminate orchestration
- No longer needed
- Running too long
- System maintenance

POST to terminate API

terminatePostUri: ...terminate?reason={text}...

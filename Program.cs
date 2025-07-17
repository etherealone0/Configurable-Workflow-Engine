using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/workflow-definitions", (WorkflowDefinition def) =>
{
    if (InMemoryStore.WorkflowDefinitions.ContainsKey(def.Id))
        return Results.BadRequest("Duplicate workflow definition ID.");

    if (def.States.Count(s => s.IsInitial) != 1)
        return Results.BadRequest("Workflow must have exactly one initial state.");

    InMemoryStore.WorkflowDefinitions[def.Id] = def;
    return Results.Ok(def);
});

app.MapGet("/workflow-definitions/{id}", (string id) =>
{
    if (!InMemoryStore.WorkflowDefinitions.TryGetValue(id, out var def))
        return Results.NotFound();

    return Results.Ok(def);
});


app.MapPost("/workflow-instances", (StartInstanceRequest request) =>
{
    if (!InMemoryStore.WorkflowDefinitions.TryGetValue(request.DefinitionId, out var definition))
    {
        return Results.NotFound("Workflow definition not found.");
    }

    var initialState = definition.States.FirstOrDefault(s => s.IsInitial);
    if (initialState == null)
    {
        return Results.BadRequest("Workflow definition has no initial state.");
    }

    var instance = new WorkflowInstance
    {
        DefinitionId = request.DefinitionId,
        CurrentStateId = initialState.Id
    };

    InMemoryStore.WorkflowInstances[instance.Id] = instance;
    return Results.Ok(instance);
});

app.MapPost("/workflow-instances/{instanceId}/execute", (string instanceId, ExecuteActionRequest request) =>
{
    if (!InMemoryStore.WorkflowInstances.TryGetValue(instanceId, out var instance))
        return Results.NotFound("Workflow instance not found.");

    if (!InMemoryStore.WorkflowDefinitions.TryGetValue(instance.DefinitionId, out var definition))
        return Results.Problem("Workflow definition missing for this instance.");

    var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
    if (currentState == null)
        return Results.Problem("Current state not found in definition.");

    if (currentState.IsFinal)
        return Results.BadRequest("Cannot execute action on a final state.");

    var action = definition.Actions.FirstOrDefault(a => a.Id == request.ActionId);
    if (action == null)
        return Results.BadRequest("Action not found in workflow definition.");

    if (!action.Enabled)
        return Results.BadRequest("Action is disabled.");

    if (!action.FromStates.Contains(currentState.Id))
        return Results.BadRequest($"Action cannot be executed from current state '{currentState.Id}'.");

    var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState);
    if (targetState == null || !targetState.Enabled)
        return Results.BadRequest("Target state is invalid or disabled.");

    // Perform transition
    instance.CurrentStateId = targetState.Id;
    instance.History.Add(new ActionHistoryEntry
    {
        ActionId = request.ActionId,
        Timestamp = DateTime.UtcNow
    });

    return Results.Ok(instance);
});

app.MapGet("/workflow-instances/{id}", (string id) =>
{
    if (!InMemoryStore.WorkflowInstances.TryGetValue(id, out var instance))
        return Results.NotFound();

    return Results.Ok(instance);
});

app.MapGet("/workflow-definitions", () =>
{
    return Results.Ok(InMemoryStore.WorkflowDefinitions.Values);
});



app.Run();

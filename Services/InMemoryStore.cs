using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public static class InMemoryStore
{
    public static readonly Dictionary<string, WorkflowDefinition> WorkflowDefinitions = new();
    public static readonly Dictionary<string, WorkflowInstance> WorkflowInstances = new();
}

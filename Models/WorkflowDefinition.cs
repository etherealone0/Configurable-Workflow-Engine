namespace WorkflowEngine.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<State> States { get; set; } = new();
    public List<WorkflowAction> Actions { get; set; } = new();
}

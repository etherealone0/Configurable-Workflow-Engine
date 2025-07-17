# Minimal Workflow Backend Service

## Quick Start

### Requirements
- .NET 8 SDK installed ([installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/))

### Run the Project
```bash
# Restore dependencies (if needed)
dotnet restore

# Run the project
dotnet run
```

Server will start at http://localhost:5000 (or a port specified by .NET)

### Sample Requests

#### 1. Create Workflow Definition
```bash
curl -X POST http://localhost:5000/workflow-definitions \
  -H "Content-Type: application/json" \
  -d '{
    "id": "wf-1",
    "states": [
      { "id": "s1", "name": "Start", "isInitial": true, "isFinal": false, "enabled": true },
      { "id": "s2", "name": "InProgress", "isInitial": false, "isFinal": false, "enabled": true },
      { "id": "s3", "name": "Done", "isInitial": false, "isFinal": true, "enabled": true }
    ],
    "actions": [
      { "id": "a1", "name": "Begin", "enabled": true, "fromStates": ["s1"], "toState": "s2" },
      { "id": "a2", "name": "Finish", "enabled": true, "fromStates": ["s2"], "toState": "s3" }
    ]
  }'
```

#### 2. Get All Definitions
```bash
curl http://localhost:5000/workflow-definitions
```

#### 3. Start Workflow Instance
```bash
curl -X POST http://localhost:5216/workflow-instances \
  -H "Content-Type: application/json" \
  -d '{"definitionId": "wf-1"}'
```

#### 4. Execute Action on Workflow Instance
```bash
# the instance id should be the id obtained after starting the workflow instance
curl -X POST http://localhost:5000/workflow-instances/{instanceId}/execute \
  -H "Content-Type: application/json" \
  -d '{ "actionId": "a1" }'
```

#### 5. Get Instance Details
```bash
curl http://localhost:5000/workflow-instances/{instanceId}
```

### Assumptions and Design Choices

- All data is stored in-memory using singleton services. No database or file storage is used.
- Workflow definitions and states must be unique by id.
- One initial state is required for each workflow definition.
- A workflow instance must start at the initial state and can only execute actions defined in its workflow.
- Executing an action validates:
    - Action is enabled.
    - Current state is in the action's fromStates.
    - The toState is valid.
    - The workflow is not in a final state.


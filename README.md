# AI and Character Task System

### Dependencies
- https://github.com/khncao/com.minus4kelvin.core 
- Tested working on Unity 2020.3.6f1+

### Status
- Rudimentary task system for nav agents
- In progress framework for AI decision making; currently unusable

### Usage
- Tasks
  - NavCharacterControl, NavMeshAgent, Collider, TaskProcessor on task agent
  - NavCharacterControl's nav target object parent
    - TaskInteractTrigger with trigger collider child
  - Create tasks by adding command sequences and register with TaskManager

### Todo
- Prefabs, example
- Decide on AI framework and implement cross-project general-use
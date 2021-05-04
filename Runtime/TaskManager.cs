using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;
using m4k.Characters;

namespace m4k.AI {
public class TaskManager : Singleton<TaskManager>
{
    // TODO: Change to customizable priority queue
    public List<Task> staffTaskQ;
    public List<Task> activeTasks;
    public List<TaskProcessor> taskHandlers;
    public List<TaskProcessor> busyHandlers;
    public GameObject readyItemsTarget;
    public TRegistry<TaskInteractable> taskInteractables;
    public float taskCheckInterval = 1f;

    Dictionary<string, List<TaskTarget>> keyTaskTargetsDict = new Dictionary<string, List<TaskTarget>>();
    Dictionary<string, object> preconditions = new Dictionary<string, object>();
    Dictionary<string, object> effects = new Dictionary<string, object>();
    bool evicting;
    float lastEvictTIme;

    // [NaughtyAttributes.Button]
    public void TestPlayerTask() {
        // CharacterManager.I.Player.ToggleTaskHandling(true);

        Task test = new Task();
        test.AddPathToKeyCmd("test1");
        RegisterTask(test);
    }

    protected override void Awake()
    {
        base.Awake();
        if(m_ShuttingDown) return;

        staffTaskQ = new List<Task>();
        activeTasks = new List<Task>();
        taskHandlers = new List<TaskProcessor>();
        busyHandlers = new List<TaskProcessor>();
    }
    float nextCheckThresh;
    void Update() {
        if(Time.time > nextCheckThresh && taskHandlers.Count > 0 && staffTaskQ.Count > 0) {
            var task = staffTaskQ[staffTaskQ.Count - 1];

            for(int i = 0; i < taskHandlers.Count; ++i) {
                var handler = taskHandlers[i];
                if(!taskHandlers[i].AssignTask(task)) continue;

                task.processor = handler;
                activeTasks.Add(task);
                busyHandlers.Add(handler);
                staffTaskQ.RemoveAt(staffTaskQ.Count - 1);
                taskHandlers.RemoveAt(i);
                // Debug.Log($"Assigned {task.description}");
                return;
            }
            nextCheckThresh = Time.time + taskCheckInterval * Time.timeScale;
        }
    }

    public TaskInteractable GetTaskInteractableInventory(Item item, int count) {
        TaskInteractable taskInteractable = null;
        for(int i = 0; i < taskInteractables.instances.Count; ++i) {
            var inst = taskInteractables.instances[i];
            var amount = inst.inventory.GetItemTotalAmount(item);
            if(amount >= count)
                return inst;
        }
        return taskInteractable;
    }

    public void RegisterTaskHandler(TaskProcessor taskProcessor) {
        taskHandlers.Add(taskProcessor);
    }
    public void UnregisterTaskHandler(TaskProcessor taskProcessor) {
        taskHandlers.Remove(taskProcessor);
    }
    public void CancelTask(Task task, bool unlist = true) {
        if(task == null) return;

        if(activeTasks.Contains(task)) {
            Debug.Log($"Canceling {task.description}");
            task.processor.AbortTask();
            busyHandlers.Remove(task.processor);
            taskHandlers.Add(task.processor);
            activeTasks.Remove(task);
        }

        if(unlist) {
            int taskInd = staffTaskQ.FindIndex(x=>x == task);
            if(taskInd != -1) {
                staffTaskQ.RemoveAt(taskInd);
            }
        }
    }
    public void CompleteTask(Task task) {
        if(task != null && activeTasks.Contains(task)) {
            busyHandlers.Remove(task.processor);
            taskHandlers.Add(task.processor);
            activeTasks.Remove(task);
        }
    }

    void RegisterTask(Task task) {
        staffTaskQ.Add(task);
    }
    public Task RegisterDeliverItemsTask(Transform deliverTrans, ITaskInteractable source = null, List<Item> items = null) 
    {
        var task = new Task() {
            orderer = source,
            description = "deliver items task",
            priority = 10,
        };

        task.AddGetCmd(readyItemsTarget.transform, null, items);
        task.AddGiveCmd(deliverTrans, source, items);

        RegisterTask(task);

        return task;
    }

    public Task RegisterCleanItemsTask(Transform cleanTarget) {
        var task = new Task();
        task.description = "clean target task";
        task.priority = 10;
        // task.orderer = source;
        task.AddCleanCmd(cleanTarget);

        RegisterTask(task);

        return task;
    }

    public Task RegisterRandomCleanTask() {
        var task = new Task();
        task.description = "clean random task";
        task.AddRandomPathCmd();
        task.AddContextActionCmd();

        RegisterTask(task);
        return task;
    }

    public Task RegisterCustomerGreetTask() {
        var task = new Task();
        task.description = "greet customer task";
        // task.AddPathPositionCmd()

        RegisterTask(task);
        return task;
    }

    public Task RegisterGatherTask(Item item) {
        var task = new Task();
        task.description = $"gather {item.itemName} task";
        // task.AddGetCmd()
        // task.AddGiveCmd()

        RegisterTask(task);
        return task;
    }
    public Task RegisterHuntTask(Character character) {
        var task = new Task();
        task.description = $"hunt {character.itemName} task";
        // task.AddGetCmd()

        RegisterTask(task);
        return task;
    }

    public void EvictAll() {
        evicting = true;
        lastEvictTIme = Time.time;

        foreach(var h in busyHandlers)
            h.AbortTask();
    }

    public void RegisterTaskTarget(string key, TaskTarget target) {
        List<TaskTarget> taskTargets;
        if(!keyTaskTargetsDict.TryGetValue(key, out taskTargets)) {
            taskTargets = new List<TaskTarget>();
            keyTaskTargetsDict.Add(key, taskTargets);
        }
        // Debug.Log($"Registered task target key: {key}; {target.gameObject}");
        taskTargets.Add(target);
    }

    public void UnregisterTaskTarget(string key, TaskTarget target) {
        List<TaskTarget> taskTargets;
        if(!keyTaskTargetsDict.TryGetValue(key, out taskTargets)) {
            return;
        }
        taskTargets.Remove(target);
    }

    public TaskTarget GetClosestTaskTarget(string key, Transform t) {
        List<TaskTarget> targets;
        if(!keyTaskTargetsDict.TryGetValue(key, out targets)) {
            Debug.LogWarning($"TaskTargets not found for {key}");
            return null;
        }
        TaskTarget closest = null;
        float closestDist = Mathf.Infinity;
        foreach(var i in targets) {
            var sqrDist = (i.transform.position - t.position).sqrMagnitude;
            if(sqrDist < closestDist) {
                closestDist = sqrDist;
                closest = i;
            }
        }
        return closest;
    }
}
}
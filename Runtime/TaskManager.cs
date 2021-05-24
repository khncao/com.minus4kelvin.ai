using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;

namespace m4k.AI {
public class TaskManager : Singleton<TaskManager>
{
    // TODO: Change to customizable priority queue
    public List<Task> staffTaskQ;
    public List<Task> activeTasks;
    public List<TaskProcessor> taskHandlers;
    public List<TaskProcessor> busyHandlers;
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
        taskInteractables = new TRegistry<TaskInteractable>();
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
        // for(int i = 0; i < busyHandlers.Count; ++i) {
        //     busyHandlers[i].cmdTimer += Time.deltaTime;
        //     if(busyHandlers[i].Timeout)
        //         busyHandlers[i].CommandTimeout();
        // }
    }

    public TaskInteractable GetTaskInteractableInventory(List<Item> items) {
        TaskInteractable taskInteractable = null;

        for(int i = 0; i < taskInteractables.instances.Count; ++i) {
            var inst = taskInteractables.instances[i];
            for(int j = 0; j < items.Count; j++) {
                if(!inst.HasItem(items[j])) {
                    break;
                }
                if(j == items.Count - 1)
                    return inst;
            }

        }
        Debug.LogWarning("Task interact inventory not found");
        return taskInteractable;
    }
    // public TaskInteractable GetTaskInteractableInventory(Item item, int count) {
    //     TaskInteractable taskInteractable = null;

    //     for(int i = 0; i < taskInteractables.instances.Count; ++i) {
    //         var inst = taskInteractables.instances[i];
    //         if(!inst.HasItem(item)) 
    //             continue;
    //         var amount = inst.GetItemTotal(item);
    //         if(amount >= count)
    //             return inst;
    //     }
    //     Debug.LogWarning("Task interact inventory not found");
    //     return taskInteractable;
    // }

    public void RegisterTaskHandler(TaskProcessor taskProcessor) {
        taskHandlers.Add(taskProcessor);
    }
    public void UnregisterTaskHandler(TaskProcessor taskProcessor) {
        taskHandlers.Remove(taskProcessor);
    }
    public void AbortTask(Task task) {
        if(activeTasks.Contains(task)) {
            busyHandlers.Remove(task.processor);
            taskHandlers.Add(task.processor);
            activeTasks.Remove(task);
            staffTaskQ.Add(task);
        }
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
        var inv = GetTaskInteractableInventory(items);
        if(!inv) Debug.Log("No inv");
        task.AddGetCmd(inv.transform, null, items);
        task.AddGiveCmd(deliverTrans, source, items);

        RegisterTask(task);

        return task;
    }

    public Task RegisterActionTask(string key, Transform target = null, int p = 10, bool destroyTarget = false) {
        var task = new Task();
        task.description = $"{key} task";
        task.priority = p;
        if(target) {
            task.AddPathTargetCmd(target);
        }
        task.AddActionCmd(key, target, destroyTarget);

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
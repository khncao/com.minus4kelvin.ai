using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
public class TaskProcessor : MonoBehaviour
{
    public TaskInteractTrigger taskInteractObj;
    public ItemArranger taskInventory; // visual of current task inventory
    public Animator anim;
    public bool enable = true;
    public float defaultCmdTimeout = 10f;
    public float cmdTimer;

    public System.Action onTaskComplete;

    public bool HasTask { get { return currTask != null; } }
    public bool Timeout { get { return cmdTimer > defaultCmdTimeout; }}

    IMoveTargetable movable;
    float durationTimer;
    bool abortingTask;
    public Task currTask;
    public TaskCommand currCmd;

    void Start()
    {
        movable = GetComponentInChildren<IMoveTargetable>();
        if(movable == null)
            movable = GetComponentInParent<IMoveTargetable>();
        if(movable == null) {
            Debug.LogError($"No movable interface found on {gameObject}");
        }
        if(!anim) anim = GetComponent<Animator>();

        if(!taskInteractObj) Debug.LogError($"{gameObject} needs taskInteractObj TaskInteractTrigger");
        taskInteractObj.processor = this;
        taskInteractObj.processorCol = GetComponentInChildren<Collider>();
        taskInteractObj.gameObject.SetActive(false);
        RegisterHandler();
        movable.OnArrive += OnNavArrive;
    }

    void OnNavArrive() {
        if(currCmd != null && currCmd.cmdType == CommandTypes.Path) {
            TaskInteract();
        }
    }

    void RegisterHandler() {
        currTask = null;
        if(enable)
            TaskManager.I.RegisterTaskHandler(this);
    }

    void GetNextCommand() {
        if(currTask.IsEmpty()) {
            // Debug.Log($"Completed {currTask.description}");
            TaskManager.I.CompleteTask(currTask);
            onTaskComplete?.Invoke();
            CleanupTask();
            return;
        }
        if(abortingTask)
            return;
        currCmd = currTask.GetNextCommand();
        cmdTimer = 0f;

        // Debug.Log($"Current target transform: {currCmd.cmdType.ToString()} {currCmd.targetTrans}");
        taskInteractObj.gameObject.SetActive(true);

        if(currCmd.targetTrans) {
            movable.SetTarget(currCmd.targetTrans, true);
        }
        else if(currCmd.cmdType == CommandTypes.Action) {
            ProcessCommand();
            return;
        }
        else if(!string.IsNullOrEmpty(currCmd.key)) {
            var t = TaskManager.I.GetClosestTaskTarget(currCmd.key, transform);
            if(t) {
                movable.SetTarget(t.transform, true);
            }
        }
        else {
            Debug.Log($"Aborting {currTask.description}: No target transform");
            AbortTask();
            return;
        }
        taskInteractObj.command = currCmd;
        durationTimer = currCmd.duration;
    }

    void ProcessCommand() {
        switch(currCmd.cmdType) {
            case CommandTypes.Get: {
                if(taskInventory) {
                    taskInventory.gameObject.SetActive(true);
                    taskInventory.UpdateItems(currCmd.orderItems);
                }
                break;
            }
            case CommandTypes.Give: {
                if(taskInventory) {
                    taskInventory.HideItems();
                    taskInventory.gameObject.SetActive(false);
                }
                break;
            }
            case CommandTypes.Action: {
                anim.SetTrigger(currCmd.key);
                break;
            }
        }

        if(currCmd.destroyTarget && currCmd.targetTrans)
            Destroy(currCmd.targetTrans.gameObject);
    }

    public void TaskInteract() {
        if(currCmd == null || abortingTask) 
            return;

        if(currTask == taskInteractObj.task && currCmd == taskInteractObj.command) 
        {
            // Debug.Log($"{currCmd.cmdType.ToString()} Interact");
            movable.Stop();
            taskInteractObj.gameObject.SetActive(false);
            ProcessCommand();
            currCmd.targetInteractable?.OnTaskInteract(currTask);
            currCmd = null;
            GetNextCommand();
        }
    }

    public bool AssignTask(Task task) {
        if(currTask != null && currTask.priority >= task.priority) {
            Debug.LogWarning("Error: already has task");
            return false;
        }
        if(Time.time - lastAbortTime < 5f) {
            return false;
        }
        abortingTask = false;
        currTask = task;
        taskInteractObj.task = task;
        
        GetNextCommand();
        return true;
    }

    float lastAbortTime;
    public void AbortTask() {
        lastAbortTime = Time.time;
        abortingTask = true;

        // Debug.Log("Abort");
        CleanupTask();
    }

    public void CommandTimeout() {
        Debug.Log("Command timeout");
        TaskManager.I.AbortTask(currTask);
        AbortTask();
    }

    void CleanupTask() {
        taskInteractObj.gameObject.SetActive(false);
        if(taskInventory)
            taskInventory.gameObject.SetActive(false);
        currTask = null;
        currCmd = null;
        taskInteractObj.task = null;
        taskInteractObj.command = null;
        cmdTimer = 0f;
    }
}
}
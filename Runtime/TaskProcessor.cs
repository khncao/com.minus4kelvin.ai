using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;
using m4k.InventorySystem;

namespace m4k.AI {
public class TaskProcessor : MonoBehaviour
{
    public NavCharacterControl navChar;
    public TaskInteractObject taskInteractObj;
    public ItemArranger taskInventory; // visual of current task inventory
    public System.Action onTaskComplete;
    public bool enable = true;
    
    public bool HasTask { get { return currTask != null; } }

    CharacterAnimation charAnim;
    float durationTimer;
    bool abortingTask;
    Task currTask;
    Command currCmd;


    void Start()
    {
        if(!navChar)
            navChar = GetComponentInChildren<NavCharacterControl>();
        if(!navChar)
            navChar = GetComponentInParent<NavCharacterControl>();
        charAnim = GetComponent<CharacterAnimation>();

        taskInteractObj = navChar.pathTarget.GetComponentInChildren<TaskInteractObject>();
        taskInteractObj.processor = this;
        taskInteractObj.processorCol = GetComponentInChildren<Collider>();
        taskInteractObj.gameObject.SetActive(false);
        if(enable) RegisterHandler();
        navChar.onArrive += OnNavArrive;
    }

    void OnNavArrive(Transform target) {
        if(currCmd != null && currCmd.cmdType == CommandTypes.Path) {
            TaskInteract();
        }
    }

    void RegisterHandler() {
        currTask = null;
        if(enable)
            TaskManager.I.RegisterTaskHandler(this);
    }

    void HandleCommandTypes() {
        switch(currCmd.cmdType) {
            case CommandTypes.Get: {
                taskInventory.gameObject.SetActive(true);
                taskInventory.UpdateItems(currCmd.cmdItems);
                break;
            }
            case CommandTypes.Give: {
                taskInventory.gameObject.SetActive(false);
                taskInventory.HideItems();
                break;
            }
            case CommandTypes.Clean: {
                charAnim.CleanFloor();
                if(currCmd.targetTrans)
                    Destroy(currCmd.targetTrans.gameObject);
                break;
            }
        }
    }

    public void TaskInteract() {
        if(currCmd == null || abortingTask) 
            return;

        if(currTask == taskInteractObj.task && currCmd == taskInteractObj.command) 
        {
            navChar.StopAgent();
            // taskInteractable.transform.SetParent(transform);
            taskInteractObj.gameObject.SetActive(false);
            HandleCommandTypes();
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
        taskInteractObj.gameObject.SetActive(false);
        currTask = task;
        taskInteractObj.processor = this;
        taskInteractObj.task = task;

        // Debug.Log("Assigned task");
        
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

        // Debug.Log("Next command: " + currCmd.cmdType.ToString());
        // Debug.Log($"Current target transform: {currCmd.targetTrans}");

        if(currCmd.targetTrans) {
            navChar.SetTarget(currCmd.targetTrans);
            taskInteractObj.gameObject.SetActive(true);
            // taskInteractable.transform.SetParent(currCmd.targetTrans, false);
        }
        else if(!string.IsNullOrEmpty(currCmd.key)) {
            var t = TaskManager.I.GetClosestTaskTarget(currCmd.key, transform);
            if(t)
                navChar.SetTarget(t.transform);
        }
        else {
            Debug.Log($"Aborting {currTask.description}: No target transform");
            AbortTask();
            return;
        }
        taskInteractObj.command = currCmd;
        durationTimer = currCmd.duration;
    }

    void CleanupTask() {
        if(currTask == null)
            return;

        taskInteractObj.gameObject.SetActive(false);
        currTask = null;
        currCmd = null;
        taskInteractObj.processor = null;
        taskInteractObj.task = null;
        taskInteractObj.command = null;
    }
}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {

[System.Serializable]
public class Task
{
    public int priority;
    public string description;
    public TaskProcessor processor;
    public ITaskInteractable orderer;
    public TaskCommand currentCommand;
    public List<TaskCommand> taskCommandQ = new List<TaskCommand>();

    public void AddPathPositionCmd(Vector3 pos) {
        AddCommand(new TaskCommand() {
            cmdType = CommandTypes.Path,
            targetPos = pos
        });
    }
    public void AddPathTargetCmd(Transform target) 
    {
        AddCommand(new TaskCommand() { 
            cmdType = CommandTypes.Path, 
            targetTrans = target,
        });
    }
    public void AddRandomPathCmd() {
        AddCommand(new TaskCommand() {
            cmdType = CommandTypes.Path,

        });
    }
    public void AddPathToKeyCmd(string k) {
        AddCommand(new TaskCommand() {
            cmdType = CommandTypes.Path,
            key = k,
        });
    }
    public void AddContextActionCmd() {
        AddCommand(new TaskCommand() {
            cmdType = CommandTypes.Action,
        });
    }
    public void AddWaitCmd(float time) 
    {
        AddCommand(new TaskCommand() { 
            cmdType = CommandTypes.Wait, 
            duration = time,
        });
    }
    public void AddGetCmd(Transform targetTrans, ITaskInteractable target = null, List<Item> items = null) 
    {
        AddCommand(new TaskCommand() { 
            cmdType = CommandTypes.Get, 
            orderItems = items,
            targetInteractable = target,
            targetTrans = targetTrans,
        });
    }
    public void AddGiveCmd(Transform targetTrans, ITaskInteractable target = null, List<Item> items = null) 
    {
        AddCommand(new TaskCommand() { 
            cmdType = CommandTypes.Give, 
            orderItems = items,
            targetInteractable = target,
            targetTrans = targetTrans,
        });
    }
    public void AddActionCmd(string k, Transform target = null, bool des = false) {
        AddCommand(new TaskCommand() {
            cmdType = CommandTypes.Action,
            key = k,
            targetTrans = target,
            destroyTarget = des,
        });
    }
    
    public void AddCommand(TaskCommand staffCommand) {
        taskCommandQ.Add(staffCommand);
    }

    public TaskCommand GetNextCommand() {
        if(taskCommandQ.Count == 0) {
            Debug.LogError("Requested command from empty task command queue");
            return null;
        }

        currentCommand = taskCommandQ[0];
        taskCommandQ.RemoveAt(0);

        return currentCommand;
    }

    public bool IsEmpty() {
        return taskCommandQ.Count <= 0;
    }
}

public interface ITaskInteractable {
    void OnTaskInteract(Task task);
}
}
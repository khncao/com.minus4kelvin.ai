using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;

namespace m4k.AI {

[System.Serializable]
public class Task
{
    public string description;
    public TaskProcessor processor;
    public ITaskInteractable orderer;
    public List<Command> taskCommandQ = new List<Command>();
    public Command currentCommand;
    public int priority;

    public void AddPathPositionCmd(Vector3 pos) {
        AddCommand(new Command() {
            cmdType = CommandTypes.Path,
            targetPos = pos
        });
    }
    public void AddPathTargetCmd(Transform target) 
    {
        AddCommand(new Command() { 
            cmdType = CommandTypes.Path, 
            targetTrans = target,
        });
    }
    public void AddRandomPathCmd() {
        AddCommand(new Command() {
            cmdType = CommandTypes.Path,

        });
    }
    public void AddPathToKeyCmd(string k) {
        AddCommand(new Command() {
            cmdType = CommandTypes.Path,
            key = k,
        });
    }
    public void AddContextActionCmd() {
        AddCommand(new Command() {
            cmdType = CommandTypes.Action,
        });
    }
    public void AddWaitCmd(float time) 
    {
        AddCommand(new Command() { 
            cmdType = CommandTypes.Wait, 
            duration = time,
        });
    }
    public void AddGetCmd(Transform targetTrans, ITaskInteractable target = null, List<Item> items = null) 
    {
        AddCommand(new Command() { 
            cmdType = CommandTypes.Get, 
            cmdItems = items,
            targetInteractable = target,
            targetTrans = targetTrans,
        });
    }
    public void AddGiveCmd(Transform targetTrans, ITaskInteractable target = null, List<Item> items = null) 
    {
        AddCommand(new Command() { 
            cmdType = CommandTypes.Give, 
            cmdItems = items,
            targetInteractable = target,
            targetTrans = targetTrans,
        });
    }
    public void AddActionCmd() {
    }
    public void AddCleanCmd(Transform targetTrans) {
        AddCommand(new Command() { 
            cmdType = CommandTypes.Clean, 
            targetTrans = targetTrans,
        });
    }
    
    public void AddCommand(Command staffCommand) {
        taskCommandQ.Add(staffCommand);
    }

    public Command GetNextCommand() {
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
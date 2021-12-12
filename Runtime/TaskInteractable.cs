using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
public enum TaskInteractableType {
    None = 0, Inventory = 1, 
}
public class TaskInteractable : MonoBehaviour, ITaskInteractable
{
    public bool isQuantitive;
    public TaskInteractableType interactableType;
    public List<ItemInstance> items;

    [System.NonSerialized]
    Inventory inventory;

    private void Start() {
        inventory = new Inventory(16);
        foreach(var i in items)
            inventory.AddItemAmount(i.item, i.amount);

        TaskManager.I.taskInteractables.RegisterInstance(this);
    }

    private void OnEnable() {
        TaskManager.I?.taskInteractables?.RegisterInstance(this);
    }
    private void OnDisable() {
        TaskManager.I?.taskInteractables.UnregisterInstance(this);
    }
    
    public void OnTaskInteract(Task task) {
        if(task.currentCommand.cmdType == CommandTypes.Get) {
            if(isQuantitive) {
                // 
            }
        }
        else if(task.currentCommand.cmdType == CommandTypes.Give) {
            if(isQuantitive) {

            }
        }
    }

    public bool HasItem(Item item) {
        return inventory.HasItem(item);
    }
    public int GetItemTotal(Item item) {
        return inventory.GetItemTotalAmount(item);
    }
}
}
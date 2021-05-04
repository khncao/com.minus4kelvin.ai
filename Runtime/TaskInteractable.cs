using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;

namespace m4k.AI {
public enum TaskInteractableType {
    None = 0, Inventory = 1, 
}
public class TaskInteractable : MonoBehaviour, ITaskInteractable
{
    public TaskInteractableType interactableType;
    public Inventory inventory;

    private void OnEnable() {
        TaskManager.I.taskInteractables.RegisterInstance(this);
    }
    private void OnDisable() {
        TaskManager.I.taskInteractables.UnregisterInstance(this);
    }
    
    public void OnTaskInteract(Task task) {

    }
}
}
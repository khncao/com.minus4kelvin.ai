using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
public class TaskInteractable : MonoBehaviour
{
    public Task task;
    public Command command;
    public TaskProcessor processor;
    public Collider processorCol;

    private void OnTriggerEnter(Collider other) {
        if(other == processorCol) {
            processor.TaskInteract();
        }
    }
}
}
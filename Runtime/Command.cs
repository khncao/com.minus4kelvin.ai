using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.InventorySystem;

namespace m4k.AI {
public enum CommandTypes {
    Path, Wait, Get, Give, Action, Clean
}

[System.Serializable]
public class Command 
{
    public CommandTypes cmdType;
    public List<Item> cmdItems;
    public ITaskInteractable targetInteractable;
    public Transform targetTrans;
    public Vector3 targetPos;
    public string key;
    public float duration;
}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
public enum CommandTypes {
    Path, Wait, Get, Give, Action
}

[System.Serializable]
public class TaskCommand 
{
    public CommandTypes cmdType;
    public ITaskInteractable targetInteractable;
    public Transform targetTrans;
    public Vector3 targetPos;
    public string key;
    public float duration;
    public List<Item> orderItems;
    public bool destroyTarget;
}
}
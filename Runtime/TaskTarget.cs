using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
public class TaskTarget : MonoBehaviour
{
    public List<string> keys;
    
    private void Start() {
        foreach(var k in keys)
            TaskManager.I.RegisterTaskTarget(k, this);
    }
    private void OnDestroy() {
        if(!TaskManager.I) return;
        foreach(var k in keys)
            TaskManager.I.UnregisterTaskTarget(k, this);
    }
}
}
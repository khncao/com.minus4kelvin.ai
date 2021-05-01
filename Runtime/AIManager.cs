using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
public class AIManager : Singleton<AIManager>
{
    public List<AIStateController> aIBehaviours;

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) return;

    }

    void Update()
    {
        foreach(var a in aIBehaviours) {
            a.OnUpdate();
        }
    }

    public void RegisterAI(AIStateController aI) {
        if(aIBehaviours.Contains(aI)) {
            Debug.LogError("Already contains aibehaviour");
            return;
        }
        aIBehaviours.Add(aI);
    }

    public void UnregisterAI(AIStateController aI) {
        aIBehaviours.Remove(aI);
    }
}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
public abstract class State 
{
    public AIStateController behaviour;

    public State(AIStateController behaviour) {
        this.behaviour = behaviour;
    }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
}
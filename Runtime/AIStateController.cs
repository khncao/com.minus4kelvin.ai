using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k.AI {
public class AIStateController : MonoBehaviour
{
    public Character character; // for profile data for behaviour variation
    public Actor actor;
    public NavCharacterControl navChar;
    public Animator anim;

    State currState;

    public virtual void Awake() {
        if(!actor) actor = GetComponent<Actor>();
    }
    public virtual void OnEnable() {
        AIManager.I.RegisterAI(this);
    }
    public virtual void OnDisable() {
        AIManager.I?.UnregisterAI(this);
    }

    public virtual void OnUpdate() {
        currState?.OnUpdate();
    }

    public void ChangeState(State newState) {
        if(currState != null) this.currState.OnExit();
        this.currState = newState;
        this.currState.OnEnter();
    }
}
}
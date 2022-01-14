// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

namespace m4k.AI {
public class StateMachine
{
    public event System.Action OnStateComplete;
    public StateProcessor processor { get; private set; }
    
    IState _currState;

    public StateMachine(StateProcessor processor) {
        this.processor = processor;
    }

    public virtual void OnUpdate() {
        if(_currState == null)
            return;

        if(_currState.OnUpdate()) {
            _currState.OnExit();
            _currState = null;
            OnStateComplete?.Invoke();
        }
    }

    public void ChangeState(IState newState) {
        _currState?.OnExit();
        _currState = newState;
        newState?.OnEnter(processor);
    }

    /// <summary>
    /// True if no current state or current state priority lower than parameter state priorty
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool CanChangeState(IState state) {
        return _currState == null || _currState.priority < state.priority;
    }
}
}
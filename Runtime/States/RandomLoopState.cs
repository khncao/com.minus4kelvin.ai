using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Random IStates from list; interval between states; states should be recyclable
/// </summary>
public class RandomLoop : IState {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentCommand { get; private set; }

    List<IState> _states;
    float _intervalBetweenStates;
    
    float _transitionTimer;

    public RandomLoop(float intervalBetweenStates, int priority = -1) {
        this._intervalBetweenStates = intervalBetweenStates;
        this.priority = priority;
        _states = new List<IState>();
    }

    public void AddState(IState state) {
        _states.Add(state);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        if(_states == null || _states.Count < 1) {
            Debug.LogError("Error: invalid list");
            return;
        }
        currentCommand = _states[0];
        currentCommand.OnEnter(processor);
        _transitionTimer = _intervalBetweenStates;
    }

    public bool OnUpdate() {
        if(currentCommand != null && currentCommand.OnUpdate()) {
            _transitionTimer = _intervalBetweenStates;
            currentCommand.OnExit();
            currentCommand = null;
        }

        if(_transitionTimer > 0f) {
            _transitionTimer -= Time.deltaTime;
        }
        else if(currentCommand == null) {
            int index = Random.Range(0, _states.Count);

            currentCommand = _states[index];
            currentCommand.OnEnter(processor);
        }

        return false;
    }

    public void OnExit() {
        currentCommand?.OnExit();
        _transitionTimer = 0f;
        currentCommand = null;
    }
}

[System.Serializable]
public class RandomLoopWrapper : StateWrapper {
    public float intervalBetweenStates;

    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<StateWrapper> states;

    public override IState GetState() {
        var state = new RandomLoop(intervalBetweenStates, priority);
        foreach(var s in states) {
            state.AddState(s.GetState());
        }
        return state;
    }
}
}
using UnityEngine;
using UnityEngine.Events;

namespace m4k.AI {
/// <summary>
/// Performs state when UnityEvent is invoked. Script only until another way to reference externally invoked UnityEvent such as SO
/// </summary>
public class UnityEventListener : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }

    IState _state;
    UnityEvent _unityEvent;

    bool _reacting;

    public UnityEventListener(UnityEvent unityEvent, IState state, int priority = -1, StateProcessor processor = null) {
        this._state = state;
        this._unityEvent = unityEvent;
        this._reacting = false;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        _unityEvent.AddListener(OnEvent);
    }

    public bool OnUpdate() {
        if(_reacting && _state.OnUpdate()) {
            _state.OnExit();
            _reacting = false;
            return true;
        }
        return false;
    }

    public void OnExit() {
        _unityEvent.RemoveListener(OnEvent);
        _state.OnExit();
        _reacting = false;
    }

    void OnEvent() {
        if(_reacting) return;
        _unityEvent.RemoveListener(OnEvent);
        _state.OnEnter(processor);
        _reacting = true;
    }
}

// [System.Serializable]
// public class UnityEventListenerWrapper : StateWrapper {
//     [SerializeReference]
// #if SERIALIZE_REFS
//     [SubclassSelector]
// #endif
//     public StateWrapper reactState;
//     public UnityEvent unityEvent;


//     public override IState GetState() {
//         return new UnityEventListener(unityEvent, reactState.GetState(), priority);
//     }
// }
}
using UnityEngine;

namespace m4k.AI {
public struct Interact : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }

    IStateInteractable _interactable;

    public Interact(IStateInteractable interactable, int priority = -1, StateProcessor processor = null) {
        this._interactable = interactable;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        if(_interactable == null && processor.currentTarget) {
            if(processor.currentTarget is IStateInteractable interactable)
                _interactable = interactable;
        }
        _interactable.OnStateInteract(this);
    }

    public bool OnUpdate() => true;

    public void OnExit() { }
}

[System.Serializable]
public class InteractWrapper : StateWrapper {
    [Tooltip("GameObject with component that implements IStateInteractable")]
    public GameObject target;

    public override IState GetState() {
        IStateInteractable t = null;
        target.TryGetComponent<IStateInteractable>(out t);
        return new Interact(t, priority);
    }
}

[CreateAssetMenu(fileName = "InteractCommand", menuName = "Data/AI/States/InteractCommand", order = 0)]
public class InteractCommand : StateWrapperBase {
    [Tooltip("GameObject with component that implements IStateInteractable")]
    public GameObject target;

    public override IState GetState() {
        IStateInteractable t = null;
        target.TryGetComponent<IStateInteractable>(out t);
        return new Interact(t, priority);
    }
}
}
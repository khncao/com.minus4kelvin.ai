
namespace m4k.AI {
// animationclip length instead? combine anim+audio+particle?
// playable instead to reduce animator state ambiguity
public struct TriggerAnimation : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    string _paramName;

    // bool _triggerSet;
    // assuming 1 transition to goal: 2 for complete; 1 to state + 1 exit state
    int _stateChangeCount; 

    public TriggerAnimation(string paramName, int priority = -1, StateProcessor processor = null) {
        this._paramName = paramName;
        this.processor = processor;
        this.priority = priority;
        // _triggerSet = false;
        _stateChangeCount = 0;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        processor.anim.SetTrigger(_paramName);
        // _triggerSet = true;
    }

    public bool OnUpdate() {
        // if(_triggerSet) {
            if(processor.CheckAnimStateChanged())
                _stateChangeCount++;
            if(_stateChangeCount == 2)
                return true;
        // }
        return false;
    }

    public void OnExit() {
        processor?.anim?.ResetTrigger(_paramName);
        // _triggerSet = false;
        _stateChangeCount = 0;
    }
}

[System.Serializable]
public class TriggerAnimationWrapper : StateWrapper {
    public string paramName;

    public override IState GetState() {
        return new TriggerAnimation(paramName, priority);
    }
}
}
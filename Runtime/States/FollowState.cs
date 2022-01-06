using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
public class Follow : IState {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentCommand { get; private set; }

    Transform _target;
    Path _pathCommand;
    float _squaredFollowThreshold;

    public Follow(Transform target, float squaredFollowThreshold, int priority = -1, StateProcessor processor = null) {
        this.processor = processor;
        this.priority = priority;
        this._target = target;
        this._squaredFollowThreshold = squaredFollowThreshold;
        _pathCommand = new Path(_target);
        currentCommand = null;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _pathCommand.OnEnter(processor);
        currentCommand = _pathCommand;

        if(_target == null && processor.currentTarget != null) {
            _target = processor.currentTarget;
        }
    }

    public bool OnUpdate() {
        if(currentCommand != null && _pathCommand.OnUpdate()) {
            _pathCommand.OnExit();
            currentCommand = null;
        }
        else if(currentCommand == null && PastThreshold()) {
            _pathCommand.OnEnter(processor);
            currentCommand = _pathCommand;
        }
        return false;
    }

    public void OnExit() {
        _pathCommand.OnExit();
    }

    bool PastThreshold() {
        return (processor.transform.position - _target.position).sqrMagnitude > _squaredFollowThreshold;
    }
}

[System.Serializable]
public class FollowWrapper : StateWrapper {
    [Tooltip("Leaving null will try to find targetTag, then try processor currentTarget")]
    public Transform target;
    public string targetTag;
    public float squaredFollowThreshold;

    public override IState GetState() {
        if(!string.IsNullOrEmpty(targetTag)) {
            target = GameObject.FindGameObjectWithTag(targetTag).transform;
        }
        return new Follow(target, squaredFollowThreshold, priority);
    }
}
}
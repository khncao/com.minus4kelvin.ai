using UnityEngine;

namespace m4k.AI {
public struct Path : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    Transform _targetTransform;
    Vector3 _targetPosition;

    // bool _arrived;

    public Path(Transform t, int priority = -1, StateProcessor processor = null) {
        _targetTransform = t;
        _targetPosition = Vector3.zero;
        this.processor = processor;
        this.priority = priority;
        // this._arrived = false;
    }

    public Path(Vector3 pos, int priority = -1, StateProcessor processor = null) {
        _targetPosition = pos;
        _targetTransform = null;
        this.processor = processor;
        this.priority = priority;
        // this._arrived = false;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        // this._arrived = false;

        if(_targetTransform)
            processor.movable.SetTarget(_targetTransform);
        else
            processor.movable.SetTarget(_targetPosition);
        
        processor.ToggleProximityTrigger(true);
        // processor.movable.OnArrive += OnArrive;
        // processor.onArrive += OnArrive;
    }

    public bool OnUpdate() {
        return !processor.movable.IsMoving;
    }

    public void OnExit() {
        // processor.movable.OnArrive -= OnArrive;
        // processor.onArrive -= OnArrive;
        processor.movable.Stop();
    }

    public void AssignTarget(Transform t) {
        _targetTransform = t;
    }

    public void AssignTarget(Vector3 pos) {
        _targetPosition = pos;
    }

    // void OnArrive() {
    //     // processor.movable.Stop();
    //     this._arrived = true;
    // }
}

[System.Serializable]
public class PathWrapper : StateWrapper {
    [Tooltip("Will fallback to targetPosition if null")]
    public Transform targetTransform;
    public Vector3 targetPosition;

    public override IState GetState() {
        if(targetTransform)
            return new Path(targetTransform, priority);
        else
            return new Path(targetPosition, priority);
    }
}
}
using UnityEngine;
using UnityEngine.AI;

namespace m4k.AI {
public struct Wander : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    Vector3 _targetPosition;
    float _radius;

    // bool _arrived;

    public Wander(Vector3 pos, float radius, int priority = -1, StateProcessor processor = null) {
        this._radius = radius;
        this._targetPosition = pos;
        this.processor = processor;
        this.priority = priority;
        // this._arrived = false;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        // this._arrived = false;

        Vector3 pivotPos = _targetPosition == Vector3.zero ? processor.transform.position : _targetPosition;
        Vector3 newPosition = pivotPos + Random.insideUnitSphere * _radius;
        NavMeshHit hit;
        // if(NavMesh.SamplePosition(newPosition, out hit, _radius, NavMesh.GetAreaFromName("Indoors"))) { }
        if(NavMesh.SamplePosition(newPosition, out hit, _radius, NavMesh.AllAreas)) {
            newPosition = hit.position;
        }
        processor.movable.SetTarget(newPosition);
        
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

    // void OnArrive() {
    //     // processor.movable.Stop();
    //     _arrived = true;
    // }
}

[System.Serializable]
public class WanderWrapper : StateWrapper {
    [Tooltip("Leave at default 0, 0, 0 to pivot from current processor position")]
    public Vector3 pivotPosition;
    public float radius;

    public override IState GetState() {
        return new Wander(pivotPosition, radius, priority);
    }
}

[CreateAssetMenu(fileName = "WanderCommand", menuName = "Data/AI/States/WanderCommand", order = 0)]
public class WanderCommand : StateWrapperBase {
    [Tooltip("Leave at default 0, 0, 0 to pivot from current processor position")]
    public Vector3 pivotPosition;
    public float radius;

    public override IState GetState() {
        return new Wander(pivotPosition, radius, priority);
    }
}
}
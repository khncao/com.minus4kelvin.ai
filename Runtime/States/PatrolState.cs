using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Ordered or random loop super state for sequentially patrolling set of transform points
/// </summary>
public class Patrol : IState {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentCommand { get; private set; }

    List<Transform> _orderedPoints;
    Path _pathCommand;
    int _currentIndex;
    bool _randomOrder;
    float _nextPathDelay;

    float _transitionTimer;

    public Patrol(List<Transform> orderedPoints, bool randomOrder, float nextPathDelay, int priority = -1) {
        this._orderedPoints = orderedPoints;
        this._randomOrder = randomOrder;
        this._nextPathDelay = nextPathDelay;
        this.priority = priority;
        if(orderedPoints == null || orderedPoints.Count < 1) {
            Debug.LogError("Error: invalid orderedPoints");
            return;
        }
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        // get closest point
        Transform closest = null;
        processor.transform.GetClosest<Transform>(_orderedPoints, out closest);
        _currentIndex = _orderedPoints.FindIndex(x=>x == closest);

        _pathCommand = new Path(_orderedPoints[_currentIndex]);
        _pathCommand.OnEnter(processor);
        currentCommand = _pathCommand;
    }

    public bool OnUpdate() {
        if(currentCommand != null && currentCommand.OnUpdate()) {
            _transitionTimer = _nextPathDelay;
            currentCommand.OnExit();
            currentCommand = null;
        }

        if(_transitionTimer > 0f) {
            _transitionTimer -= Time.deltaTime;
        }
        else if(currentCommand == null) {
            _currentIndex = _randomOrder ? Random.Range(0, _orderedPoints.Count) : (_currentIndex + 1) % _orderedPoints.Count;

            _pathCommand.AssignTarget(_orderedPoints[_currentIndex]);
            currentCommand = _pathCommand;
            currentCommand.OnEnter(processor);
        }
        return false;
    }

    public void OnExit() {
        currentCommand?.OnExit();
    }
}

[System.Serializable]
public class PatrolWrapper : StateWrapper {
    public List<Transform> points;
    public bool randomOrder;
    public float nextPathDelay;

    public override IState GetState() {
        return new Patrol(points, randomOrder, nextPathDelay, priority);
    }
}

[CreateAssetMenu(fileName = "PatrolState", menuName = "Data/AI/States/PatrolState", order = 0)]
public class PatrolState : StateWrapperBase {
    public List<Transform> points;
    public bool randomOrder;
    public float nextPathDelay;

    public override IState GetState() {
        return new Patrol(points, randomOrder, nextPathDelay, priority);
    }
}
}
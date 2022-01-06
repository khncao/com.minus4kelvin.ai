using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Super state; ordered sequence of command states to perform complex task.
/// </summary>
[System.Serializable]
public class StateSequence : IState
{
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }

    LinkedList<IState> _commandList;
    LinkedListNode<IState> _currentNode;
    IState _currentCommand;

    public StateSequence(int priority = -1, StateProcessor processor = null) {
        this.priority = priority;
        this.processor = processor;
        this._currentCommand = null;
        _commandList = new LinkedList<IState>();
        _currentNode = null;
    }

    public void Enqueue(IState command) {
        _commandList.AddLast(command);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        _currentNode = _commandList.First;
        _currentCommand = _currentNode.Value;
        _currentCommand.OnEnter(processor);
    }

    public bool OnUpdate() {
        if(_currentCommand != null && _currentCommand.OnUpdate()) {
            _currentCommand.OnExit();

            if(_currentNode == _commandList.Last) {
                return true;
            }
            else {
                _currentNode = _currentNode.Next;
                _currentCommand = _currentNode.Value;
                _currentCommand.OnEnter(processor);
            }
        }
        return false;
    }

    public void OnExit() {
        _currentCommand?.OnExit();
        _currentCommand = null;
    }
}

[System.Serializable]
public class StateSequenceWrapper : StateWrapper {
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<StateWrapper> states;

    public override IState GetState() {
        var state = new StateSequence(priority);
        foreach(var s in states) {
            state.Enqueue(s.GetState());
        }
        return state;
    }
}
}
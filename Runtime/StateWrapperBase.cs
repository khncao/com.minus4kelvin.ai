using System;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Base for deriving state wrappers used in StatesProfile and other editor editing applications
/// </summary>
[Serializable]
public abstract class StateWrapper {
    public int priority;
    public Conditions conditions;

    public abstract IState GetState();
}

public abstract class StateWrapperBase : ScriptableObject {
    public int priority;
    public Conditions conditions;

    public abstract IState GetState();
}
}
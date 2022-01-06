using UnityEngine;

namespace m4k.AI {
public struct Destroy : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    GameObject go;

    public Destroy(GameObject go, int priority = -1, StateProcessor processor = null) {
        this.go = go;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        if(go)
            MonoBehaviour.Destroy(go);
        else if(processor.currentTarget)
            MonoBehaviour.Destroy(processor.currentTarget.gameObject);
        else
            Debug.LogWarning("Destroyed nothing");
    }

    public bool OnUpdate() => true;
    public void OnExit() {}
}

[System.Serializable]
public class DestroyWrapper : StateWrapper {
    public GameObject go;
    
    public override IState GetState() {
        return new Destroy(go, priority);
    }
}
}
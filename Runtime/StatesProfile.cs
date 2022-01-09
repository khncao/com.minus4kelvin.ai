using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {

[CreateAssetMenu(fileName = "StatesProfile", menuName = "Data/AI/States Profile", order = 0)]
public class StatesProfile : ScriptableObject {
//     [SerializeReference]
// #if SERIALIZE_REFS
//     [SubclassSelector]
// #endif
//     public List<StateWrapper> states;

//     [SerializeReference]
// #if SERIALIZE_REFS
//     [SubclassSelector]
// #endif
//     public StateWrapper defaultState;


    [InspectInline]
    public List<StateWrapperBase> stateWrapperBases;
    [InspectInline]
    public StateWrapperBase defaultStateWrapperBase;


    public IState GetState(IState current) {
        int priorityThreshold = current != null ? current.priority : -1;
        
        // for(int i = 0; i < states.Count; ++i) {
        //     if(!states[i].conditions.CheckCompleteReqs())
        //         continue;

        //     if(states[i].priority > priorityThreshold)
        //         return states[i].GetState();
        // }
        // return defaultState.GetState();

        for(int i = 0; i < stateWrapperBases.Count; ++i) {
            if(!stateWrapperBases[i].conditions.CheckCompleteReqs())
                continue;

            if(stateWrapperBases[i].priority > priorityThreshold)
                return stateWrapperBases[i].GetState();
        }
        return defaultStateWrapperBase.GetState();
    }
}

}
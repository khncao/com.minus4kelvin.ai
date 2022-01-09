using UnityEngine;
using m4k.Characters;

namespace m4k.AI {
public class CharacterDetection : IState {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    
    IState _detectingState;
    IState _gotoState;
    DetectRadiusAngle<CharacterControl> _detector;
    float _lastCheckTime;

    public CharacterDetection(IState detectingState, IState gotoState, Transform self, string characterTag, float maxSquaredRange, float viewAngles = 0f, int priority = -1, StateProcessor processor = null) {
        this._detectingState = detectingState;
        this._gotoState = gotoState;
        this.priority = priority;
        this.processor = processor;
        this._lastCheckTime = 0f;

        var characters = CharacterManager.I.GetCharacterControls(characterTag);
        this._detector = new DetectRadiusAngle<CharacterControl>(self, characters, maxSquaredRange, viewAngles);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _detectingState.OnEnter(processor);

        if(_detector.self == null)
            _detector.self = processor.transform;
    }

    public bool OnUpdate() {
        if((Time.time - _lastCheckTime) > processor.detectionCooldown && 
        _detector.UpdateHits()) 
        {
            _lastCheckTime = Time.time;
            processor.SetTarget(_detector.GetCachedClosest().transform);
            return processor.TryChangeState(_gotoState);
        }
        return _detectingState.OnUpdate();
    }

    public void OnExit() {
        _detectingState.OnExit();
    }
}

[System.Serializable]
public class CharacterDetectionWrapper : StateWrapper {
    public string characterTag;
    public float maxSquaredRange;
    public float viewAngles;

    [Tooltip("Basis to perform detection. Will use processor transform if null")]
    public Transform self;
    
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public StateWrapper detectingState, gotoState;


    public override IState GetState() {
        return new CharacterDetection(detectingState.GetState(), gotoState.GetState(), self, characterTag, maxSquaredRange, viewAngles, priority);
    }
}

[CreateAssetMenu(fileName = "CharacterDetectionState", menuName = "Data/AI/States/CharacterDetectionState", order = 0)]
public class CharacterDetectionState : StateWrapperBase {
    public string characterTag;
    public float maxSquaredRange;
    public float viewAngles;

    [Tooltip("Basis to perform detection. Will use processor transform if null")]
    public Transform self;
    
    [InspectInline(canCreateSubasset = true)]
    public StateWrapperBase detectingState, gotoState;

    public override IState GetState() {
        return new CharacterDetection(detectingState.GetState(), gotoState.GetState(), self, characterTag, maxSquaredRange, viewAngles, priority);
    }
}
}
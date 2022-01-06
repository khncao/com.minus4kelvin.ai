using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using m4k.Items;
using m4k.Characters;

namespace m4k.AI {
/// <summary>
/// AI agent that contains and manages the primary state machine. Passed into IState classes for access to character related actions such as navigation, inventory, and animation. Derive from, or directly modify to add state actions.
/// </summary>
public class StateProcessor : MonoBehaviour
{
    public static List<StateProcessor> stateProcessors { get; } = new List<StateProcessor>();
    public static System.Action<StateProcessor> onAddProcessor, onRemoveProcessor;

    public StatesProfile statesProfile;
    public ProcessorProximityTrigger proximityTrigger;
    public INavMovable movable;
    public ItemArranger itemArranger;
    public Animator anim;
    public CharacterControl cc;
    public bool conscriptable = true;
    public float detectionCooldown = 1f;

    public System.Action onStateComplete, onArrive;

    public bool HasState { get { return currentState != null; } }
    public IState currentState { get; private set; }
    public Transform currentTarget { get; private set; } // can be set and used by special states; for carrying detected closest target from state to state

    protected float lastAbortTime;
    protected bool abortingTask;

    protected PlayableGraph playableGraph;
    protected StateMachine stateMachine;

    // protected DetectRadiusAngle<IStateInteractable> stateInteractableDetector;
    // protected DetectRadiusAngle<CharacterControl> characterControlDetector;

    protected virtual void Start() {
        movable = GetComponentInChildren<INavMovable>();
        if(movable == null)
            movable = GetComponentInParent<INavMovable>();
        if(movable == null) {
            Debug.LogError($"No movable interface found on {gameObject}");
            return;
        }
        if(!anim) anim = GetComponent<Animator>();

        if(!proximityTrigger) {
            Debug.LogError($"{gameObject} needs taskInteractObj TaskInteractTrigger on navigation target object");
            return;
        }
        proximityTrigger.processor = this;
        proximityTrigger.processorCol = GetComponentInChildren<Collider>();
        ToggleProximityTrigger(false);

        stateMachine = new StateMachine(this);
        stateMachine.OnStateComplete += OnStateComplete;

        // stateInteractableDetector = new DetectRadiusAngle<IStateInteractable>(transform, StateInteractableManager.I.stateInteractables.instances, 20f, 90f);
        // characterControlDetector = new DetectRadiusAngle<CharacterControl>(transform, CharacterManager.I.activeCharacterControls, 10f, 0f);

        TryChangeState(statesProfile.GetState(currentState), true);
    }

    public virtual void OnEnable() {
        if(!conscriptable) return;
        stateProcessors.Add(this);
        onAddProcessor?.Invoke(this);
    }

    public virtual void OnDisable() {
        if(!conscriptable) return;
        stateProcessors.Remove(this);
        onRemoveProcessor?.Invoke(this);
    }

    public virtual void Update() {
        stateMachine.OnUpdate();
    }

    public IState GetState() {
        return statesProfile.GetState(currentState);
    }

    public virtual bool TryChangeState(IState state, bool forceChange = false) {
        if(stateMachine.CanChangeState(state) || forceChange) {
            stateMachine.ChangeState(state);
            // Debug.Log(state);
            currentState = state;
            return true;
        }
        Debug.Log($"Failed to change state: {state}");
        return false;
    }

    protected virtual void OnStateComplete() {
        CleanupState();
        onStateComplete?.Invoke();
        TryChangeState(statesProfile.GetState(currentState));
    }

    public virtual void AbortState() {
        lastAbortTime = Time.time;
        abortingTask = true;

        CleanupState();
    }

    protected virtual void CleanupState() {
        ToggleProximityTrigger(false);
        itemArranger?.gameObject.SetActive(false);
        currentState.OnExit();
        currentState = null;
    }

    public virtual void OnArrive() {
        onArrive?.Invoke();
        ToggleProximityTrigger(false);
        movable.Stop();
    }

    public virtual void SetTarget(Transform target) {
        currentTarget = target;
    }

    public virtual void ShowItems(List<ItemInstance> items) {
        if(itemArranger) {
            itemArranger.gameObject.SetActive(true);
            itemArranger.UpdateItems(items);
        }
    }

    public virtual void HideItems(List<ItemInstance> items) {
        if(itemArranger) {
            itemArranger.HideItems();
            itemArranger.gameObject.SetActive(false);
        }
    }

    public virtual void PlayAnimation(AnimationClip clip) {
        AnimationPlayableUtilities.PlayClip(anim, clip, out playableGraph);
    }

    AnimatorStateInfo prevAnimStateInfo0, currAnimStateInfo0;
    public virtual bool CheckAnimStateChanged() {
        prevAnimStateInfo0 = currAnimStateInfo0;
        currAnimStateInfo0 = anim.GetCurrentAnimatorStateInfo(0);
        if(prevAnimStateInfo0.shortNameHash != currAnimStateInfo0.shortNameHash) {
            return true;
        }
        return false;
    }

    public virtual void DestroyObject(GameObject go) {
        Destroy(go);
    }

    public virtual void ToggleProximityTrigger(bool b) {
        proximityTrigger?.gameObject.SetActive(b);
    }


}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
[RequireComponent(typeof(InventoryComponent))]
public class StateInteractableInventory : MonoBehaviour, IStateInteractable
{
    public bool isQuantitive;
    [Tooltip("Temp test items")]
    public List<ItemInstance> items;

    Inventory _inventory;

    private void Start() {
        _inventory = GetComponent<InventoryComponent>().inventory;

        // foreach(var i in items)
        //     _inventory.AddItemAmount(i.item, i.amount);

        OnEnable();
    }

    private void OnEnable() {
        if(!StateInteractableManager.I) return;
        StateInteractableManager.I.inventoryInteractables?.RegisterInstance(this);
        StateInteractableManager.I.stateInteractables.RegisterInstance(this);
    }
    private void OnDisable() {
        if(!StateInteractableManager.I) return;
        StateInteractableManager.I.inventoryInteractables.UnregisterInstance(this);
        StateInteractableManager.I.stateInteractables.UnregisterInstance(this);
    }
    
    public void OnStateInteract(IState state) {
        if(!isQuantitive) {
            return;
        }
        switch(state) {
            case GetItems: {
                // transfer _inventory items to state.processor
                var transfer = ((GetItems)state).items;
                foreach(var i in transfer) {
                    _inventory.RemoveItemAmount(i.item, i.amount);
                }
                break;
            }
            case PutItems: {
                // from state.processor to _inventory
                var transfer = ((PutItems)state).items;
                foreach(var i in transfer) {
                    _inventory.AddItemAmount(i.item, i.amount);
                }
                break;
            }
        }
    }

    public bool CanStateInteract(IState state) {
        if(!isQuantitive) {
            return true;
        }
        switch(state) {
            case GetItems: {
                var transfer = ((GetItems)state).items;
                foreach(var i in transfer) {
                    if(!items.Exists(x=>x.item == i.item))
                        return false;
                }
                break;
            }
            case PutItems: {
                var transfer = ((PutItems)state).items;
                foreach(var i in transfer) {
                    return _inventory.GetMaxAmountItemsFit(i.item) < i.amount;
                }
                break;
            }
        }
        return false;
    }

    public bool HasItems(List<ItemInstance> items) {
        for(int i = 0; i < items.Count; ++i) {
            if(!this.items.Exists(x=>x.item == items[i].item))
                return false;
        }
        return true;
    }
}
}
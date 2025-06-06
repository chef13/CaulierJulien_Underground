using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class StateNeedFood : CreatureState
{

    public StateNeedFood(CreatureAI creature) : base(creature)
    {

    }

    public override void Enter()
    {
        Controller.basicNeed = true;
        CheckForFoodHQ();

    }

    public override void Exit()
    {
        Controller.basicNeed = false;
    }

    public override void Update()
    {
        if (Controller.currentHungerState == CreatureController.hungerState.Full || Controller.currentHungerState == CreatureController.hungerState.Normal)
        {
            if (creature.previousState is not StateNeedRest)
            {
                creature.SwitchState(creature.previousState);
                return;
            }
            else
            {
                creature.SwitchState(new StateIdle(creature));
                return;
            }
        }

        if (!Controller.hasDestination && Controller.currentFaction != null && Controller.currentFaction.currentHQ.Count > 0)
        {
            CheckForFoodHQ();
        }
        else if (!Controller.hasDestination && !Controller.data.carnivor)
        {

            //TryEatVegetable();
            //if (Controller.currentHungerState != CreatureController.hungerState.Full)
            SearchForVegetable();
        }
        else if (!Controller.hasDestination && Controller.data.carnivor)
        {
            HuntPrey();
        }
    }

    private void CheckForFoodHQ()
    {
        if (Controller.currentFaction.foodResources > 0)
        {
            RoomInfo closerHQ = GetCloserHQ();
            TileInfo destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
            Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
        }
    }

    private void HuntPrey()
    {
        CreatureController prey = null;
        prey = CheckForRoomWith(prey, 0);
        if (prey == null)
        {
            prey = CheckForRoomWith(prey, 1);
        }
        if (prey == null)
        {
            prey = CheckForRoomWith(prey, 2);
        }
        if (prey == null)
        {
            //Debug.Log("No prey found, switching to explore state.");
            creature.SwitchState(new StateExplore(creature));
            return;
        }
        Controller.SetDestination(new Vector2(prey.transform.position.x, prey.transform.position.y));
    }

    public void SearchForVegetable()
    {
        FlaureBehaviour prey = null;
        prey = CheckForRoomWith(prey, 0);
        if (prey == null)
        {
            prey = CheckForRoomWith(prey, 1);
        }
        if (prey == null)
        {
            prey = CheckForRoomWith(prey, 2);
        }
        if (prey == null)
        {
            //Debug.Log("No prey found, switching to explore state.");
            creature.SwitchState(new StateExplore(creature));
            return;
        }
        Controller.SetDestination(new Vector2(prey.transform.position.x, prey.transform.position.y));
    }
    

    private void TryEatVegetable()
{
    var tile = Controller.currentTile;
    if (tile == null) return;
    foreach (var obj in tile.objects)
    {
        var flaure = obj != null ? obj.GetComponent<FlaureBehaviour>() : null;
        if (flaure != null && flaure.isEdible && Controller.data.herbivor)
        {
            Controller.StartCoroutine(Controller.GoEatTarget(obj));
            break;
        }
    }
}
}

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
        
       // Controller.basicNeed = true;
       // CheckForFoodHQ();

    }

    public override void Exit()
    {
       // Controller.basicNeed = false;
    }

    public override void Update()
    {
        if (Controller.currentHungerState == CreatureController.hungerState.Full)
        {
            creature.SwitchState(new StateIdle(creature));
            return;

        }

        /*if (!Controller.hasDestination && Controller.currentHungerState == CreatureController.hungerState.Starving && Controller.currentFaction != null && Controller.currentFaction.currentHQ.Count > 0)
        {
            CheckForFoodHQ();
        }*/
        if (Controller.hasDestination || Controller.currentFoodTarget != null)
         return;

        if (!Controller.hasDestination)
        {
            RoomInfo Room = null;
            if (Controller.currentRoom == null)
            {
                Room = Controller.currentTile.corridor.connectedRooms[Random.Range(0, Controller.currentTile.corridor.connectedRooms.Count)];
            }
            else
            {
                Room = Controller.currentRoom;
            }

            if (Controller.data.carnivor && Controller.currentFoodTarget == null)
            {
                //HuntPrey();
                if (Controller.currentRoom == null)
                {
                    creature.SwitchState(new StateExplore(creature));
                    return;
                }

                RoomInfo enemyRoom = FindRoom(
                                    Room,
                                    2,
                                    room => room.tiles.Any(t => t.creatures.Any(c => c.currentFaction != Controller.currentFaction))
                                );
                if (enemyRoom != null)
                {
                    Debug.Log($"{Controller.name} found prey in room {enemyRoom.index}");
                    Controller.SetDestination(enemyRoom.tileCenter);
                }
            }
            if (Controller.data.herbivor && Controller.currentFoodTarget == null)
            {
                RoomInfo foodRoom = FindRoom(
                                Room,
                                3,
                                room => room.tiles.Any(t => t.objects.Any(o => o.GetComponent<FlaureBehaviour>()?.isEdible == true))
                            );
                if (foodRoom != null)
                {
                    Debug.Log($"{Controller.name} found food in room {foodRoom.index}");
                    Controller.SetDestination(foodRoom.tileCenter);
                }
                //TryEatVegetable();
                //if (Controller.currentHungerState != CreatureController.hungerState.Full)
                //SearchForVegetable();
            }
            if (!Controller.hasDestination)
            {
                Debug.Log($"{Controller.name} has no destination in needfood, finding a random room");
                RoomInfo randomRoom = FindRoom(Room, 2);
                Controller.SetDestination(randomRoom.tileCenter);
            }
        }
        
    }

    private void CheckForFoodHQ()
    {
            RoomInfo closerHQ = GetCloserHQ();
            var pos  = closerHQ.tileCenter;
            Controller.SetDestination(pos);
        
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

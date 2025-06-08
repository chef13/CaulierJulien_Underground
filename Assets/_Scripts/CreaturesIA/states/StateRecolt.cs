using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

public class StateRecolt : CreatureState
{

    private Coroutine recoltCoroutine;
    
    public StateRecolt(CreatureAI creature) : base(creature)
    {
    }

    public override void Enter()
    {
    }
    
    public override void Exit()
    {
        if (Controller != null && recoltCoroutine != null)
        {
            Controller.StopCoroutine(recoltCoroutine);
            recoltCoroutine = null;
        }
        if (Controller != null)
        {
            Controller.recoltTarget = false;
            Controller.currentRecoltTarget = null;
        }
    }

    public override void Update()
    {




        //Debug.Log($"{Controller.name} is in Recolt state");
        if (Controller.currentResources >= Controller.data.maxEnergy / 2)
        {
            Controller.lookingForRessource = false;
        }
        else
        {
            Controller.lookingForRessource = true;
        }

        if (!Controller.hasDestination && !Controller.lookingForRessource)
        {
            var closerHQ = GetCloserHQ();
            TileInfo destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
            Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
        }

        /*if (Controller.lookingForRessource && Controller.currentFoodTarget == null && Controller.currentRecoltTarget == null && recoltCoroutine == null)
        {
            //Debug.Log($"{Controller.name} looking for resources");
            if (Controller.data.carnivor)
            {
                CheckCarnivor();
            }
            if (Controller.data.herbivor && !Controller.recoltTarget)
            {
                CheckHerbivor();
                //Debug.Log($"{Controller.name} checking for herbivor targets");
            }
        }*/
        /*if (!Controller.hasDestination && Controller.currentFoodTarget == null && Controller.currentRecoltTarget == null)
        {
            //Debug.Log($"{Controller.name} looking for resources");
            if (Controller.data.carnivor)
            {
                CheckCarnivor();
            }
            if (Controller.data.herbivor && !Controller.recoltTarget)
            {
                CheckHerbivor();
                //Debug.Log($"{Controller.name} checking for herbivor targets");
            }
        }*/

        if (Controller.hasDestination || Controller.currentFoodTarget != null || Controller.currentRecoltTarget != null)
        return;


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
                                    1,
                                    room => room.tiles.Any(t => t.creatures.Any(c => c.currentFaction != Controller.currentFaction))
                                );


                if (enemyRoom != null)
                {
                   // Debug.Log($"{Controller.name} found enemy room at {enemyRoom.index}");
                    Controller.SetDestination(enemyRoom.tileCenter);
                }
            }
            if (Controller.data.herbivor && Controller.currentFoodTarget == null)
            {
                RoomInfo foodRoom = FindRoom(
                                Room,
                                2,
                                room => room.tiles.Any(t => t.objects.Any(o => o.GetComponent<FlaureBehaviour>()?.isEdible == true))
                            );
                if (foodRoom != null)
                {
                    //Debug.Log($"{Controller.name} found food room at {foodRoom.index}");
                    Controller.SetDestination(foodRoom.tileCenter);
                }
                //TryEatVegetable();
                //if (Controller.currentHungerState != CreatureController.hungerState.Full)
                //SearchForVegetable();
            }
            if (!Controller.hasDestination)
            {
                //Debug.Log($"{Controller.name} has no destination, finding a random room");
                RoomInfo randomRoom = FindRoom(Room, 2);
                Controller.SetDestination(randomRoom.tileCenter);
            }
        }
    }

    public IEnumerator GoRecoltTarget(GameObject target)
    {
        if (target == null) yield break;

        Vector2 previousDestination = Controller.destination;
        Controller.tempDestination = target.transform.position;
        Controller.SetDestination(Controller.tempDestination);

        while (target != null && target.activeInHierarchy && Controller.recoltTarget)
        {
            // Revalidate target
            if (!Controller.CheckOntargetValidity(target))
                break;

            float distance = Vector2.Distance(Controller.transform.position, Controller.tempDestination);
            Controller.tempDestination = target.transform.position;

            // If close enough, interact
            if (!Controller.hasDestination || distance <= 1.5f)
            {
                Controller.agent.isStopped = true;
                Controller.animator.SetTrigger("Attack");
                yield return new WaitForSeconds(1f);
                Controller.agent.isStopped = false;

                if (target.TryGetComponent(out FlaureBehaviour flaure))
                {
                    flaure.IsEaten();
                    Controller.currentResources += flaure.flaureData.edibleAmount;
                }
                else if (target.TryGetComponent(out CreatureController corpse))
                {
                    corpse.currentCreatureType.IsEaten();
                    Controller.currentResources += corpse.data.maxLife / 2;
                }

                break;
            }

            // Maintain destination if needed
            if (!Controller.hasDestination)
                Controller.SetDestination(Controller.tempDestination);

            yield return new WaitForSeconds(0.1f);
        }

        // Cleanup
        Controller.SetDestination(previousDestination);
        Controller.currentRecoltTarget = null;
        Controller.recoltTarget = false;
        recoltCoroutine = null;
    }
    /*public IEnumerator Recolt()
    {
        
    }*/

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
            // prey = CheckForRoomWith(prey, 2);
        }
        if (prey == null)
        {
            RoomInfo randomRoom = null;
            if (Controller.currentRoom != null)
            {
                randomRoom = Controller.currentRoom.connectedRooms[Random.Range(0, Controller.currentRoom.connectedRooms.Count)];
            }
            else
            {
                randomRoom = Controller.currentTile.corridor.connectedRooms[Random.Range(0, Controller.currentTile.corridor.connectedRooms.Count)];
            }
            TileInfo randomTile = randomRoom.tiles[Random.Range(0, randomRoom.tiles.Count)];
            Controller.SetDestination(new Vector2(randomTile.position.x, randomTile.position.y));
            return;
        }
        else
        {
            Controller.SetDestination(new Vector2(prey.transform.position.x, prey.transform.position.y));
        }
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
            //prey = CheckForRoomWith(prey, 2);
        }
        if (prey == null)
        {
            RoomInfo randomRoom = null;
            if (Controller.currentRoom != null)
            {
                randomRoom = Controller.currentRoom.connectedRooms[Random.Range(0, Controller.currentRoom.connectedRooms.Count)];
            }
            else 
            {
                randomRoom = Controller.currentTile.corridor.connectedRooms[Random.Range(0, Controller.currentTile.corridor.connectedRooms.Count)];
            }
            TileInfo randomTile = randomRoom.tiles[Random.Range(0, randomRoom.tiles.Count)];
            Controller.SetDestination(new Vector2(randomTile.position.x, randomTile.position.y));
            return;
        }
        Controller.SetDestination(new Vector2(prey.transform.position.x, prey.transform.position.y));
    }
    

    public override void CheckHerbivor()
    {
        //Debug.Log($"{name}"  + " checking for herbivor targets");
            for (int s = 0; s < Controller._surroundingTiles.Count; s++)
            {
                var tile = Controller._surroundingTiles[s];
                if (tile.objects != null && tile.objects.Count > 0)
                {
                for (int f = 0; f < tile.objects.Count; f++)
                {
                    var flaureComp = tile.objects[f].GetComponent<FlaureBehaviour>();
                    if (flaureComp != null && flaureComp.gameObject.activeInHierarchy && flaureComp.isEdible)
                    {
                        Controller.currentRecoltTarget = tile.objects[f];
                        Controller.recoltTarget = true;
                        // Debug.Log($"{name} found edible Flaure {flaureComp.name} at {tile.position}");
                        recoltCoroutine = Controller.StartCoroutine(GoRecoltTarget(Controller.currentRecoltTarget));
                        return;
                    }
                }
                }
            }
    }
    public override void CheckCarnivor()
    {
        //Debug.Log($"{name}"  + " checking for carnivor targets");
        for (int i = 0; i < Controller.CreaturesInRange.Count; i++)
        {

            var creature = Controller.CreaturesInRange[i];
            if (creature != null && creature.gameObject.activeInHierarchy && !creature.isDead && creature.currentFaction != Controller.currentFaction)
            {
                //Debug.Log($"Found creature to prey {creature.name} at {creature.currentTile.position}");
                
                Controller.creatureAI.SwitchState(new StateAttack(Controller.creatureAI, creature));
                return;
            }
            
            //Debug.Log($"No creature found in range.");        
            if (!Controller.recoltTarget && creature.gameObject.activeInHierarchy && creature != null && creature.isDead && creature.isCorpse)
            {
                
                    Controller.recoltTarget = true;
                    Controller.currentRecoltTarget = creature.gameObject;
                    recoltCoroutine = Controller.StartCoroutine(GoRecoltTarget(Controller.currentRecoltTarget));
                    return;
                
            }
        }
    }
}

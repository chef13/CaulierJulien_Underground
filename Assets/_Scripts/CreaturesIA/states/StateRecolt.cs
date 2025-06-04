using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateRecolt : CreatureState
{


    
    public StateRecolt(CreatureAI creature) : base(creature)
    {
    }

    public override void Enter()
    {
    }

    public override void Update()
    {

        if (DetectTreat(out CreatureController target))
                {
                    creature.SwitchState(new StateAttack(creature, target));
                }

            
        Debug.Log($"{Controller.name} is in Recolt state");
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

        if (Controller.lookingForRessource && !Controller.recoltTarget && !Controller.foodTarget)
        {
            Debug.Log($"{Controller.name} looking for resources");
            if (Controller.data.carnivor)
            {
                CheckCarnivor();
                
                Debug.Log($"{Controller.name} checking for carnivor targets");
            }
            if (Controller.data.herbivor && !Controller.recoltTarget)
            {
                CheckHerbivor();
                Debug.Log($"{Controller.name} checking for herbivor targets");
            }
        }

        if (!Controller.hasDestination && Controller.lookingForRessource)
        {
            if (Controller.data.carnivor)
            {
                HuntPrey();
            }
            if (!Controller.hasDestination && Controller.data.herbivor)
            {
                SearchForVegetable();
            }

            
        }
    }

    public IEnumerator GoRecoltTarget(GameObject target)
    {
        while (Controller.recoltTarget)
        {
            //Debug.Log($"{name} GoEatTarget coroutine started for {target?.name}");
            Vector2 previousDestination = Controller.destination;
            Controller.tempDestination = target.transform.position;
            Controller.SetDestination(Controller.tempDestination);
            //Debug.Log($"{name} is going to eat {target.name} at {tempDestination} from {Vector2.Distance(transform.position, tempDestination)}");
            if (target == null || !target.activeInHierarchy)
            {
                target = null;
                Controller.recoltTarget = false;

                //Debug.LogWarning($"{name} tried to eat a target that is null or inactive.");
                yield break;
            }
            yield return new WaitUntil(() => !Controller.hasDestination || Vector2.Distance(Controller.transform.position, Controller.tempDestination) <= 1.5f);

            if (target.GetComponent<FlaureBehaviour>() != null)
            {
                FlaureBehaviour flaure = target.GetComponent<FlaureBehaviour>();
                Controller.animator.SetTrigger("Attack");
                Controller.agent.isStopped = true; // Stop moving while attacking

                yield return new WaitForSeconds(Controller.animator.GetCurrentAnimatorStateInfo(0).length);
                Controller.agent.isStopped = false; // Resume moving after attack
                flaure.IsEaten();
                Controller.currentResources += flaure.flaureData.edibleAmount;
                Controller.recoltTarget = false;
            }
            if (target.GetComponent<CreatureController>() != null)
            {
                CreatureController creature = target.GetComponent<CreatureController>();
                Controller.animator.SetTrigger("Attack");
                Controller.agent.isStopped = true; // Stop moving while attacking

                yield return new WaitForSeconds(Controller.animator.GetCurrentAnimatorStateInfo(0).length);
                Controller.agent.isStopped = false; // Resume moving after attack
                creature.IsEaten();
                Controller.currentResources += creature.data.maxLife / 2;
                Controller.recoltTarget = false;
            }
            Controller.tempDestination = Vector2.zero;
            Controller.SetDestination(previousDestination);
        }
        yield break;
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
    

    private void CheckHerbivor()
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
                        if (flaureComp != null && flaureComp.isEdible)
                        {
                            Controller.recoltTarget = true;
                           // Debug.Log($"{name} found edible Flaure {flaureComp.name} at {tile.position}");
                            Controller.StartCoroutine(GoRecoltTarget(tile.objects[f]));
                            break;
                        }
                        //else
                           // Debug.Log($"{name} found Flaure {flaureComp.name} at {tile.position}, but it is not edible.");
                    }
                }
            }
    }
    private void CheckCarnivor()
    {
        //Debug.Log($"{name}"  + " checking for carnivor targets");
        for (int i = 0; i < Controller.CreaturesInRange.Count; i++)
        {

            var creature = Controller.CreaturesInRange[i];
            if (creature != null && !creature.isDead && creature.currentFaction != Controller.currentFaction)
            {
                Debug.Log($"Found creature to prey {creature.name} at {creature.currentTile.position}");
                Controller.recoltTarget = true;
                this.creature.SwitchState(new StateAttack(this.creature, creature));
                break;
            }
            
            //Debug.Log($"No creature found in range.");        
            if (!Controller.recoltTarget && creature != null && creature.isDead && creature.isCorpse)
            {
                Debug.Log($"Found dead creature {creature.name} at {creature.currentTile.position}");
                Controller.recoltTarget = true;
                Controller.StartCoroutine(GoRecoltTarget(creature.gameObject));
                break;
            }
        }
    }
}

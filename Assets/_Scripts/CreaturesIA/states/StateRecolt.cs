using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateRecolt : CreatureState
{

    bool recoltTarget = false;
    bool lookingForRessource = false;
    public StateRecolt(CreatureAI creature) : base(creature)
    {
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        if (Controller.currentResources >= Controller.data.maxEnergy / 2)
        {
            lookingForRessource = false;
        }
        else
        {
            lookingForRessource = true;
        }

        if (!Controller.hasDestination && !lookingForRessource)
        {
            var closerHQ = GetCloserHQ();
            TileInfo destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
            Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
        }

        if (lookingForRessource && !recoltTarget)
        {
            if (Controller.data.carnivor)
            {
                CheckCarnivor();
            }
            if (Controller.data.herbivor && !recoltTarget)
            {
                CheckHerbivor();
            }
        }

        if (!Controller.hasDestination && lookingForRessource)
        {
            if (Controller.data.carnivor)
            {
                HuntPrey();
            }
            if (Controller.data.herbivor)
            {
                SearchForVegetable();
            }

            
        }
    }

    public IEnumerator GoRecoltTarget(GameObject target)
    {
        while (recoltTarget)
        {
            //Debug.Log($"{name} GoEatTarget coroutine started for {target?.name}");
            Vector2 previousDestination = Controller.destination;
            Controller.tempDestination = target.transform.position;
            Controller.SetDestination(Controller.tempDestination);
            //Debug.Log($"{name} is going to eat {target.name} at {tempDestination} from {Vector2.Distance(transform.position, tempDestination)}");
            if (target == null || !target.activeInHierarchy)
            {
                target = null;
                recoltTarget = false;

                //Debug.LogWarning($"{name} tried to eat a target that is null or inactive.");
                yield break;
            }
            yield return new WaitUntil(() => !Controller.hasDestination || Vector2.Distance(Controller.transform.position, Controller.tempDestination) <= 1f);

            if (target.GetComponent<FlaureBehaviour>() != null)
            {
                FlaureBehaviour flaure = target.GetComponent<FlaureBehaviour>();
                Controller.animator.SetTrigger("Attack");
                Controller.agent.isStopped = true; // Stop moving while attacking

                yield return new WaitForSeconds(Controller.animator.GetCurrentAnimatorStateInfo(0).length);
                Controller.agent.isStopped = false; // Resume moving after attack
                flaure.IsEaten();
                Controller.currentResources += flaure.flaureData.edibleAmount;
                recoltTarget = false;
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
                recoltTarget = false;
            }
            Controller.tempDestination = Vector2.zero;
            Controller.SetDestination(previousDestination);
        }
        yield break;
    }

    private void HuntPrey()
    {
        List<TileInfo> tileWithPrey = new List<TileInfo>();
        if (Controller.currentRoom == null)
        {
            Debug.LogWarning("Current room is null, cannot hunt prey.");
            return;
        }
        tileWithPrey.Add(Controller.currentTile);

        for (int i = 0; i < Controller.currentRoom.tiles.Count; i++)
        {
            TileInfo tile = Controller.currentRoom.tiles[i];
            if (tile.creatures != null && tile.creatures.Count > 0)
            {
                foreach (var obj in tile.creatures)
                {
                    CreatureController prey = obj.GetComponent<CreatureController>();
                    if (prey != null && prey.currentFaction != Controller.currentFaction)
                    {
                        if (prey.isDead && !prey.isCorpse)
                        {
                            return;
                        }
                        else if (!prey.isCorpse && !prey.isDead || prey.isDead && prey.isCorpse)
                            tileWithPrey.Add(tile);

                    }
                }
            }
        }
        if (tileWithPrey.Count == 0)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector2Int roomPosCheck = new Vector2Int(Controller.currentRoom.index.x + i, Controller.currentRoom.index.y + j);
                    if (DungeonGenerator.Instance.roomsMap.TryGetValue(roomPosCheck, out RoomInfo room))
                    {
                        if (Controller.currentFaction.knownRoomsDict.ContainsKey(room.index))
                        {
                            for (int f = 0; f < room.tiles.Count; f++)
                            {
                                TileInfo tile = room.tiles[f];
                                if (tile.objects != null && tile.creatures.Count > 0)
                                {
                                    foreach (var obj in tile.creatures)
                                    {
                                        CreatureController prey = obj.GetComponent<CreatureController>();
                                        if (prey != null && prey.currentFaction != Controller.currentFaction)
                                        {
                                            if (prey.isDead && !prey.isCorpse)
                                            {
                                                return;
                                            }
                                            else if (!prey.isCorpse && !prey.isDead || prey.isDead && prey.isCorpse)
                                                tileWithPrey.Add(tile);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        if (tileWithPrey.Count == 0)
        {
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    Vector2Int roomPosCheck = new Vector2Int(Controller.currentRoom.index.x + i, Controller.currentRoom.index.y + j);
                    if (DungeonGenerator.Instance.roomsMap.TryGetValue(roomPosCheck, out RoomInfo room))
                    {
                        if (Controller.currentFaction.knownRoomsDict.ContainsKey(room.index))
                        {
                            for (int f = 0; f < room.tiles.Count; f++)
                            {
                                TileInfo tile = room.tiles[f];
                                if (tile.objects != null && tile.creatures.Count > 0)
                                {
                                    foreach (var obj in tile.creatures)
                                    {
                                        CreatureController prey = obj.GetComponent<CreatureController>();
                                        if (creature != null && prey.currentFaction != Controller.currentFaction)
                                        {
                                            if (prey.isDead && !prey.isCorpse)
                                            {
                                                return;
                                            }
                                            else if (!prey.isCorpse && !prey.isDead || prey.isDead && prey.isCorpse)
                                                tileWithPrey.Add(tile);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        if (tileWithPrey.Count == 0)
        {
            Debug.Log("No food found, switching to explore state.");
            creature.SwitchState(new StateExplore(creature));
            return;
        }
        float minDistance = 1000;
        TileInfo closestTileWithPrey = null;
        for (int i = 0; i < tileWithPrey.Count; i++)
        {
            float distanceForFood =
                Controller.GetPathDistance(
                    Controller.agent, tileWithPrey[i].position);
            if (distanceForFood < minDistance)
            {
                minDistance = distanceForFood;
                closestTileWithPrey = tileWithPrey[i];
            }
        }

        Controller.SetDestination(new Vector2Int(closestTileWithPrey.position.x, closestTileWithPrey.position.y));
    }

    public void SearchForVegetable()
    {
        List<TileInfo> TileWithFood = new List<TileInfo>(); ;
        for (int i = 0; i < Controller.currentRoom.tiles.Count; i++)
        {
            TileInfo tile = Controller.currentRoom.tiles[i];
            if (tile.objects != null && tile.objects.Count > 0)
            {
                foreach (var obj in tile.objects)
                {
                    FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                    if (flaure != null && flaure.isEdible && Controller.data.herbivor)
                    {
                        TileWithFood.Add(tile);
                    }
                }
            }
        }
        if (TileWithFood.Count == 0)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    Vector2Int roomPosCheck = new Vector2Int(Controller.currentRoom.index.x + i, Controller.currentRoom.index.y + j);
                    if (DungeonGenerator.Instance.roomsMap.TryGetValue(roomPosCheck, out RoomInfo room))
                    {
                        if (Controller.currentFaction.knownRoomsDict.ContainsKey(room.index))
                        {
                            for (int f = 0; f < room.tiles.Count; f++)
                            {
                                TileInfo tile = room.tiles[f];
                                if (tile.objects != null && tile.objects.Count > 0)
                                {
                                    foreach (var obj in tile.objects)
                                    {
                                        FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                                        if (flaure != null && flaure.isEdible && Controller.data.herbivor)
                                        {
                                            TileWithFood.Add(tile);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        if (TileWithFood.Count == 0)
        {
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    Vector2Int roomPosCheck = new Vector2Int(Controller.currentRoom.index.x + i, Controller.currentRoom.index.y + j);
                    if (DungeonGenerator.Instance.roomsMap.TryGetValue(roomPosCheck, out RoomInfo room))
                    {
                        if (Controller.currentFaction.knownRoomsDict.ContainsKey(room.index))
                        {
                            for (int f = 0; f < room.tiles.Count; f++)
                            {
                                TileInfo tile = room.tiles[f];
                                if (tile.objects != null && tile.objects.Count > 0)
                                {
                                    foreach (var obj in tile.objects)
                                    {
                                        FlaureBehaviour flaure = obj.GetComponent<FlaureBehaviour>();
                                        if (flaure != null && flaure.isEdible && Controller.data.herbivor)
                                        {
                                            TileWithFood.Add(tile);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }

        if (TileWithFood.Count == 0)
        {
            Debug.Log("No food found, switching to explore state.");
            creature.SwitchState(new StateExplore(creature));
            return;
        }
        float minDistance = 1000;
        TileInfo closestTile = null;
        for (int i = 0; i < TileWithFood.Count; i++)
        {
            float distanceForFood =
                Controller.GetPathDistance(
                    Controller.agent, TileWithFood[i].position);
            if (distanceForFood < minDistance)
            {
                minDistance = distanceForFood;
                closestTile = TileWithFood[i];
            }
        }

        if (DetectTreat(out CreatureController prey) != null)
        {
            Debug.Log("Found a prey, switching to fight state.");
            creature.SwitchState(new StateAttack(creature, prey));
            return;
        }
        Controller.SetDestination(new Vector2Int(closestTile.position.x, closestTile.position.y));
    }
    

    private void CheckHerbivor()
    {
        //Debug.Log($"{name}"  + " checking for herbivor targets");
            for (int s = 0; s < Controller.surroundingTiles.Count; s++)
            {
                var tile = Controller.surroundingTiles[s];
                if (tile.objects != null && tile.objects.Count > 0)
                {
                    for (int f = 0; f < tile.objects.Count; f++)
                    {
                        var flaureComp = tile.objects[f].GetComponent<FlaureBehaviour>();
                        if (flaureComp != null && flaureComp.isEdible)
                        {
                            recoltTarget = true;
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
                if (creature != null && creature.isDead && creature.isCorpse)
                {
                    //Debug.Log($"Found dead creature {creature.name} at {creature.currentTile.position}");
                    recoltTarget = true;
                    Controller.StartCoroutine(GoRecoltTarget(creature.gameObject));
                    break;
                }

            }
    }
}

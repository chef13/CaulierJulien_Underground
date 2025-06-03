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
                                    if (creature!= null && prey.currentFaction != Controller.currentFaction)
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

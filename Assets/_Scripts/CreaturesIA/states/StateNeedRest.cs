using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class StateNeedRest : CreatureState
{
    public StateNeedRest(CreatureAI creature) : base(creature)
    {

    }
    


    public override void Enter()
    {
        TileInfo destinationTile = null;

        if (Controller.currentFaction != null && Controller.currentFaction.currentHQ.Count > 0)
        {
            RoomInfo closerHQ = GetCloserHQ();
            destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
            Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
        }
        else
        { CheckForADeadEnd(); }
        
        

    }

    public override void Exit()
    {
        //Controller.basicNeed = false;
    }

    public override void Update()
    {
        if (Controller.sleeping) return;

           if (Controller.currentEnergy >= Controller.data.maxEnergy-5)
            {
                Controller.basicNeed = false;
                if (creature.previousState is not StateNeedFood)
                    creature.SwitchState(creature.previousState);
                else
                    creature.SwitchState(new StateIdle(creature));
                return;
            }

        if (Controller.currentEnergyState != CreatureController.energyState.Full )

            if (!Controller.hasDestination && Controller.currentRoom.faction == Controller.currentFaction && !Controller.sleeping)
            {
                //bool foundFactionTileAround = false;
                for (int v = -1; v < 2; v++)
                {
                    if (DungeonGenerator.Instance.dungeonMap.TryGetValue(Controller.currentTile.position + new Vector3Int(0, v,0), out TileInfo tile))
                    {
                        if (tile.room.faction == Controller.currentFaction )
                        {
                            //foundFactionTileAround = true;
                            Controller.Rest();
                            break;
                        }
                    }
                }
                for (int h = -1; h < 2; h++)
                {
                    if (DungeonGenerator.Instance.dungeonMap.TryGetValue(Controller.currentTile.position + new Vector3Int(h, 0,0), out TileInfo tile))
                    {
                        if (tile.room.faction == Controller.currentFaction )
                        {
                            //foundFactionTileAround = true;
                            Controller.Rest();
                            break;
                        }
                    }
                }

               /*if (!foundFactionTileAround)
                {
                    Debug.Log("Creature " + Controller.name + " found no faction tile around at " + Controller.currentTile.position);
                    CheckForADeadEnd();
                }
                else
                Controller.Rest();*/
            }
            else if (!Controller.hasDestination && !Controller.sleeping)
            {
                // bool foundDeadTileAround = false;
                for (int v = -1; v < 2; v++)
                {
                    if (DungeonGenerator.Instance.dungeonMap.TryGetValue(Controller.currentTile.position + new Vector3Int(0, v,0), out TileInfo tile))
                    {
                        if (tile.isDeadEnd )
                        {
                            //foundDeadTileAround = true;
                            Controller.Rest();
                            break;
                        }
                    }
                }
                for (int h = -1; h < 2; h++)
                {
                    if (DungeonGenerator.Instance.dungeonMap.TryGetValue(Controller.currentTile.position + new Vector3Int(h, 0,0), out TileInfo tile))
                    {
                        if (tile.isDeadEnd)
                        {
                            //foundDeadTileAround = true;
                            Controller.Rest();
                            break;
                        }
                    }
                }
                Debug.Log("Creature " + Controller.name + " found a dead end at " + Controller.currentTile.position);
                //Controller.Rest();
            }
            else if (Controller.hasDestination && Controller.currentEnergyState == CreatureController.energyState.Exhausted)
            {
                CheckForADeadEnd();
            }

            else if (Controller.currentEnergy <= 0 && !Controller.sleeping)
            {
                Controller.Rest();
            }

    }


    private void CheckForADeadEnd()
    {
        if (Controller.currentTile.corridor != null)
        {
            foreach (RoomInfo room in Controller.currentTile.corridor.connectedRooms)
            {
                for (int j = 0; j < room.tiles.Count; j++)
                {
                    TileInfo tile = room.tiles[j];
                    if (tile.isDeadEnd)
                    {
                        Controller.SetDestination(new Vector2(tile.position.x, tile.position.y));
                        return;
                    }
                }
            }
        }

        if (Controller.currentRoom != null)
            {
                for (int i = 0; i < Controller.currentRoom.tiles.Count; i++)
                {
                    TileInfo tile = Controller.currentRoom.tiles[i];
                    if (tile.isDeadEnd)
                    {
                        Controller.SetDestination(new Vector2(tile.position.x, tile.position.y));
                        return;
                    }
                }
            }

        

    }
}

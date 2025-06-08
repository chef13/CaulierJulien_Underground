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

        if (Controller.currentEnergy >= Controller.data.maxEnergy - 5 || Controller.currentHP >= Controller.data.maxLife - 5)
        {
            
                creature.SwitchState(new StateIdle(creature));
            return;
        }

        if (Controller.currentEnergyState != CreatureController.energyState.Full || Controller.currentHP < Controller.data.maxLife -5)
        {
            if (CheckIfInHQ())
            {
                Controller.Rest();
                return;
            }
            
            if (!Controller.hasDestination && Controller.currentRoom != null && Controller.currentFaction.currentHQ.Count > 0 && Controller.currentRoom.faction != Controller.currentFaction)
            {
                RoomInfo closerHQ = GetCloserHQ();
                if (closerHQ != null)
                {
                    TileInfo destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
                    Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
                }
                return;
            }

            if (Controller.hasDestination && Controller.currentEnergyState == CreatureController.energyState.Exhausted)
            {
                CheckForADeadEnd();
            }

            if (Controller.currentEnergy <= 0 && !Controller.sleeping)
            {
                Controller.Rest();
            }
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

    private bool CheckIfInHQ()
    {
        if (Controller.currentRoom != null && Controller.currentRoom.faction == Controller.currentFaction)
        {
            return true;
        }
        return false;
    }
}

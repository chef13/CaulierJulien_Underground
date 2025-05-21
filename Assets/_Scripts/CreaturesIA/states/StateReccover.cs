using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class StateReccover : CreatureState
{

    RoomInfo factionHQ;
    public StateReccover(CreatureAI creature) : base(creature)
    {

    }

    public override void Enter()
    {
        RestCheck();


        if (creature.controller.currentEnergy >= creature.controller.maxEnergy)
        {
            creature.controller.currentEnergy = creature.controller.maxEnergy;
            creature.controller.currentFaction.AskedForState(creature.controller.transform);
        }
    }

    public override void Update()
    {

    }

    private void Rest()
    {
        creature.controller.currentEnergy += creature.controller.maxEnergy * 0.1f * Time.deltaTime;
        if (creature.controller.currentEnergy >= creature.controller.maxEnergy)
        {
            creature.controller.currentEnergy = creature.controller.maxEnergy;

        }
    }

    private void RestCheck()
    {
        RoomInfo factionHQ = creature.controller.currentFaction.currentHQ;

        if (factionHQ == null)
            return;

        // Convert creature world position to grid cell position
        Vector3Int creaturePos = Vector3Int.FloorToInt(creature.controller.transform.position);

        // Check if creature is inside one of the HQ tiles
        bool isInHQ = factionHQ.tiles.Exists(tile => tile.position == creaturePos);

        if (isInHQ)
        {
            Rest();
        }
        else
        {
            // Get a random valid world position inside the HQ room
            TileInfo targetTile = factionHQ.tiles[Random.Range(0, factionHQ.tiles.Count)];
            Vector3 worldPos = new Vector3(targetTile.position.x + 0.5f, targetTile.position.y + 0.5f);

            creature.controller.SetDestination(worldPos);
        }
    }
}

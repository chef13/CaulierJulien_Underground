using UnityEngine;

public class StateIdle : CreatureState
{

    public StateIdle(CreatureAI creature) : base(creature)
    {
    }

    public override void Enter()
    {
        if (Controller.currentFaction.currentHQ.Count > 0)
        {
            RoomInfo closerHQ = GetCloserHQ();
            TileInfo destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
            Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
        }
        else
        {
            creature.SwitchState(new StateExplore(creature));
        }
    }

    public override void Update()
    {
        if (!Controller.hasDestination)
        {
            if (Controller.currentFaction.currentHQ.Count > 0)
            {
                RoomInfo closerHQ = GetCloserHQ();
                TileInfo destinationTile = closerHQ.tiles[Random.Range(0, closerHQ.tiles.Count)];
                Controller.SetDestination(new Vector2Int(destinationTile.position.x, destinationTile.position.y));
            }
            else
            {
                creature.SwitchState(new StateExplore(creature));
            }
        }
    }
}

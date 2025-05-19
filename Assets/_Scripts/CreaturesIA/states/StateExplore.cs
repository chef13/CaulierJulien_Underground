using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEditor.Rendering;

public class StateExplore : CreatureState
{
    
    public NavMeshAgent agent;
    private float exploreRange = 5f;
    private Vector2Int cardinalExplo;
    public StateExplore(CreatureAI creature) : base(creature) { }

    /*public override void Enter()
    {
        
        //creature.controller.CheckCurrentRoom();
        agent = creature.GetComponent<NavMeshAgent>();
        cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0, Direction2D.cardinalDirectionsList.Count)];
        SetNewDestination();
    }

    public override void Update()
    {
        //RegisterNewTiles();

        if (!creature.controller.hasDestination)
        {
            if (creature.controller.currentFaction.knowRooms.Contains(creature.controller.currentRoom) || creature.controller.currentRoom == null)
            SetNewDestination();
            else if (creature.controller.currentRoom != null && !creature.controller.currentFaction.knowRooms.Contains(creature.controller.currentRoom))
            //ExploreCurrentRoom(creature.controller.currentRoom);
        }

        // Exemple de transition vers un autre état
        if (DetectEnemy(out GameObject target))
        {
            Debug.Log("Enemy detected, switching to StateAttack!");
            creature.SwitchState(new StateAttack(creature, target));
        }

        // Check if the surrounding tiles in detection range are known, if not add them to knownTiles
        



    }

    public override void SetNewDestination()
    {
        // Assuming neighboringRooms is a list of Vector3 positions for neighboring rooms
        List<GameObject> neighboringRooms = GetNeighboringRooms();

        if (neighboringRooms != null && neighboringRooms.Count > 0)
        {
            
                var potentialRoom = neighboringRooms[Random.Range(0, neighboringRooms.Count)];
                Vector2 potentialDestination = potentialRoom.transform.position;
                // Check if the destination is walkable using NavMesh
                if (creature.IsWalkable(potentialDestination))
                {
                    
                    creature.controller.SetDestination(potentialDestination);
                    return; // Exit after setting a valid destination
                }
            
        }

        Vector2Int creaturePos = Vector2Int.RoundToInt(creature.transform.position);
        int maxSteps = 10;
        
       
            Vector2Int checkPos = creaturePos;
            for (int step = 5; step <= maxSteps; step++)
            {
            checkPos +=  cardinalExplo*2;
            // Check if this tile exists and is not known
           
                {
                    Vector2 worldDestination = new Vector3(checkPos.x, checkPos.y);
                    if (creature.IsWalkable(worldDestination))
                    {
                        creature.controller.SetDestination(worldDestination);
                        return;
                    }
                    else
                    cardinalExplo = Direction2D.cardinalDirectionsList[Random.Range(0,Direction2D.cardinalDirectionsList.Count)];
                }
            }
        

        Debug.LogWarning("Failed to find a valid random destination.");
    }

    private List<GameObject> GetNeighboringRooms()
    {
        List<GameObject> neighboringRooms = new List<GameObject>();
        
        foreach (var room in creature.controller.roomGenerator.dgRoomslist)
        {
            Vector2 roomPosition = room.transform.position;
            if (Vector2.Distance(creature.transform.position, roomPosition) <= exploreRange && !creature.controller.currentFaction.knowRooms.Contains(room))
            {
                        neighboringRooms.Add(room);
                        break;
            }
        }

        return neighboringRooms.Count > 0 ? neighboringRooms : null;
    }

    /*private void ExploreCurrentRoom(GameObject currentRoom)
    {
        RoomInfo roomInfo = currentRoom.GetComponent<RoomComponent>().roomInfo;
        List<TileInfo> tiles = roomInfo.positions;
        TileInfo randomTile = tiles[Random.Range(0, tiles.Count)];
        Vector2 worldDestination = new Vector3(randomTile.position.x, randomTile.position.y);
        creature.controller.SetDestination(worldDestination);

        bool allTilesKnown =false;
        foreach (var pos in tiles)
        {
            Vector2Int checkPos = new Vector2Int(pos.position.x, pos.position.y);
            if (creature.controller.currentFaction.knownTilePositions.Contains(checkPos))
            {
                allTilesKnown = true;
            }
            else
            {
                allTilesKnown = false;
            }
        }

        // Check if all tiles in the room are known

         /*bool allTilesKnown = roomInfo.positions.TrueForAll(tiles =>
           creature.controller.currentFaction.knowntileInfoDict.ContainsKey(tiles.position));*/
        /*if (allTilesKnown && !creature.controller.currentFaction.knowRooms.Contains(currentRoom))
        {
            // All tiles in the room are known, you can add logic here if needed
            creature.controller.currentFaction.knowRooms.Add(currentRoom);

        }*/


    

    /*private void RegisterNewTiles()
    {
            Vector2Int creaturePos = Vector2Int.RoundToInt(creature.transform.position);
            float detectionRange = creature.controller.detectionRange;
            var knownTiles = creature.controller.currentFaction.knowntileInfoDict;
            var knownTilePositions = new HashSet<Vector2Int>();

            for (int dx = -Mathf.CeilToInt(detectionRange); dx <= Mathf.CeilToInt(detectionRange); dx++)
            {
                for (int dy = -Mathf.CeilToInt(detectionRange); dy <= Mathf.CeilToInt(detectionRange); dy++)
                {
                    Vector2Int checkPos = creaturePos + new Vector2Int(dx, dy);
                    if (Vector2.Distance(creaturePos, checkPos) <= detectionRange)
                    {
                        TileInfo tile = creature.controller.roomGenerator.tilesInfos.Find(t => t.position == checkPos);
                        if (tile != null && !knownTilePositions.Contains(tile.position))
                        {
                            creature.controller.currentFaction.knowntileInfoDict.Add(tile.position, tile);
                        }
                    }
                }
            }
    }*/

}

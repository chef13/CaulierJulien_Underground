using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SpellOnTile : SpellTarget
{

  public SpellOnTile(Spell spell) : base(spell)
  {
  }


          public override IEnumerator CastingSpell(Spell spell)
    {
        var dungeon = DungeonGenerator.Instance;
        var manaCore = ManaCore.Instance;
        var tileTargeter = manaCore.tileTargeter;
        tileTargeter.SetActive(true);
        Vector3 origin = manaCore.spellOrigin.transform.position;
        while (true)
        {
            if(manaCore.avatarController!=null)
            manaCore.avatarController.casting = true;
            spell.transform.position = spell.manaCore.spellOrigin.position;
            // Always update the targeter position to follow the mouse

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(-0.5f, -0.5f, 0); // Adjust z to ensure it's above the floor
            mouseWorld.z = 0f;
            float distance = Vector3.Distance(origin, mouseWorld);
            if (distance > spell.beamLength * (spell.spellRange / 2f))
            {
                mouseWorld = origin + Vector3.ClampMagnitude(mouseWorld - origin, spell.beamLength * (spell.spellRange / 2f));
            }
            Vector3Int cellTargeted = Vector3Int.RoundToInt(mouseWorld);
            if (dungeon.dungeonMap.TryGetValue(cellTargeted, out TileInfo targetTile) && targetTile.isFloor)
            {
                spell.targetTile = targetTile;
                manaCore.tileTargeter.transform.position = targetTile.position + new Vector3(0.5f, 0.5f, 0);
            }
            else
            {
                Vector2 direction = (mouseWorld - origin).normalized;
                float actualLength = spell.beamLength * (spell.spellRange / 2f);
                float stepSize = 1f;
                int steps = Mathf.FloorToInt(actualLength / stepSize);
                HashSet<Vector3Int> visitedCells = new();
                Vector3Int lastFloorCell = Vector3Int.zero;
                bool lastFloorCellSet = false;
                bool hitWall = false;
                bool valideTile = false;

                for (int i = 0; i <= steps && !hitWall && !valideTile; i++)
                {
                    Vector3 samplePoint = origin + (Vector3)(direction * (i * stepSize));
                    Vector3Int cell = Vector3Int.RoundToInt(samplePoint);

                    if (visitedCells.Contains(cell))
                        continue;

                    visitedCells.Add(cell);

                    if (dungeon.dungeonMap.TryGetValue(cell, out TileInfo tile))
                    {
                        if (tile.isFloor)
                        {
                            lastFloorCell = cell;
                            lastFloorCellSet = true;
                            spell.targetTile = tile;
                            if (tile.position == cellTargeted)
                            {
                                valideTile = true;
                                manaCore.tileTargeter.transform.position = tile.position + new Vector3(0.5f, 0.5f, 0);
                            }
                        }
                        else
                        {
                            spell.targetTile = tile;
                            manaCore.tileTargeter.transform.position = tile.position + new Vector3(0.5f, 0.5f, 0);
                            hitWall = true;

                            if (lastFloorCellSet && dungeon.dungeonMap.TryGetValue(lastFloorCell, out TileInfo fallbackTile) && fallbackTile.isFloor)
                            {
                                spell.targetTile = fallbackTile;
                                manaCore.tileTargeter.transform.position = fallbackTile.position + new Vector3(0.5f, 0.5f, 0);
                            }
                        }
                    }
                }
            }


            // Only cast on mouse up
                if (Input.GetMouseButtonUp(0))
                {
                    manaCore.tileTargeter.SetActive(false);
                    if (manaCore.avatarController != null)
                    manaCore.avatarController.casting = false;

                    if (spell.targetTile != null)
                    {
                        spell.Cast();
                    }
                    else
                    {
                        Debug.LogWarning("No valid tile targeted for spell casting.");
                    }

                    
                    manaCore.castingCoroutine = null;
                    yield break;
                }
                if (Input.GetMouseButtonUp(1))
                {
                     manaCore.tileTargeter.SetActive(false);
                    // Cancel casting if right mouse button is pressed
                if (manaCore.avatarController != null)
                    manaCore.avatarController.casting = false;

                   
                    manaCore.castingCoroutine = null;
                    yield break;
                }

            yield return null;
        }
    }

}

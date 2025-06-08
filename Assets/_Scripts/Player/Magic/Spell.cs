using System;
using UnityEngine;

public class Spell : MonoBehaviour
{
    public enum SpellColors
    {
        Brown,
        Blue,
        Green,
        White,
        Black
    }
    public enum SpellTargets
    {
        Tile,
        Room,
        Creature,
        Faction,
        Beam,
        Dungeon
    }

    public SpellColors spellColor;
    public SpellTargets spellTarget;
    private SpellColor colorHandler;
    private SpellTarget targetHandler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorHandler = CreateColorHandler(spellColor);
        targetHandler = CreateTargetHandler(spellTarget);
    }

    public void Cast(GameObject target)
    {
        targetHandler?.SwitchTarget(this);
        colorHandler?.ApplyColorEffect(this);

    }

    private SpellColor CreateColorHandler(SpellColors color)
    {
        return color switch
        {
            SpellColors.Brown => new SpellBrown(this),
            SpellColors.Blue => new SpellBlue(this),
            SpellColors.Green => new SpellGreen(this),
            SpellColors.White => new SpellWhite(this),
            SpellColors.Black => new SpellBlack(this),
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
    }
    
    private SpellTarget CreateTargetHandler(SpellTargets target)
    {
        return target switch
        {
            SpellTargets.Tile => new SpellOnTile(this),
            SpellTargets.Room => new SpellOnRoom(this),
            SpellTargets.Creature => new SpellOnCreature(this),
            SpellTargets.Faction => new SpellOnFaction(this),
            SpellTargets.Beam => new SpellBeam(this),
            SpellTargets.Dungeon => new SpellOnDungeon(this),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

}

using System;
using System.Collections;
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
    public SpellColor colorHandler;
    public SpellTarget targetHandler;
    public SpellEffectSO effect;
    public ManaCore manaCore;

    [SerializeField] public int spellPower = 1;
    [SerializeField] public int spellRange = 1;
    [SerializeField] public int spellDuration = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void Build(SpellColors color, SpellTargets target, SpellEffectSO effect, ManaCore manaCore)
    {
        this.spellColor = color;
        this.spellTarget = target;
        this.effect = effect;
        this.manaCore = manaCore;

        colorHandler = CreateColorHandler(color);
        targetHandler = CreateTargetHandler(target);


        targetHandler?.SwitchTarget(this);
        colorHandler?.ApplyColorEffect(this);

    }

    public void Cast()
    {
        Debug.Log($"Casting spell with color: {spellColor} and target: {spellTarget}");
        effect?.ApplyEffect(this, null, spellRange, spellPower, spellDuration);
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

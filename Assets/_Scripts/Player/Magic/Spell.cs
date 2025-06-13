using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        Beam,
    }

    public enum SpellTargets_Ritual
    {
        Faction,
        Dungeon,
        RoomRitual
    }

    public bool avatarSpell = false;
    public bool emptySpell = false;
    public int currentIndexDisplay = 0;
    public SpellColors spellColor;
    public SpellTargets spellTarget;
    public SpellTargets_Ritual spellTargetRitual;
    public SpellColor colorHandler;
    public SpellTarget targetHandler;
    public SpellEffectSO effect;
    public ManaCore manaCore;
    public float beamLength = 8f;
    public TileInfo targetTile;
    public FactionBehaviour targetFaction;
    public List<CreatureController> targetedCreatures = new List<CreatureController>();
    public List<TileInfo> targetedTiles = new List<TileInfo>();
    public Sprite spellIcon;
    [SerializeField] public int spellPower = 1;
    [SerializeField] public int spellRange = 1;
    [SerializeField] public int spellDuration = 1;
    public int manaCost = 10;
    public bool isRitualSpell = false;
    public Coroutine SpellEffectCoroutine;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private IEnumerator DelayedSummon()
    {
        yield return new WaitForSeconds(0.1f);
        Cast();
        yield break;
    }

    public void Build(SpellColors color, SpellTargets target, SpellEffectSO effect, ManaCore manaCore)
    {
        this.manaCore = manaCore;
        this.spellColor = color;
        this.spellTarget = target;
        this.effect = effect;


        colorHandler = CreateColorHandler(color);
        targetHandler = CreateTargetHandler(target);


        targetHandler?.SwitchTarget(this);
        colorHandler?.SwitchColor(this);
    }

    public void BuildRitual(SpellColors color, SpellTargets_Ritual target, SpellEffectSO effect, ManaCore manaCore)
    {
        this.manaCore = manaCore;
        this.spellColor = color;
        this.spellTargetRitual = target;
        this.effect = effect;

        colorHandler = CreateColorHandler(color);
        targetHandler = CreateRitualTargetHandler(target);

        targetHandler?.SwitchTargetRitual(this);
        colorHandler?.SwitchColor(this);
    }

    public void Cast()
    {
        if (emptySpell)
        {
            Debug.LogWarning("Attempted to cast an empty spell.");
            return;
        }
        if (targetHandler == null || colorHandler == null)
        {
            Debug.LogError("Spell is not properly initialized. Cannot cast.");
            return;
        }
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
            SpellTargets.Beam => new SpellBeam(this),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }
    
    private SpellTarget CreateRitualTargetHandler(SpellTargets_Ritual target)
    {
        return target switch
        {
            SpellTargets_Ritual.Faction => new SpellOnFaction(this),
            SpellTargets_Ritual.Dungeon => new SpellOnDungeon(this),
            SpellTargets_Ritual.RoomRitual => new SpellOnRoomRitual(this),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }




}

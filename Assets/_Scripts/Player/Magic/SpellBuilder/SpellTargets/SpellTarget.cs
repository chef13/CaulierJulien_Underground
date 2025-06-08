using Mono.Cecil.Cil;
using UnityEngine;

public abstract class SpellTarget
{
    public enum SpellTargets
    {
        tile,
        Room,
        Creature,
        Faction,
        Beam,
        Dungeon
    }
    public SpellTargets spellTarget;
    public Spell spell;
    public SpellTarget(Spell spell)
    {
        this.spell = spell;
    }

    public TileInfo tileTaget;
    public RoomInfo roomTarget;
    public CreatureController creatureTarget;
    public FactionBehaviour factionTarget;
    public Vector2Int beamTarget;
    public ManaCore dungeonTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {

    }
    public void GetTarget(Spell spell, TileInfo tileTaget = null, RoomInfo roomTarget = null, CreatureController creature = null, Vector2Int beamTarget = default, ManaCore dungeonTarget = null)
    {
        this.spell = spell;
        this.tileTaget = tileTaget;
        this.roomTarget = roomTarget;
        this.creatureTarget = creature;
        this.beamTarget = beamTarget;
        this.dungeonTarget = dungeonTarget;


        SwitchTarget(spell);
    }
    
    
    public void SwitchTarget(Spell spell)
    {
        switch (spell.spellTarget)
        {
            case Spell.SpellTargets.Tile:
                spellTarget = SpellTargets.tile;
                break;
            case Spell.SpellTargets.Room:
                spellTarget = SpellTargets.Room;
                break;
            case Spell.SpellTargets.Creature:
                spellTarget = SpellTargets.Creature;
                break;
            case Spell.SpellTargets.Faction:
                spellTarget = SpellTargets.Faction;
                break;
            case Spell.SpellTargets.Beam:
                spellTarget = SpellTargets.Beam;
                break;
            case Spell.SpellTargets.Dungeon:
                spellTarget = SpellTargets.Dungeon;
                break;
        }
    }
}

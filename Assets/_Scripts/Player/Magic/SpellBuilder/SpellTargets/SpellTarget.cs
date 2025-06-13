
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
public abstract class SpellTarget
{
    SpellBuilder spellBook = SpellBuilder.Instance;
    public enum SpellTargets
    {
        tile,
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
    public SpellTargets spellTarget;
    public SpellTargets_Ritual spellTargetRitual;
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
                new SpellOnTile(spell);
                break;
            case Spell.SpellTargets.Room:
                spellTarget = SpellTargets.Room;
                new SpellOnRoom(spell);
                break;
            case Spell.SpellTargets.Creature:
                spellTarget = SpellTargets.Creature;
                new SpellOnCreature(spell);
                break;
            case Spell.SpellTargets.Beam:
                spellTarget = SpellTargets.Beam;
                new SpellBeam(spell);
                break;
        }
    }

    public void SwitchTargetRitual(Spell spell)
    {
        switch (spell.spellTargetRitual)
        {
            case Spell.SpellTargets_Ritual.Faction:
                spellTargetRitual = SpellTargets_Ritual.Faction;
                new SpellOnFaction(spell);
                break;
            case Spell.SpellTargets_Ritual.Dungeon:
                spellTargetRitual = SpellTargets_Ritual.Dungeon;
                new SpellOnDungeon(spell);
                break;
            case Spell.SpellTargets_Ritual.RoomRitual:
                spellTargetRitual = SpellTargets_Ritual.RoomRitual;
                new SpellOnRoomRitual(spell);
                break;
        }
    }
    
    public abstract IEnumerator CastingSpell(Spell spell);
    
}

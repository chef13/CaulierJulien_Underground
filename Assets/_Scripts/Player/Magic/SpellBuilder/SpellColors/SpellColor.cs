using System.Collections.Generic;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

public abstract class SpellColor
{
    public Spell spell;
    public TilemapVisualizer TilemapVisualizer => TilemapVisualizer.Instance;
    public ManaCore ManaCore => ManaCore.Instance;
    public bool avatarSpell => ManaCore.currentAvatar != null;
    public GameObject AvatarCreature => ManaCore.currentAvatar;
    public FactionSpawner FactionSpawner => FactionSpawner.instance;
    public FactionBehaviour dungeonFaction => FactionSpawner.dungeonFaction;

    public enum SpellColors
    {
        Brown,
        Blue,
        Green,
        White,
        Black
    }
    public SpellColors _spellColor;

    public SpellColor(Spell spell)
    {
        this.spell = spell;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SwitchColor(Spell spell)
    {
        //_spellColor = spellColor;
        switch (spell.spellColor)
        {
            case Spell.SpellColors.Brown:
                //_spellColor = SpellColors.Brown;
                spell.colorHandler = new SpellBrown(spell);
                break;
            case Spell.SpellColors.Blue:
                //_spellColor = SpellColors.Blue;
                spell.colorHandler = new SpellBlue(spell);
                break;
            case Spell.SpellColors.White:
                //_spellColor = SpellColors.White;
                spell.colorHandler = new SpellWhite(spell);
                break;
            case Spell.SpellColors.Green:
                //_spellColor = SpellColors.Green;
                spell.colorHandler = new SpellGreen(spell);
                break;
            case Spell.SpellColors.Black:
                //_spellColor = SpellColors.Black;
                spell.colorHandler = new SpellBlack(spell);
                break;
        }
    }

    public virtual void EffectOnTile(TileInfo tile) { }
    public virtual void EffectOnRoom(RoomInfo room) { }
    public virtual void EffectOnCreature(CreatureController creature) { }

    public virtual void EffectOnAlliedCreature(CreatureController creature)    {    }
    public virtual void EffectOnEnnemisCreature(CreatureController creature)    {    }
    public virtual void EffectOnFaction(FactionBehaviour faction, float duration) { }
    public virtual void EffectOnBeam(Vector2Int beamTarget) { }
    public virtual void EffectOnDungeon( float duration) { }
    public virtual void EffectOnArea(List<TileInfo> area) { }

}

using CrashKonijn.Goap.Runtime;
using UnityEngine;

public abstract class SpellColor
{
    public Spell spell;
    public enum SpellColors
    {
        Brown,
        Blue,
        Red,
        Green,
        Yellow,
        Purple
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
    
    public void ApplyColorEffect(Spell spell)
    {
        // This method can be overridden in derived classes to apply specific color effects
        EffectOnTile();
    }

    public virtual void EffectOnTile() { }

}

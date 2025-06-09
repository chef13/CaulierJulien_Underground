using UnityEngine;

public abstract class SpellEffectSO : ScriptableObject
{
    public string effectName;
    public Sprite icon; // optional, for U
    public float beamLength;

    public abstract void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1);
}

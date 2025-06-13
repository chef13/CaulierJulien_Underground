using UnityEngine;
using UnityEngine.UI;

public abstract class SpellEffectSO : ScriptableObject
{
    public int manacost = 10;
    public float effectDuration = 50f;
    public string description;
    public string spellName;
    public Sprite icon; // optional, for U


    public abstract void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1);


    public virtual void ApplyDamage(Spell caster, CreatureController target, int damage)
    {
        if (target == null || target.isDead)
        {
            Debug.LogWarning("Target is null or dead, cannot apply damage.");
            return;
        }

        if (caster.manaCore.avatarController != null)
            target.OnHit(caster.manaCore.avatarController, damage);
        else
            target.OnHit(null, damage);
    }
}

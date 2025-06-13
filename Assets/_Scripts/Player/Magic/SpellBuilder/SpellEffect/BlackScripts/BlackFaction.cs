using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BlackFactionEffect")]
public class BlackFactionEffectSO : SpellEffectSO
{

    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {
        var targetFaction = caster.targetFaction;
        caster.manaCore.UseMana(caster.manaCost);
        caster.colorHandler.EffectOnFaction(targetFaction, effectDuration * durationlevel);
}

}

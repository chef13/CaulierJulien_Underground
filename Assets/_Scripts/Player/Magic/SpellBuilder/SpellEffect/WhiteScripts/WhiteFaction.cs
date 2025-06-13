using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/WhiteFactionEffect")]
public class WhiteFactionEffectSO : SpellEffectSO
{


public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
{
    
        caster.manaCore.UseMana(caster.manaCost);
        var targetFaction = caster.targetFaction;
        caster.colorHandler.EffectOnFaction(targetFaction, effectDuration * durationlevel);
}

}

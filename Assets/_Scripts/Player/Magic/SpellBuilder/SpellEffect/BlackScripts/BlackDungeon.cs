using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BlackDungeonEffect")]
public class BlackDungeonEffectSO : SpellEffectSO
{
    public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
    {
        caster.manaCore.UseMana(caster.manaCost);
        caster.colorHandler.EffectOnDungeon( effectDuration * durationlevel);
}

}

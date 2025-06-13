using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Magic/Effects/BlueCreatureEffect")]
public class BlueCreatureEffectSO : SpellEffectSO
{

    private static HashSet<TileInfo> waterAffectedTiles = new();

public override void ApplyEffect(Spell caster, GameObject target = null, int rangelevel = 1, int powerlevel = 1, int durationlevel = 1)
{
        caster.manaCore.UseMana(caster.manaCost);
        var targets = caster.targetedCreatures;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] != null && targets[i].currentFaction.dungeonFav < -5)
            {
                caster.colorHandler.EffectOnEnnemisCreature(targets[i]);
            }
        
            if (targets[i] != null && targets[i].currentFaction.dungeonFav <= 5 && targets[i].currentFaction.dungeonFav >= -5)
            {
                caster.colorHandler.EffectOnCreature(targets[i]);
            }

            if (targets[i] != null && targets[i].currentFaction.dungeonFav > 5)
            {
                caster.colorHandler.EffectOnAlliedCreature(targets[i]);
            }
        }
}

}

using UnityEngine;
using System.Collections;
public class SpellSummon : SpellTarget
{

    public SpellSummon(Spell spell) : base(spell)
    {
    }

      public override IEnumerator CastingSpell()
    {
        yield break;
    }
}

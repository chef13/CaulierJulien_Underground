using UnityEngine;
using System.Collections;
public class SpellOnTile : SpellTarget
{

    public SpellOnTile(Spell spell) : base(spell)
    {
    }

      public override IEnumerator CastingSpell()
    {
        yield break;
    }
}

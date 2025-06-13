using UnityEngine;
using System.Collections;
public class SpellOnDungeon : SpellTarget
{

        public SpellOnDungeon(Spell spell) : base(spell)
    {
    }
        public override IEnumerator CastingSpell(Spell spell)
    {
        var manaCore = spell.manaCore;
        manaCore.UseMana(spell.manaCost);
        spell.Cast();
        yield break;
    }
}

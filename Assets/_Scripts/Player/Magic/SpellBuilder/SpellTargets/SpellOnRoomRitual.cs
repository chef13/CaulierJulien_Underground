using UnityEngine;
using System.Collections;
public class SpellOnRoomRitual : SpellTarget
{
    public TileInfo targetTile;
        public SpellOnRoomRitual(Spell spell) : base(spell)
    {
    }
        public override IEnumerator CastingSpell(Spell spell)
    {
        yield break;
    }
}

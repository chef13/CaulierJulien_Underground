using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
public class SpellBeam : SpellTarget
{
    public Image icon;
    public TileInfo targetTile;
    public SpellBeam(Spell spell) : base(spell)
    {
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override IEnumerator CastingSpell(Spell spell)
    {
            if (spell == null) { Debug.LogError("Spell is null in SpellBeam!"); yield break; }
             if (spell.manaCore == null) { Debug.LogError("spell.manaCore is null in SpellBeam!"); yield break; }
            if (spell.manaCore.spellOrigin == null) { Debug.LogError("spell.manaCore.spellOrigin is null in SpellBeam!"); yield break; }
  
        //spell = spell.manaCore.spell.GetComponent<Spell>();
        var manaCore = ManaCore.Instance;
        manaCore.lineRenderer.enabled = true;
        manaCore.lineRenderer.positionCount = 2;
        manaCore.lineRenderer.startColor = spell.manaCore.GetColorFromSpellColor(spell.spellColor);
        manaCore.lineRenderer.endColor = spell.manaCore.GetColorFromSpellColor(spell.spellColor);

        while (!Input.GetMouseButtonUp(0))
        {
            if(manaCore.avatarController!=null)
            manaCore.avatarController.casting = true;
            spell.transform.position = spell.manaCore.spellOrigin.position; // Update spell position to the player's position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 direction = (mouseWorld - spell.manaCore.spellOrigin.position).normalized;
            RaycastHit2D hits = Physics2D.Raycast(spell.manaCore.spellOrigin.position, direction, spell.beamLength * (spell.spellRange/2f), LayerMask.GetMask("wall"));
            Vector3 hitPos = hits.collider != null ? hits.point : spell.manaCore.spellOrigin.position + Vector3.ClampMagnitude(mouseWorld - spell.manaCore.spellOrigin.position, spell.beamLength * (spell.spellRange/2f));
            manaCore.lineRenderer.SetPosition(0, spell.manaCore.spellOrigin.position);
            manaCore.lineRenderer.SetPosition(1, hitPos);

            if (Input.GetMouseButtonUp(1))
            {
                // Cancel casting if right mouse button is pressed
                if(manaCore.avatarController!=null)
                manaCore.avatarController.casting = false;
                Debug.Log("Casting cancelled");
                manaCore.lineRenderer.enabled = false;
                manaCore.castingCoroutine = null;
                yield break;
            }

            yield return null; // Wait for next frame
        }
        if(manaCore.avatarController!=null)
        manaCore.avatarController.casting = false;
        manaCore.lineRenderer.enabled = false; // Turn off visual after casting
        spell.Cast();
        manaCore.castingCoroutine = null;
        yield break;
    }
}

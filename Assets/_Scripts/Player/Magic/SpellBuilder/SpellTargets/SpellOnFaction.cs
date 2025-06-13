using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
public class SpellOnFaction : SpellTarget
{
    public FactionBehaviour targetFaction;
    public SpellOnFaction(Spell spell) : base(spell)
    {
    }
    public override IEnumerator CastingSpell(Spell spell)
    {
                

        var dungeon = DungeonGenerator.Instance;
        var manaCore = ManaCore.Instance;
        var factionSpawner = FactionSpawner.instance;
        if(manaCore.avatarController!=null)
        manaCore.avatarController.casting = true;
        SpellBuilder.Instance.OpenRitualCanvas();
        if (SpellBuilder.Instance.faction1 != null)
        SpellBuilder.Instance.faction1.onClick.AddListener(() => OnFactiontargetSelect(SpellBuilder.Instance.faction1));
        if (SpellBuilder.Instance.faction2 != null)
        SpellBuilder.Instance.faction2.onClick.AddListener(() => OnFactiontargetSelect(SpellBuilder.Instance.faction2));
        if (SpellBuilder.Instance.faction3 != null)
        SpellBuilder.Instance.faction3.onClick.AddListener(() => OnFactiontargetSelect(SpellBuilder.Instance.faction3));
        if (SpellBuilder.Instance.faction4 != null)
        SpellBuilder.Instance.faction4.onClick.AddListener(() => OnFactiontargetSelect(SpellBuilder.Instance.faction4));
        if (SpellBuilder.Instance.faction5 != null)
        SpellBuilder.Instance.faction5.onClick.AddListener(() => OnFactiontargetSelect(SpellBuilder.Instance.faction5));
        if (SpellBuilder.Instance.faction6 != null)
        SpellBuilder.Instance.faction6.onClick.AddListener(() => OnFactiontargetSelect(SpellBuilder.Instance.faction6));

        while (targetFaction == null)
        {
            if (Input.GetMouseButtonUp(1))
            {
                // Cancel casting if right mouse button is pressed
                if(manaCore.avatarController!=null)
                manaCore.avatarController.casting = false;
                manaCore.castingCoroutine = null;
                RemoveAllListeners();
                 SpellBuilder.Instance.RitualOnFactionCanvas.enabled = false;
                 SpellBuilder.Instance.RitualOnFactionCanvas.gameObject.SetActive(false);
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }




        spell.targetFaction = targetFaction;
        RemoveAllListeners();




        if(manaCore.avatarController!=null)
        manaCore.avatarController.casting = false;
        spell.Cast();



        manaCore.castingCoroutine = null;
        yield break;
    }
    


     public void OnFactiontargetSelect(Button button)
    {
        if (button == SpellBuilder.Instance.faction1)
        {
            targetFaction = FactionSpawner.instance.factionsIA[0];
        }
        else if (button == SpellBuilder.Instance.faction2)
        {
            targetFaction = FactionSpawner.instance.factionsIA[1];
        }
        else if (button == SpellBuilder.Instance.faction3)
        {
            targetFaction = FactionSpawner.instance.factionsIA[2];
        }
        else if (button == SpellBuilder.Instance.faction4)
        {
            targetFaction = FactionSpawner.instance.factionsIA[3];
        }
        else if (button == SpellBuilder.Instance.faction5)
        {
            targetFaction = FactionSpawner.instance.factionsIA[4];
        }
        else if (button == SpellBuilder.Instance.faction6)
        {
            targetFaction = FactionSpawner.instance.factionsIA[5];
        }
        SpellBuilder.Instance.RitualOnFactionCanvas.enabled = false;
        SpellBuilder.Instance.RitualOnFactionCanvas.gameObject.SetActive(false);
    } 

    private void RemoveAllListeners()
    {
        if (SpellBuilder.Instance.faction1 != null)
            SpellBuilder.Instance.faction1.onClick.RemoveAllListeners();
        if (SpellBuilder.Instance.faction2 != null)
            SpellBuilder.Instance.faction2.onClick.RemoveAllListeners();
        if (SpellBuilder.Instance.faction3 != null)
            SpellBuilder.Instance.faction3.onClick.RemoveAllListeners();
        if (SpellBuilder.Instance.faction4 != null)
            SpellBuilder.Instance.faction4.onClick.RemoveAllListeners();
        if (SpellBuilder.Instance.faction5 != null)
            SpellBuilder.Instance.faction5.onClick.RemoveAllListeners();
        if (SpellBuilder.Instance.faction6 != null)
            SpellBuilder.Instance.faction6.onClick.RemoveAllListeners();
    }
}

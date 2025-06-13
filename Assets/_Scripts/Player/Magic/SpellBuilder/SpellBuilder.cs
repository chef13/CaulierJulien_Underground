
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;

using TMPro;

public class SpellBuilder : MonoBehaviour
{
    public static SpellBuilder Instance;
    public Canvas SpellBook;
    public bool isSpellBookOpen = false;
    public GameObject spellPrefab;
    public SpellColor spellColor;
    public SpellTarget spellTarget;
    public Spell spell;
    public Spell ritualSpell;
    public SpellEffectList spellEffectList;

    public enum SpellColors
    {
        Brown,
        Blue,
        Green,
        White,
        Black
    }
    public SpellColors currentSpellColor;
    public SpellColors currentSpellColor_Ritual;
    public enum SpellTargets
    {
        Tile,
        Room,
        Creature,
        Beam,
    }

    public enum SpellTargets_Ritual
    {
        Faction,
        Dungeon,
        Room
    }
    public Image currentSpellIcon;
    public Image currentSpellIcon_Ritual;
    //public Image spellIcon1, spellIcon2, spellIcon3, spellIcon4, spellIcon5;
    //public List<GameObject> spellObjects = new List<GameObject>();
    public Spell spell1, spell2, spell3, spell4, spell5;

    public Image spellIcon1, spellIcon2, spellIcon3, spellIcon4, spellIcon5;
    public Slider manaSlider;
    public TMP_Text manaText;
    public Button currentSelectedSlotButton;
    public GameObject currentSelectedSpellObject;
    public SpellTargets currentSpellTargets;
    public SpellTargets_Ritual currentSpellTargets_Ritual;
    public Button tileTargetButton, roomTargetButton, roomTargetButton_Ritual, beamTargetButton, creatureTargetButton, factionTargetButton_Ritual, dungeonTargetButton_Ritual,
    blueButton, greenButton, blackButton, whiteButton, brownButton, buildButton, spellSlot1, spellSlot2, spellSlot3, spellSlot4, spellSlot5,
    powerPLUSbutton, powerMINUSbutton, rangePLUSbutton, rangeMINUSbutton;
    private List<Button> colorButtons = new List<Button>();
    private List<Button> colorButtons_Ritual = new List<Button>();

    private List<Button> targetButtons = new List<Button>();
    private List<Button> targetButtons_Ritual = new List<Button>();
    public Button blueButtonRitual, greenButtonRitual, blackButtonRitual, whiteButtonRitual, brownButtonRitual;
    public Color defaultColor = Color.white;
    public GameObject selectorTarget;
    public GameObject selectorTarget_Ritual;
    public GameObject selectorColor_Ritual;
    public GameObject selectorColor;
    public GameObject spellSlotSelector;
    private SpellEffectSO spellEffect;
    private SpellEffectSO spellEffectRitual;
    public TMP_Text spellPowerText;
    public TMP_Text spellPowerText_Ritual;
    [Range(1, 3)]
    public int spellPowerLVL;
    [Range(1, 3)]
    public int spellPowerLVL_Ritual;
    public TMP_Text spellRangeText;
    public TMP_Text spellDurationText_Ritual;
    [Range(1, 3)]
    public int spellRangeLVL;
    [Range(1, 3)]
    public int spellDurationLVL_Ritual;
    public TMP_Text manaCostText;
    public TMP_Text manaCostText_Ritual;
    public int manaCost;
    public int manaCost_Ritual;
    public Button powerPLUSbutton_Ritual, powerMINUSbutton_Ritual, durationPLUSbutton_Ritual, durationMINUSbutton_Ritual;

    public Canvas RitualOnFactionCanvas;
    public Button faction1, faction2, faction3, faction4, faction5, faction6;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        spell1.emptySpell = true;
        spell2.emptySpell = true;
        spell3.emptySpell = true;
        spell4.emptySpell = true;
        spell5.emptySpell = true;
        spellEffect = PickSpellEffect();
        spellEffectRitual = PickSpellEffect_Ritual();
        UpdateSpellIcon();
        UpdateSpellIcon_Ritual();
        currentSelectedSlotButton = spellSlot1; // Default to the first spell slot
        currentSelectedSpellObject = spell1.gameObject; // Default to the first spell object
        float _manaCost = spellEffect.manacost * (spellPowerLVL * 0.75f) * (spellRangeLVL * 0.8f);
        float _manaCost_Ritual = spellEffectRitual.manacost * (spellPowerLVL_Ritual * 0.75f) * (spellDurationLVL_Ritual * 0.8f);
        manaCost = Mathf.RoundToInt(_manaCost);
        manaCost_Ritual = Mathf.RoundToInt(_manaCost_Ritual);
        manaCostText.text = manaCost.ToString();
        manaCostText_Ritual.text = manaCost_Ritual.ToString();
        blueButton.onClick.AddListener(() => ButtonClicked(blueButton, colorButtons, SpellColors.Blue, currentSpellTargets));
        greenButton.onClick.AddListener(() => ButtonClicked(greenButton, colorButtons, SpellColors.Green, currentSpellTargets));
        blackButton.onClick.AddListener(() => ButtonClicked(blackButton, colorButtons, SpellColors.Black, currentSpellTargets));
        whiteButton.onClick.AddListener(() => ButtonClicked(whiteButton, colorButtons, SpellColors.White, currentSpellTargets));
        brownButton.onClick.AddListener(() => ButtonClicked(brownButton, colorButtons, SpellColors.Brown, currentSpellTargets));

        tileTargetButton.onClick.AddListener(() => ButtonClicked(tileTargetButton, targetButtons, currentSpellColor, SpellTargets.Tile));
        roomTargetButton.onClick.AddListener(() => ButtonClicked(roomTargetButton, targetButtons, currentSpellColor, SpellTargets.Room));
        beamTargetButton.onClick.AddListener(() => ButtonClicked(beamTargetButton, targetButtons, currentSpellColor, SpellTargets.Beam));
        creatureTargetButton.onClick.AddListener(() => ButtonClicked(creatureTargetButton, targetButtons, currentSpellColor, SpellTargets.Creature));


        factionTargetButton_Ritual.onClick.AddListener(() => ButtonClicked_Ritual(factionTargetButton_Ritual, targetButtons, currentSpellColor, SpellTargets_Ritual.Faction));
        dungeonTargetButton_Ritual.onClick.AddListener(() => ButtonClicked_Ritual(dungeonTargetButton_Ritual, targetButtons, currentSpellColor, SpellTargets_Ritual.Dungeon));
        roomTargetButton_Ritual.onClick.AddListener(() => ButtonClicked_Ritual(roomTargetButton_Ritual, targetButtons, currentSpellColor, SpellTargets_Ritual.Room));

        blueButtonRitual.onClick.AddListener(() => ButtonClicked_Ritual(blueButtonRitual, colorButtons_Ritual, SpellColors.Blue, currentSpellTargets_Ritual));
        greenButtonRitual.onClick.AddListener(() => ButtonClicked_Ritual(greenButtonRitual, colorButtons_Ritual, SpellColors.Green, currentSpellTargets_Ritual));
        blackButtonRitual.onClick.AddListener(() => ButtonClicked_Ritual(blackButtonRitual, colorButtons_Ritual, SpellColors.Black, currentSpellTargets_Ritual));
        whiteButtonRitual.onClick.AddListener(() => ButtonClicked_Ritual(whiteButtonRitual, colorButtons_Ritual, SpellColors.White, currentSpellTargets_Ritual));
        brownButtonRitual.onClick.AddListener(() => ButtonClicked_Ritual(brownButtonRitual, colorButtons_Ritual, SpellColors.Brown, currentSpellTargets_Ritual));

        spellSlot1.onClick.AddListener(() => SetSelectedSpellSlot(spellSlot1));
        spellSlot2.onClick.AddListener(() => SetSelectedSpellSlot(spellSlot2));
        spellSlot3.onClick.AddListener(() => SetSelectedSpellSlot(spellSlot3));
        spellSlot4.onClick.AddListener(() => SetSelectedSpellSlot(spellSlot4));
        spellSlot5.onClick.AddListener(() => SetSelectedSpellSlot(spellSlot5));

        powerMINUSbutton.onClick.AddListener(() => powerButtonClicked(powerMINUSbutton));
        powerPLUSbutton.onClick.AddListener(() => powerButtonClicked(powerPLUSbutton));
        rangeMINUSbutton.onClick.AddListener(() => powerButtonClicked(rangeMINUSbutton));
        rangePLUSbutton.onClick.AddListener(() => powerButtonClicked(rangePLUSbutton));

        powerMINUSbutton_Ritual.onClick.AddListener(() => powerButtonClicked_Ritual(powerMINUSbutton_Ritual));
        powerPLUSbutton_Ritual.onClick.AddListener(() => powerButtonClicked_Ritual(powerPLUSbutton_Ritual));
        durationMINUSbutton_Ritual.onClick.AddListener(() => powerButtonClicked_Ritual(durationMINUSbutton_Ritual));
        durationPLUSbutton_Ritual.onClick.AddListener(() => powerButtonClicked_Ritual(durationPLUSbutton_Ritual));


    }

    /*public void CreateSpell()
    {

        spell = currentSelectedSlotObject.GetComponent<Spell>();
        spell.transform.SetParent(ManaCore.Instance.transform);
        spell.transform.localPosition = Vector3.zero; // Reset position to ManaCore's position
        spell.colorHandler = SetSpellColor();
        spell.targetHandler = GetSpellTarget();
        spell.effect = PickSpellEffect();
        //spell.spellIcon = PickSpellEffect().icon;
        spell.manaCore = ManaCore.Instance;
        int firstSpell = 0;
        foreach (var spell in spellObjects)
        {
            var effectCheck = spell.GetComponent<Spell>();
            if (effectCheck.emptySpell)
            {
                firstSpell ++;
            }
        }
        if (firstSpell == 4)
        {
            ManaCore.Instance.currentSpell = spell;
            var spellSlotIndex = spellSlots.IndexOf(currentSelectedSlotButton);
            spellIcons[spellSlotIndex].sprite = spell.effect.icon; // Set the icon for the selected spell slot
            if (spellSlotIndex == 0)
            {
                SwitchSpellSelected(true); // Switch to the next spell slot if the first one is filled
                SwitchSpellSelected(true); // Switch back to the first spell slot
            }
            else if (spellSlotIndex == 1)
            {
                SwitchSpellSelected(true); // Switch to the previous spell slot if the second one is filled
            }
            else if (spellSlotIndex == 3)
            {
                SwitchSpellSelected(false); // Switch back to the first spell slot
            }
            else if (spellSlotIndex == 4)
            {
                SwitchSpellSelected(false); // Switch to the next spell slot if the fifth one is filled
                SwitchSpellSelected(false); // Switch back to the first spell slot
            }
        }
    }*/

    public void CreateSpell()
    {
        spell = currentSelectedSpellObject.GetComponent<Spell>();
        spell.transform.SetParent(ManaCore.Instance.transform);
        spell.transform.localPosition = Vector3.zero;

        spell.manaCore = ManaCore.Instance; // <-- Set this first!
        spell.colorHandler = SetSpellColor();
        spell.targetHandler = GetSpellTarget();
        spell.effect = PickSpellEffect();
        spell.emptySpell = false; // Mark the spell as not empty
        spell.spellPower = spellPowerLVL;
        spell.spellRange = spellRangeLVL;
        spell.manaCost = manaCost;

        //DrawnSpellInBook(); // Update the spell book UI with the new spell

        if (currentSelectedSpellObject == spell1.gameObject)
        {
            spellIcon1.sprite = spell.effect.icon; // Set the icon for the first spell slot
        }
        else if (currentSelectedSpellObject == spell2.gameObject)
        {
            spellIcon2.sprite = spell.effect.icon; // Set the icon for the second spell slot
        }
        else if (currentSelectedSpellObject == spell3.gameObject)
        {
            spellIcon3.sprite = spell.effect.icon; // Set the icon for the third spell slot
        }
        else if (currentSelectedSpellObject == spell4.gameObject)
        {
            spellIcon4.sprite = spell.effect.icon; // Set the icon for the fourth spell slot
        }
        else if (currentSelectedSpellObject == spell5.gameObject)
        {
            spellIcon5.sprite = spell.effect.icon; // Set the icon for the fifth spell slot
        }
    }

    public void CastRitual()
    {
        ritualSpell = Instantiate(spellPrefab, ManaCore.Instance.transform).GetComponent<Spell>();
        ritualSpell.transform.localPosition = Vector3.zero; // Reset position to ManaCore's position
        ritualSpell.manaCore = ManaCore.Instance; // <-- Set this first!
        ritualSpell.colorHandler = SetSpellColor_Ritual();
        ritualSpell.targetHandler = GetSpellTarget_Ritual();
        ritualSpell.effect = PickSpellEffect_Ritual();
        ritualSpell.spellPower = spellPowerLVL_Ritual;
        ritualSpell.spellDuration = spellDurationLVL_Ritual;
        ritualSpell.manaCost = manaCost_Ritual;
        ritualSpell.targetHandler.CastingSpell(ritualSpell);
        ritualSpell.BuildRitual(
            (Spell.SpellColors)currentSpellColor_Ritual,
            (Spell.SpellTargets_Ritual)currentSpellTargets_Ritual,
            ritualSpell.effect,
            ManaCore.Instance
        );
        ManaCore.Instance.castingCoroutine = StartCoroutine(ritualSpell.targetHandler.CastingSpell(ritualSpell));
    }

    /*int emptyCount = 0;

    foreach (var s in spellObjects)
    {
        if (s.GetComponent<Spell>().emptySpell)
            emptyCount++;
    }
    Debug.Log("currentSelectedSlotButton = " + currentSelectedSlotButton?.name);

    for (int i = 0; i < spellSlots.Count; i++)
    {
        Debug.Log($"Slot {i}: {spellSlots[i].name} | Equal: {spellSlots[i] == currentSelectedSlotButton}");
    }

    // Assign icon
    int spellSlotIndex = spellSlots.FindIndex(b => b == currentSelectedSlotButton);
    if (spellSlotIndex < 0 || spellSlotIndex >= spellIcons.Count)
    {
        Debug.LogError($"Invalid spellSlotIndex: {spellSlotIndex}. Check spellSlots and spellIcons setup.");
        return;
    }
    spellIcons[spellSlotIndex].sprite = spell.effect.icon;
    spellSlots[spellSlotIndex].GetComponent<Image>().sprite = spell.effect.icon; // Set the icon for the selected spell slot
    // If it's the first actual spell created, set it as the selected one
    if (emptyCount == spellObjects.Count - 1)
    {
        ManaCore.Instance.currentSpell = spell;

        // Rebuild the spell icon display so index 2 becomes the active slot
        RearrangeSpellIcons(spellSlotIndex);
    }*/




    private SpellColor SetSpellColor()
    {
        return currentSpellColor switch
        {
            SpellColors.Blue => new SpellBlue(spell),
            SpellColors.Green => new SpellGreen(spell),
            SpellColors.Black => new SpellBlack(spell),
            SpellColors.White => new SpellWhite(spell),
            _ => new SpellBrown(spell), // Default
        };
    }

    private SpellColor SetSpellColor_Ritual()
    {
        return currentSpellColor_Ritual switch
        {
            SpellColors.Blue => new SpellBlue(ritualSpell),
            SpellColors.Green => new SpellGreen(ritualSpell),
            SpellColors.Black => new SpellBlack(ritualSpell),
            SpellColors.White => new SpellWhite(ritualSpell),
            _ => new SpellBrown(ritualSpell), // Default
        };
    }

    private SpellTarget GetSpellTarget()
    {
        return currentSpellTargets switch
        {
            SpellTargets.Tile => new SpellOnTile(spell),
            SpellTargets.Room => new SpellOnRoom(spell),
            SpellTargets.Beam => new SpellBeam(spell),
            SpellTargets.Creature => new SpellOnCreature(spell),
            _ => new SpellOnTile(spell), // Default

        };
    }

    private SpellTarget GetSpellTarget_Ritual()
    {
        return currentSpellTargets_Ritual switch
        {
            SpellTargets_Ritual.Faction => new SpellOnFaction(ritualSpell),
            SpellTargets_Ritual.Dungeon => new SpellOnDungeon(ritualSpell),
            SpellTargets_Ritual.Room => new SpellOnRoomRitual(ritualSpell),
            _ => new SpellOnDungeon(ritualSpell), // Default
        };
    }


    private void SetColor(SpellColors color)
    {
        currentSpellColor = color;
    }

    private void SetColor_Ritual(SpellColors color)
    {
        currentSpellColor_Ritual = color;
    }

    private void SetTarget(SpellTargets target)
    {
        currentSpellTargets = target;
    }

    private void SetTarget_Ritual(SpellTargets_Ritual target)
    {
        currentSpellTargets_Ritual = target;
    }

    private void HighlightSelectedColor(List<Button> buttons, Button selected)
    {
        if (selected == brownButton)
        {
            selectorColor.transform.position = brownButton.transform.position; // Move selector to brown button
        }
        else if (selected == blueButton)
        {
            selectorColor.transform.position = blueButton.transform.position; // Move selector to blue button
        }
        else if (selected == greenButton)
        {
            selectorColor.transform.position = greenButton.transform.position; // Move selector to green button
        }
        else if (selected == blackButton)
        {
            selectorColor.transform.position = blackButton.transform.position; // Move selector to black button

        }
        else if (selected == whiteButton)
        {
            selectorColor.transform.position = whiteButton.transform.position; // Move selector to white button
        }
    }

    private void HighlightSelectedColor_Ritual(List<Button> buttons, Button selected)
    {
        if (selected == brownButtonRitual)
        {
            selectorColor_Ritual.transform.position = brownButtonRitual.transform.position; // Move selector to brown button
        }
        else if (selected == blueButtonRitual)
        {
            selectorColor_Ritual.transform.position = blueButtonRitual.transform.position; // Move selector to blue button
        }
        else if (selected == greenButtonRitual)
        {
            selectorColor_Ritual.transform.position = greenButtonRitual.transform.position; // Move selector to green button
        }
        else if (selected == blackButtonRitual)
        {
            selectorColor_Ritual.transform.position = blackButtonRitual.transform.position; // Move selector to black button

        }
        else if (selected == whiteButtonRitual)
        {
            selectorColor_Ritual.transform.position = whiteButtonRitual.transform.position; // Move selector to white button
        }
    }

    private void SetSelectedSpellSlot(Button selected)
    {

            spellSlotSelector.SetActive(true); // Ensure the selector is active
            var selectorRect = spellSlotSelector.GetComponent<RectTransform>();
            var buttonRect = selected.GetComponent<RectTransform>();
            selectorRect.anchoredPosition = buttonRect.anchoredPosition;
            currentSelectedSlotButton = selected; // Update the currently selected slot
            if (currentSelectedSlotButton == spellSlot1)
            {
                currentSelectedSpellObject = spell1.gameObject;
            }
            else if (currentSelectedSlotButton == spellSlot2)
            {
                currentSelectedSpellObject = spell2.gameObject;
            }
            else if (currentSelectedSlotButton == spellSlot3)
            {
                currentSelectedSpellObject = spell3.gameObject;
            }
            else if (currentSelectedSlotButton == spellSlot4)
            {
                currentSelectedSpellObject = spell4.gameObject;
            }
            else if (currentSelectedSlotButton == spellSlot5)
            {
                currentSelectedSpellObject = spell5.gameObject;
            }

    }

    public void DrawnSpellInBook()
    {
        spellSlot1.GetComponent<Image>().sprite = spell1.GetComponent<Spell>().effect.icon;
        spellSlot2.GetComponent<Image>().sprite = spell2.GetComponent<Spell>().effect.icon;
        spellSlot3.GetComponent<Image>().sprite = spell3.GetComponent<Spell>().effect.icon;
        spellSlot4.GetComponent<Image>().sprite = spell4.GetComponent<Spell>().effect.icon;
        spellSlot5.GetComponent<Image>().sprite = spell5.GetComponent<Spell>().effect.icon;
    }

    /*private void SetSelectedSpellSlot(Button selected)
{
var selectorRect = spellSlotSelector.GetComponent<RectTransform>();
var buttonRect = selected.GetComponent<RectTransform>();
selectorRect.anchoredPosition = buttonRect.anchoredPosition;

currentSelectedSlotButton = selected;

int index = spellSlots.IndexOf(selected);
if (index >= 0 && index < spellObjects.Count)
{
currentSelectedSlotObject = spellObjects[index];
}
else
{
Debug.Log("Selected button does not match any known spell slot.");
currentSelectedSlotObject = null;
}
}*/

    private void HighlightSelectedTarget(Button selected)
    {
        var selectorRect = selectorTarget.GetComponent<RectTransform>();
        var buttonRect = selected.GetComponent<RectTransform>();
        selectorRect.anchoredPosition = buttonRect.anchoredPosition;

    }

    private void HighlightSelectedTarget_Ritual(Button selected)
    {
        var selectorRect_Ritual = selectorTarget_Ritual.GetComponent<RectTransform>();
        var buttonRect_Ritual = selected.GetComponent<RectTransform>();
        selectorRect_Ritual.anchoredPosition = buttonRect_Ritual.anchoredPosition;

    }

    private SpellEffectSO PickSpellEffect()
    {
        if (currentSpellTargets == SpellTargets.Tile)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueTile;
                case SpellColors.Green:
                    return spellEffectList.greenTile;
                case SpellColors.Black:
                    return spellEffectList.blackTile;
                case SpellColors.White:
                    return spellEffectList.whiteTile;
                case SpellColors.Brown:
                    return spellEffectList.brownTile;
            }
        }
        else if (currentSpellTargets == SpellTargets.Room)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueRoom;
                case SpellColors.Green:
                    return spellEffectList.greenRoom;
                case SpellColors.Black:
                    return spellEffectList.blackRoom;
                case SpellColors.White:
                    return spellEffectList.whiteRoom;
                case SpellColors.Brown:
                    return spellEffectList.brownRoom;
            }
        }
        else if (currentSpellTargets == SpellTargets.Creature)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueCreature;
                case SpellColors.Green:
                    return spellEffectList.greenCreature;
                case SpellColors.Black:
                    return spellEffectList.blackCreature;
                case SpellColors.White:
                    return spellEffectList.whiteCreature;
                case SpellColors.Brown:
                    return spellEffectList.brownCreature;
            }

        }

        else if (currentSpellTargets == SpellTargets.Beam)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueBeam;
                case SpellColors.Green:
                    return spellEffectList.greenBeam;
                case SpellColors.Black:
                    return spellEffectList.blackBeam;
                case SpellColors.White:
                    return spellEffectList.whiteBeam;
                case SpellColors.Brown:
                    return spellEffectList.brownBeam;
            }
        }

        return null;
    }

    public SpellEffectSO PickSpellEffect_Ritual()
    {
        if (currentSpellTargets_Ritual == SpellTargets_Ritual.Faction)
        {

            switch (currentSpellColor_Ritual)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueFaction;
                case SpellColors.Green:
                    return spellEffectList.greenFaction;
                case SpellColors.Black:
                    return spellEffectList.blackFaction;
                case SpellColors.White:
                    return spellEffectList.whiteFaction;
                case SpellColors.Brown:
                    return spellEffectList.brownFaction;
            }
        }
        else if (currentSpellTargets_Ritual == SpellTargets_Ritual.Dungeon)
        {
            switch (currentSpellColor_Ritual)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueDungeon;
                case SpellColors.Green:
                    return spellEffectList.greenDungeon;
                case SpellColors.Black:
                    return spellEffectList.blackDungeon;
                case SpellColors.White:
                    return spellEffectList.whiteDungeon;
                case SpellColors.Brown:
                    return spellEffectList.brownDungeon;
            }
        }
        else if (currentSpellTargets_Ritual == SpellTargets_Ritual.Room)
        {
            switch (currentSpellColor_Ritual)
            {
                case SpellColors.Blue:
                    return spellEffectList.blueRoomRitual;
                case SpellColors.Green:
                    return spellEffectList.greenRoomRitual;
                case SpellColors.Black:
                    return spellEffectList.blackRoomRitual;
                case SpellColors.White:
                    return spellEffectList.whiteRoomRitual;
                case SpellColors.Brown:
                    return spellEffectList.brownRoomRitual;
            }
        }

        return null;
    }


    public void UpdateSpellIcon()
    {

        if (currentSpellTargets == SpellTargets.Tile)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    currentSpellIcon.sprite = spellEffectList.blueTile.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon.sprite = spellEffectList.greenTile.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon.sprite = spellEffectList.blackTile.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon.sprite = spellEffectList.whiteTile.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon.sprite = spellEffectList.brownTile.icon;
                    break;
            }
        }
        else if (currentSpellTargets == SpellTargets.Room)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    currentSpellIcon.sprite = spellEffectList.blueRoom.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon.sprite = spellEffectList.greenRoom.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon.sprite = spellEffectList.blackRoom.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon.sprite = spellEffectList.whiteRoom.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon.sprite = spellEffectList.brownRoom.icon;
                    break;
            }
        }
        else if (currentSpellTargets == SpellTargets.Creature)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    currentSpellIcon.sprite = spellEffectList.blueCreature.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon.sprite = spellEffectList.greenCreature.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon.sprite = spellEffectList.blackCreature.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon.sprite = spellEffectList.whiteCreature.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon.sprite = spellEffectList.brownCreature.icon;
                    break;
            }
        }
        else if (currentSpellTargets == SpellTargets.Beam)
        {
            switch (currentSpellColor)
            {
                case SpellColors.Blue:
                    currentSpellIcon.sprite = spellEffectList.blueBeam.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon.sprite = spellEffectList.greenBeam.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon.sprite = spellEffectList.blackBeam.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon.sprite = spellEffectList.whiteBeam.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon.sprite = spellEffectList.brownBeam.icon;
                    break;
            }
        }


    }

    private void UpdateSpellIcon_Ritual()
    {
        if (currentSpellTargets_Ritual == SpellTargets_Ritual.Faction)
        {
            switch (currentSpellColor_Ritual)
            {
                case SpellColors.Blue:
                    currentSpellIcon_Ritual.sprite = spellEffectList.blueFaction.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon_Ritual.sprite = spellEffectList.greenFaction.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon_Ritual.sprite = spellEffectList.blackFaction.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon_Ritual.sprite = spellEffectList.whiteFaction.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon_Ritual.sprite = spellEffectList.brownFaction.icon;
                    break;
            }
        }
        else if (currentSpellTargets_Ritual == SpellTargets_Ritual.Dungeon)
        {
            switch (currentSpellColor_Ritual)
            {
                case SpellColors.Blue:
                    currentSpellIcon_Ritual.sprite = spellEffectList.blueDungeon.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon_Ritual.sprite = spellEffectList.greenDungeon.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon_Ritual.sprite = spellEffectList.blackDungeon.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon_Ritual.sprite = spellEffectList.whiteDungeon.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon_Ritual.sprite = spellEffectList.brownDungeon.icon;
                    break;
            }
        }
        else if (currentSpellTargets_Ritual == SpellTargets_Ritual.Room)
        {
            switch (currentSpellColor_Ritual)
            {
                case SpellColors.Blue:
                    currentSpellIcon_Ritual.sprite = spellEffectList.blueRoomRitual.icon;
                    break;
                case SpellColors.Green:
                    currentSpellIcon_Ritual.sprite = spellEffectList.greenRoomRitual.icon;
                    break;
                case SpellColors.Black:
                    currentSpellIcon_Ritual.sprite = spellEffectList.blackRoomRitual.icon;
                    break;
                case SpellColors.White:
                    currentSpellIcon_Ritual.sprite = spellEffectList.whiteRoomRitual.icon;
                    break;
                case SpellColors.Brown:
                    currentSpellIcon_Ritual.sprite = spellEffectList.brownRoomRitual.icon;
                    break;
            }
        }
    }


    private void ButtonClicked(Button button, List<Button> buttons, SpellColors color, SpellTargets target)
    {
        if (buttons == colorButtons)
        {
            SetColor(color);
            HighlightSelectedColor(colorButtons, button);
        }
        else if (buttons == targetButtons)
        {
            SetTarget(target);
            HighlightSelectedTarget(button);
        }

        currentSpellColor = color;
        currentSpellTargets = target;

        UpdateSpellIcon();


    }

    private void ButtonClicked_Ritual(Button button, List<Button> buttons, SpellColors color, SpellTargets_Ritual target)
    {
        if (buttons == colorButtons_Ritual)
        {
            SetColor_Ritual(color);
            HighlightSelectedColor_Ritual(colorButtons_Ritual, button);
        }
        else if (buttons == targetButtons_Ritual)
        {
            SetTarget_Ritual(target);
            HighlightSelectedTarget_Ritual(button);
        }

        currentSpellColor_Ritual = color;
        currentSpellTargets_Ritual = target;

        UpdateSpellIcon_Ritual();


    }


    private void powerButtonClicked(Button button)
    {
        if (button == powerPLUSbutton && spellPowerLVL < 3)
        {
            spellPowerLVL++;
        }
        else if (button == powerMINUSbutton && spellPowerLVL > 0)
        {
            spellPowerLVL--;
        }
        spellPowerText.text = spellPowerLVL.ToString();

        if (button == rangePLUSbutton && spellRangeLVL < 3)
        {
            spellRangeLVL++;
        }
        else if (button == rangeMINUSbutton && spellRangeLVL > 0)
        {
            spellRangeLVL--;
        }
        spellRangeText.text = spellRangeLVL.ToString();
        float _manaCost = spellEffect.manacost * (spellPowerLVL * 0.75f) * (spellRangeLVL * 0.8f);
        manaCost = Mathf.RoundToInt(_manaCost);

        manaCostText.text = manaCost.ToString();

    }

    private void powerButtonClicked_Ritual(Button button)
    {
        if (button == powerPLUSbutton_Ritual && spellPowerLVL < 3)
        {
            spellPowerLVL_Ritual++;
        }
        else if (button == powerMINUSbutton_Ritual && spellPowerLVL > 0)
        {
            spellPowerLVL_Ritual--;
        }
        spellPowerText_Ritual.text = spellPowerLVL_Ritual.ToString();

        if (button == durationPLUSbutton_Ritual && spellDurationLVL_Ritual < 3)
        {
            spellDurationLVL_Ritual++;
        }
        else if (button == durationMINUSbutton_Ritual && spellDurationLVL_Ritual > 0)
        {
            spellDurationLVL_Ritual--;
        }
        spellDurationText_Ritual.text = spellDurationLVL_Ritual.ToString();
        float _manaCost_Ritual = spellEffectRitual.manacost * (spellPowerLVL_Ritual * 0.75f) * (spellDurationLVL_Ritual * 0.8f);
        manaCost_Ritual = Mathf.RoundToInt(_manaCost_Ritual);

        manaCostText_Ritual.text = manaCost_Ritual.ToString();

    }

    public void OpenRitualCanvas()
    {
        RitualOnFactionCanvas.enabled = true;
        RitualOnFactionCanvas.gameObject.SetActive(true);
    }



    public void BoolBookOpen()
    {
       isSpellBookOpen = !isSpellBookOpen;
    }
}

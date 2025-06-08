using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpellBuilder : MonoBehaviour
{
    public static SpellBuilder Instance;
    public GameObject spellPrefab;
    public SpellColor spellColor;
    public SpellTarget spellTarget;
    public Spell spell;
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
    public enum SpellTargets
    {
        Tile,
        Room,
        Creature,
        Faction,
        Beam,
        Dungeon
    }
    public SpellTargets currentSpellTargets;
    public Button tileTargetButton, roomTargetButton, beamTargetButton, creatureTargetButton, factionTargetButton, dungeonTargetButton,
    blueButton, greenButton, blackButton, whiteButton, brownButton, buildButton;
    private List<Button> colorButtons = new List<Button>();
    private List<Button> targetButtons = new List<Button>();
    public Color defaultColor = Color.white;

    public Text spellNameText;
    public Text spellColorText;
    public Text spellTargetText;
    public Text spellPowerText;
    public Text spallPowerLVL;
    public Text spellRangeLVL;
    public Text spellDurationLVL;

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
        colorButtons = new List<Button> { blueButton, greenButton, blackButton, whiteButton, brownButton };
        targetButtons = new List<Button> { tileTargetButton, roomTargetButton, beamTargetButton, creatureTargetButton, factionTargetButton, dungeonTargetButton };

        blueButton.onClick.AddListener(() => SetColor(SpellColors.Blue)); HighlightSelectedColor(colorButtons, blueButton);
        greenButton.onClick.AddListener(() => SetColor(SpellColors.Green)); HighlightSelectedColor(colorButtons, greenButton);
        blackButton.onClick.AddListener(() => SetColor(SpellColors.Black)); HighlightSelectedColor(colorButtons, blackButton);
        whiteButton.onClick.AddListener(() => SetColor(SpellColors.White)); HighlightSelectedColor(colorButtons, whiteButton);
        brownButton.onClick.AddListener(() => SetColor(SpellColors.Brown)); HighlightSelectedColor(colorButtons, brownButton);

        tileTargetButton.onClick.AddListener(() => SetTarget(SpellTargets.Tile)); HighlightSelectedTarget(targetButtons, tileTargetButton);
        roomTargetButton.onClick.AddListener(() => SetTarget(SpellTargets.Room)); HighlightSelectedTarget(targetButtons, roomTargetButton);
        beamTargetButton.onClick.AddListener(() => SetTarget(SpellTargets.Beam)); HighlightSelectedTarget(targetButtons, beamTargetButton);
        creatureTargetButton.onClick.AddListener(() => SetTarget(SpellTargets.Creature)); HighlightSelectedTarget(targetButtons, creatureTargetButton);
        factionTargetButton.onClick.AddListener(() => SetTarget(SpellTargets.Faction)); HighlightSelectedTarget(targetButtons, factionTargetButton);
        dungeonTargetButton.onClick.AddListener(() => SetTarget(SpellTargets.Dungeon)); HighlightSelectedTarget(targetButtons, dungeonTargetButton);
    }

    public void CreateSpell()
    {

        spell = Instantiate(spellPrefab).GetComponent<Spell>();
        spell.transform.SetParent(ManaCore.Instance.transform);
        spell.transform.localPosition = Vector3.zero; // Reset position to ManaCore's position
        spell.colorHandler = SetSpellColor();
        spell.targetHandler = GetSpellTarget();
        spell.effect = PickSpellEffect();
        spell.manaCore = ManaCore.Instance;
        ManaCore.Instance.spellObjects.Add(spell.gameObject);
        if (ManaCore.Instance.currentSpell != null)
        {
            ManaCore.Instance.currentSpell = spell;
        }
    }

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

    private SpellTarget GetSpellTarget()
    {
        return currentSpellTargets switch
        {
            SpellTargets.Tile => new SpellOnTile(spell),
            SpellTargets.Room => new SpellOnRoom(spell),
            SpellTargets.Beam => new SpellBeam(spell),
            SpellTargets.Creature => new SpellOnCreature(spell),
            SpellTargets.Faction => new SpellOnFaction(spell),
            _ => new SpellOnDungeon(spell), // Default
        };
    }


    private void SetColor(SpellColors color)
    {
        currentSpellColor = color;
    }

    private void SetTarget(SpellTargets target)
    {
        currentSpellTargets = target;
    }

    private void HighlightSelectedColor(List<Button> buttons, Button selected)
    {
        foreach (var btn in buttons)
        {
            btn.image.color = defaultColor;
        }
        if (selected = brownButton)
        {
            selected.image.color = Color.yellow; // Highlight color for brown
        }
        else if (selected == blueButton)
        {
            selected.image.color = Color.blue; // Highlight color for blue
        }
        else if (selected == greenButton)
        {
            selected.image.color = Color.green; // Highlight color for green
        }
        else if (selected == blackButton)
        {
            selected.image.color = Color.black; // Highlight color for black
        }
        else if (selected == whiteButton)
        {
            selected.image.color = Color.white; // Highlight color for white
        }
    }
    private void HighlightSelectedTarget(List<Button> buttons, Button selected)
    {
        foreach (var btn in buttons)
        {
            btn.image.color = defaultColor;
        }
        if (selected)
        {
            selected.image.color = Color.orange; // Highlight color for tile
        }
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
        else if (currentSpellTargets == SpellTargets.Faction)
        {
            
            switch (currentSpellColor)
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
        else
            switch (currentSpellColor)
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

        return null;
    }
}

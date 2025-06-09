using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class AvatarGenerator : MonoBehaviour
{
    AvatarGenerator Instance;
    public enum SpellColors
    {
        Brown,
        Blue,
        Green,
        White,
        Black
    }
    public SpellColors currentSpellColor;
    public Color defaultColor = Color.white;
    public Spell AvatarSpellPrefab;
    public Spell avatarSpell;
    public SpellColor colorHandler;

    public SpellEffectSO effect;
    public ManaCore manaCore;
    public SpellEffectSO spellEffectSO;

    public Button blueButton, greenButton, blackButton, whiteButton, brownButton, summondButton;
    private List<Button> colorButtons = new List<Button>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        manaCore = ManaCore.Instance;
        colorButtons = new List<Button> { blueButton, greenButton, blackButton, whiteButton, brownButton };

        blueButton.onClick.AddListener(() => SetColor(SpellColors.Blue)); HighlightSelectedColor(colorButtons, blueButton);
        greenButton.onClick.AddListener(() => SetColor(SpellColors.Green)); HighlightSelectedColor(colorButtons, greenButton);
        blackButton.onClick.AddListener(() => SetColor(SpellColors.Black)); HighlightSelectedColor(colorButtons, blackButton);
        whiteButton.onClick.AddListener(() => SetColor(SpellColors.White)); HighlightSelectedColor(colorButtons, whiteButton);
        brownButton.onClick.AddListener(() => SetColor(SpellColors.Brown)); HighlightSelectedColor(colorButtons, brownButton);
    }

    // Update is called once per frame
    void Update()
    {

    }


        public void SummonAvatar()
    {
        avatarSpell = Instantiate(AvatarSpellPrefab).GetComponent<Spell>();
        avatarSpell.transform.SetParent(manaCore.transform);
        avatarSpell.transform.localPosition = Vector3.zero;
        avatarSpell.colorHandler = SetSpellColor();
    }
    private SpellColor SetSpellColor()
    {
        return currentSpellColor switch
        {
            SpellColors.Blue => new SpellBlue(avatarSpell),
            SpellColors.Green => new SpellGreen(avatarSpell),
            SpellColors.Black => new SpellBlack(avatarSpell),
            SpellColors.White => new SpellWhite(avatarSpell),
            SpellColors.Brown => new SpellBrown(avatarSpell),
            _ => new SpellBrown(avatarSpell), // Default
        };
    }

    


    private void SetColor(SpellColors color)
    {
        currentSpellColor = color;
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

}

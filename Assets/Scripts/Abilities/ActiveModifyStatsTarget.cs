using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Animations;


[CreateAssetMenu(menuName = "Card Abilities Active/ModifyStatsTarget")]

public class ActiveModifyStatsTarget : CardAbility, IActiveAbility
{
    public int modification_health;
    public int modification_attack;
    public string targetName;//any
    public int targetLevel; //-1
    public bool onlyTargetFriends = false;
    public bool canIncreaseHealthAboveOriginal = false;
    private Game game;
    private GameObject card_self;

    public void Activate(GameObject _card_self, Game _game)
    {
        game = _game;
        card_self = _card_self;
        Color purple = new Color(128f / 255f, 0f, 128f / 255f, 0.5f);
        Camera.main.backgroundColor = purple;
        game.StartCoroutine(game.TargetSelection(Apply));

    }
    public void ai_Activate(GameObject _card_self, Game _game)
    {
        game = _game;
        card_self = _card_self;
        //target selection
        GameObject target = null;
        if (onlyTargetFriends == true)
        {
            foreach (GameObject card in game.player2.Field.ToList())
            {
                CardProperties props = card.GetComponent<CardProperties>();
                if (CheckTarget(props) && card != card_self)
                {
                    if (canIncreaseHealthAboveOriginal == false)
                    {
                        if (props.life < props.originalLife) target = card;
                    }
                    else target = card;
                }
            }
        }
        else
        {
            foreach (GameObject card in game.player1.Field.ToList())
            {
                CardProperties props = card.GetComponent<CardProperties>();
                if (CheckTarget(props))
                {
                    target = card;
                }
            }
        }

        Apply(target);
        if(target != null)
            Debug.Log(target.name + "Health modified");
    }

    private void Apply(GameObject selectedCard)
    {

        if (selectedCard == null)
        {
            game.infoBox.GetComponent<InfoBox>().DisplayText("target not found");
            Camera.main.backgroundColor = game.originalBackgroundColor;
            Debug.Log("Ability cancalled");
            return;
        }
        CardProperties props = selectedCard.GetComponent<CardProperties>();

        if (!CheckTarget(props) || card_self == selectedCard)
        {
            game.infoBox.GetComponent<InfoBox>().DisplayText("target does not match");
            Camera.main.backgroundColor = game.originalBackgroundColor;
            Debug.Log("friend doesnt meet requirements");
            return;
        }

        props.life += modification_health;
        if (canIncreaseHealthAboveOriginal == false && props.life >= props.originalLife) props.ResetLife();

        props.attack += modification_attack;

        game.infoBox.GetComponent<InfoBox>().DisplayText("target health modified");
        card_self.GetComponent<CardProperties>().active_ability_UsesPerTurn -= 1;
        Camera.main.backgroundColor = game.originalBackgroundColor;
    }
    
    private bool CheckTarget(CardProperties prop)
    {
        if (onlyTargetFriends)
            if (game.activePlayer.Field.Contains(prop.gameObject) == false)
                return false;


        if (targetName == "any")
        {
            if (targetLevel == -1) return true;
        }
        else if (prop.cardName == targetName)
        {
            return true;
        }
        else if (prop.level == targetLevel)
        {
            return true;
        }

        return false;

    }
    
}
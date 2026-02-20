using UnityEngine;

[CreateAssetMenu(menuName = "Card Abilities Passive/ModifyOwnAttackOnFieldFriend")]
public class ModifyOwnAttackOnFieldFriend : CardAbility, IOnFieldFriend
{
    public int modification;
    public string friendName;//any
    public int friendLevel; //-1


    public void OnFieldFriend(GameObject card, Game game, GameObject friend, Player player)
    {
        CardProperties props_this_card = card.GetComponent<CardProperties>();

        int count = 0;
        foreach (GameObject other in player.Field)
        {
            if (other == card) continue;

            CardProperties props = other.GetComponent<CardProperties>();
            if (CheckFriend(props))
            {
                count++;
            }
        }

        int diff = count - props_this_card.counter;
        props_this_card.attack += diff * modification;
        props_this_card.counter = count;

    }
    private bool CheckFriend(CardProperties prop)
    {
        if (friendName == "any")
        {
            if (friendLevel == -1) return true;
        }
        else if (prop.cardName == friendName)
        {
            return true;
        }
        else if (prop.level == friendLevel)
        {
            return true;
        }

        return false;
        
    }


}
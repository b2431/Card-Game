using UnityEngine;

[CreateAssetMenu(menuName = "Card Abilities Passive/ModifyOwnStatsOnKill")]
public class ModifyOwnStatsOnKill : CardAbility, IOnKill
{
    public int modification_attack;
    public int modification_health;


    public void OnKill(GameObject card_self, Game game)
    {
        CardProperties props = card_self.GetComponent<CardProperties>();
        props.attack += modification_attack;
        props.life += modification_health;
    }

}


using System.Collections.Generic;
using UnityEngine;

public class CardProperties : MonoBehaviour
{
    [Header("Properties")]
    public string cardName;
    public int level;
    public int cost;
    public int life;
    public int attack;
    public int attackRange; //1 or 2 or 3
    public int attacksPerTurn = 1;
    public int active_ability_UsesPerTurn = 1;

    public int card_type; // 0: normal, 1: power orb

    public string passive_ability_text;
    public string active_ability_text;

    public CardAbility passive_ability;
    public CardAbility active_ability;

    [Header("Debug")]
    public int originalLife;
    public int originalAttack;
    private int originalAPT;
    private int original_active_ability_UsesPerTurn;


    //since the same CardAbility script is shared among all colones of a card these are variables to use as individual counters (for example the wolfs)
    public int counter = 0; //until I come up with something better

    void Awake()
    {
        originalLife = life;
        originalAttack = attack;
        originalAPT = attacksPerTurn;
        original_active_ability_UsesPerTurn = active_ability_UsesPerTurn;
    }

    public void ResetAPT()
    {
        attacksPerTurn = originalAPT;
    }
    public void ResetLife()
    {
        life = originalLife;
    }
    public void ResetAttack()
    {
        attack = originalAttack;
    }
    public void ResetActiveAbilityUsesPerTurn()
    {
        active_ability_UsesPerTurn = original_active_ability_UsesPerTurn;
    }


}

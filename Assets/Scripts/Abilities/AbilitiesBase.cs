using UnityEngine;


public abstract class CardAbility : ScriptableObject
{

}


/// for all active abilities on button press
public interface IActiveAbility
{
    void Activate(GameObject card_self, Game game);
    void ai_Activate(GameObject card_self, Game game);
}


/// for passive abilities
//birth
public interface IOnSummonSelf
{
    void OnSummon(GameObject card_self, Game game);
}

public interface IOnSummonFriend
{
    void OnSummonFriend(GameObject card_self, Game game, GameObject friend);
}
public interface IOnSummonOpp
{
    void OnSummonOpp(GameObject card_self, Game game, GameObject opp);
}
//existence
public interface IOnFieldFriend
{
    void OnFieldFriend(GameObject card_self, Game game, GameObject friend, Player player);
     //void OffFieldFriend(GameObject card_self, Game game, GameObject friend);
}
public interface IOnFieldOpp
{
    void OnFieldOpp(GameObject card_self, Game game, GameObject opp, Player player);
    //void OffFieldOpp(GameObject card_self, Game game, GameObject opp);
}

//death
public interface IOnDeathSelf
{
    void OnDeath(GameObject card_self, Game game);
}
public interface IOnDeathFriend
{
    void OnDeathFriend(GameObject card_self, Game game);
}
public interface IOnDeathOpp
{
    void OnDeathOpp(GameObject card_self, Game game);
}


public interface IOnTurnStart
{
    void OnTurnStart(GameObject card_self, Game game);
}
public interface IOnKill
{
    void OnKill(GameObject card_self, Game game);
}








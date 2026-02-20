using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    
    public static bool ai_game;

    public void StartLokalGame()
    {
        ai_game = false;
        SceneManager.LoadScene("Main");
    }
    public void StartGameAgainsAi()
    {
        ai_game = true;
        SceneManager.LoadScene("Main");
    }
}

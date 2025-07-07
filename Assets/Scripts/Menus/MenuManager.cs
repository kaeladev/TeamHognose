using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static string MenuSceneName = "PB_MainMenu";
    public static string CreditsSceneName = "PB_Credits";
    public static string BakerySceneName = "PB_Bakery";
    public static string BreakroomSceneName = "KO_NarrativeGym";
    public static void LoadMainMenuScene()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public static void LoadCreditsScene()
    {
        SceneManager.LoadScene(CreditsSceneName);
    }

    public static void LoadBakeryScene()
    {
        SceneManager.LoadScene(BakerySceneName);
    }

    public static void LoadBreakroomScene()
    {
        SceneManager.LoadScene(BreakroomSceneName);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}

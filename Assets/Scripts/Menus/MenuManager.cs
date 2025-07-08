using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static string MenuSceneName = "PB_MainMenu";
    public static string CreditsSceneName = "PB_Credits";
    public static string BakerySceneName = "PB_Bakery";
    public static string BreakroomSceneName = "PB_Breakroom";

    private bool MenuOpen = false;

    private void Update()
    {
        if (!MenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Canvas MenuCanvas = GetComponentInChildren<Canvas>(true);
            if (MenuCanvas)
            {
                MenuOpen = true;
                MenuCanvas.gameObject.SetActive(true);
                MenuManager.PauseGame();
            }
        }
        else if (MenuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        Canvas MenuCanvas = GetComponentInChildren<Canvas>(true);
        if (MenuCanvas)
        {
            MenuOpen = false;
            MenuCanvas.gameObject.SetActive(false);
            MenuManager.ResumeGame();
        }
    }

    public static void LoadSceneByName(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    public static void LoadMainMenuScene()
    {
        ResumeGame();
        if (StorySceneManager.PersistentStoryInstance)
        {
            StorySceneManager.PersistentStoryInstance.CleanUpScene();
        }
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

    public static void LoadBreakroomScene(bool RestartGame = false)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (RestartGame && StorySceneManager.PersistentStoryInstance)
        {
            StorySceneManager.PersistentStoryInstance.ResetForNewGame();
        }

        SceneManager.LoadScene(BreakroomSceneName);
    }

    public static void PauseGame()
    {
        Time.timeScale = 0;
    }

    public static void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public static bool IsGamePaused()
    {
        return Time.timeScale == 0;
    }

    public static void QuitGame()
    {
        Application.Quit();
    }

    public static void UpdateCursor(Texture2D CursorTexture = null)
    {
        if (CursorTexture != null && IsGamePaused())
        {
            return;
        }
        Cursor.SetCursor(CursorTexture, Vector2.zero, CursorMode.Auto);
    }
}

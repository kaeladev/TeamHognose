using UnityEngine;

public class CreditsMenu : MonoBehaviour
{
   void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MenuManager.LoadMainMenuScene();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuManager.QuitGame();
        }
    }
}

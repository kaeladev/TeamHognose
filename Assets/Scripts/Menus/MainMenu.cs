using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button PlayButton;
    public Button CreditsButton;
    public Button QuitButton;

    void Start()
    {
       PlayButton.onClick.AddListener(delegate {
           PlayGame();
       });

        CreditsButton.onClick.AddListener(delegate {
            SeeCredits();
        });

        QuitButton.onClick.AddListener(delegate {
            QuitGame();
        });
    }

    void PlayGame()
    {
        MenuManager.LoadBreakroomScene(true);
    }

    void SeeCredits()
    {
        MenuManager.LoadCreditsScene();
    }

    void QuitGame()
    {
        MenuManager.QuitGame();
    }
}

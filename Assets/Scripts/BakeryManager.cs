using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The bakery manager exists to be a conduit between work station & user,
// As well as allow Inky and Yuzu to interact with the scene without a work station
public class BakeryManager : MonoBehaviour
{
    public static BakeryManager CurrentBakeryInstance;

    public Texture2D HoverCursorTexture = null;

    [HideInInspector]
    public int StoredIngredients = 1;

    private Canvas SceneTransitionCanvas;
    private string PromptedLoadScene;
    private MB_WorkStation[] WorkStations;
    private MB_NPCBehavior[] WorkingNPCs;
    public MB_NPCBehavior_Inky Inky;

    void Start()
    {
        CurrentBakeryInstance = this;

        WorkStations = GetComponentsInChildren<MB_WorkStation>();
        WorkingNPCs = GetComponentsInChildren<MB_NPCBehavior>();

        SceneTransitionCanvas = Camera.main.GetComponentInChildren<Canvas>(true);
        SceneTransitionCanvas.gameObject.SetActive(false);

        foreach (MB_NPCBehavior NPC in WorkingNPCs)
        {
            NPC.ProductionComplete.AddListener(Inky.OnInkyFetch);

            foreach (MB_WorkStation WorkStation in WorkStations)
            {
                if (NPC.tag == WorkStation.tag)
                {
                    MB_NPCBehavior_Work Worker = (MB_NPCBehavior_Work)NPC;
                    WorkStation.NPC = Worker;
                    Worker.WorkStation = WorkStation;
                }
            }
        }
    }

    private void Update()
    {
        if (StorySceneManager.PersistentStoryInstance)
        {
            StorySceneManager.PersistentStoryInstance.TickBakeryTime();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PromptMenuScene();
        }
    }

    public bool GrabDoughBall()
    {
        foreach (MB_WorkStation WorkStation in WorkStations)
        {
            if (WorkStation.tag == "Squilliam")
            {
                return WorkStation.TakeAmount(0.5f);
            }
        }
        return false;
    }

    public int GrabIngredients()
    {
        if (StoredIngredients == 0)
        {
            return 0;
        }

        if (StoredIngredients > 1)
        {
            StoredIngredients -= 2;
            return 2;
        }

        StoredIngredients--;
        return 1;
    }

    protected void OnMouseEnter()
    {
        MenuManager.UpdateCursor(HoverCursorTexture);
    }

    protected void OnMouseExit()
    {
        MenuManager.UpdateCursor(null);
    }

    void OnMouseOver()  // Add collision to an exit door; for skipping work/speedrunning story
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (MB_WorkStation WorkStation in WorkStations)
            {
                if (WorkStation.gameObject.tag == "Tortilla")
                {
                    MB_WorkStation_Tortilla TortillaStation = (MB_WorkStation_Tortilla)WorkStation;
                    if (TortillaStation.IsShiftComplete)
                    {
                        MenuManager.LoadBreakroomScene();
                        return;
                    }
                }
            }
            PromptBreakroomScene();
        }
    }

    public void ConfirmSceneTransition()
    {
        MenuManager.LoadSceneByName(PromptedLoadScene);
        MenuManager.ResumeGame();
    }

    public void CancelSceneTransition()
    {
        SceneTransitionCanvas.gameObject.SetActive(false);
        PromptedLoadScene = "";
        MenuManager.ResumeGame();
    }

    void PromptMenuScene()
    {
        PromptedLoadScene = MenuManager.MenuSceneName;
        SceneTransitionCanvas.gameObject.SetActive(true);
        MenuManager.PauseGame();
        TextMeshProUGUI PromptText = SceneTransitionCanvas.GetComponentInChildren<TextMeshProUGUI>();
        PromptText.text = "Go to Main Menu?\n...the game will end...";
    }

    void PromptBreakroomScene()
    {
        PromptedLoadScene = MenuManager.BreakroomSceneName;
        SceneTransitionCanvas.gameObject.SetActive(true);
        MenuManager.PauseGame();
        TextMeshProUGUI PromptText = SceneTransitionCanvas.GetComponentInChildren<TextMeshProUGUI>();
        PromptText.text = "Go to Breakroom?\n...the day will end...";
    }
}

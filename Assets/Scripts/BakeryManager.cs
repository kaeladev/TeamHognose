using UnityEngine;
using UnityEngine.SceneManagement;

// The bakery manager exists to be a conduit between work station & user,
// As well as allow Inky and Yuzu to interact with the scene without a work station
public class BakeryManager : MonoBehaviour
{
    public static BakeryManager CurrentBakeryInstance;

    [HideInInspector]
    public float StoredIngredients = 1;

    private MB_WorkStation[] WorkStations;
    private MB_NPCBehavior[] WorkingNPCs;
    public MB_NPCBehavior_Inky Inky;

    void Start()
    {
        CurrentBakeryInstance = this;

        WorkStations = GetComponentsInChildren<MB_WorkStation>();
        WorkingNPCs = GetComponentsInChildren<MB_NPCBehavior>();

        foreach (MB_NPCBehavior NPC in WorkingNPCs)
        {
            NPC.ProductionComplete.AddListener(Inky.OnInkyFetch);

            foreach (MB_WorkStation WorkStation in WorkStations)
            {
                if (NPC.tag == WorkStation.tag)
                {
                    WorkStation.NPC = (MB_NPCBehavior_Work)NPC;
                }
            }
        }
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

    void OnMouseOver()  // Add collision to an exit door; for skipping work/speedrunning story
    {
        // TODO: Add confirmation display first
        if (Input.GetMouseButtonDown(0))
        {
            MenuManager.LoadBreakroomScene();
        }
    }
}

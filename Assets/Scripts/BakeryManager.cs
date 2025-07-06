using UnityEngine;
using UnityEngine.SceneManagement;

// The bakery manager exists to be a conduit between work station & user,
// As well as allow Inky and Yuzu to interact with the scene without a work station
public class BakeryManager : MonoBehaviour
{
    public static BakeryManager CurrentBakeryInstance;

    // TODO: Eventually the trigger for the workday ending should be the final customer getting served
    public int CustomersPerDay = 3;
    public Vector2 TimeRangeBetweenCustomerEntry;
    public string StorySceneName;

    private int CustomersSeenToday = 0;
    private int CustomersServedToday = 0;
    private float TimeUntilNextCustomer;

    [HideInInspector]
    public float StoredIngredients = 1;

    private MB_WorkStation[] WorkStations;
    private MB_NPCBehavior[] WorkingNPCs;
    public MB_NPCBehavior_Inky Inky;

    void Start()
    {
        CurrentBakeryInstance = this;

        if (TimeRangeBetweenCustomerEntry == Vector2.zero)
        {
            TimeRangeBetweenCustomerEntry = new Vector2(5, 10);
        }

        WorkStations = GetComponentsInChildren<MB_WorkStation>();
        WorkingNPCs = GetComponentsInChildren<MB_NPCBehavior>();

        RandomizeTimeForNextCustomerEntry();
        Debug.Log("First Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");

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

    void Update()
    {
        if (CustomersSeenToday < CustomersPerDay)
        {
            TimeUntilNextCustomer -= Time.deltaTime;
            if (TimeUntilNextCustomer < 0)
            {
                CustomersSeenToday++;
                Debug.Log("Customer Arrived: " + CustomersSeenToday.ToString() + "/" + CustomersPerDay.ToString());

                if (CustomersSeenToday < CustomersPerDay)
                {
                    RandomizeTimeForNextCustomerEntry();
                    Debug.Log("Next Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");
                }
                // Spawn/show another customer in Tortilla's queue line
            }
        }

        if (CustomersServedToday == CustomersPerDay)
        {
            GoToStory();
        }

        UpdateWorkStationStatuses();
    }

    void UpdateWorkStationStatuses()
    {
        /*
         * Pseudocode babyyyy
         * So squilliam can only work if ingredients are in mixer bowl
         * while squilliam is working and work station is not active,
         * squilliam is querying if dependency is met;
         * then the work station can active if grabbing able to grab 1 ingredient
         * activating squilliam station = -1 ingredient
         * Ingredients last for 10 seconds then is depleted
         * 
         * Lil Soup can only work if can grab dough from mixer bowl
         * the swap just happens at 50%, its just a visual swap and anim swap
         */
       // if ()
       // { 
       // }

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
        if (Input.GetMouseButtonDown(0))
        {
            GoToStory();
        }
    }

    void RandomizeTimeForNextCustomerEntry()
    {
        TimeUntilNextCustomer = Random.Range(TimeRangeBetweenCustomerEntry.x, TimeRangeBetweenCustomerEntry.y);
    }

    void GoToStory()
    {
        // TODO: Async load? Or fake loading screen for fun?
        SceneManager.LoadScene(StorySceneName);
    }
}

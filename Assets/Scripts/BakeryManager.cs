using UnityEngine;
using UnityEngine.SceneManagement;

public class BakeryManager : MonoBehaviour
{
    // TODO: Eventually the trigger for the workday ending should be the final customer getting served
    public int CustomersPerDay = 3;
    public Vector2 TimeRangeBetweenCustomerEntry;
    public string StorySceneName;

    private int CustomersSeenToday = 0;
    private int CustomersServedToday = 0;
    private float TimeUntilNextCustomer;

    void Start()
    {
        if (TimeRangeBetweenCustomerEntry == Vector2.zero)
        {
            TimeRangeBetweenCustomerEntry = new Vector2(5, 10);
        }
       
        RandomizeTimeForNextCustomerEntry();
        Debug.Log("First Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");
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

using TMPro;
using UnityEngine;

public class MB_WorkStation_Tortilla : MB_WorkStation
{
    public float TimeToServeCusomter = 3.0f;
    public int CustomersPerDay = 3;
    public Vector2 TimeRangeBetweenCustomerEntry;

    private int CustomersSeenToday = 0;
    private int CustomersServedToday = 0;
    private float TimeUntilNextCustomer;
    private float TimeUntilCurrentCustomerServed;

    public override bool CanWork()
    {
        return base.CanWork() && IsCustomerQueued();
    }

    void Start()
    {
        TimeUntilCurrentCustomerServed = TimeToServeCusomter;

        if (TimeRangeBetweenCustomerEntry == Vector2.zero)
        {
            TimeRangeBetweenCustomerEntry = new Vector2(5, 10);
        }

        RandomizeTimeForNextCustomerEntry();
        Debug.Log("First Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");
    }

    public override void Update()
    {
        base.Update();
    }

    protected override void UpdateProduction()
    {
        // TODO: Can Tortilla only fill orders if there has been a recent batch finished by Lil Soup???
        if (IsMakingProgress())
        {
            TimeUntilCurrentCustomerServed -= Time.deltaTime;

            if (TimeUntilCurrentCustomerServed < 0)
            {
                TimeUntilCurrentCustomerServed = TimeToServeCusomter;
                CustomersServedToday++;
                Debug.Log("Customer #" + CustomersServedToday + " Served!");
            }
        }

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
                // TODO: Spawn/show another customer in Tortilla's queue line
            }
        }

        if (CustomersServedToday == CustomersPerDay)
        {
            if (StorySceneManager.PersistentStoryInstance)
            {
                StorySceneManager.PersistentStoryInstance.MarkCompletedBakeryShift();
            }
            MenuManager.LoadBreakroomScene();
        }

        DisplayStationCompletion();
    }

    void RandomizeTimeForNextCustomerEntry()
    {
        TimeUntilNextCustomer = Random.Range(TimeRangeBetweenCustomerEntry.x, TimeRangeBetweenCustomerEntry.y);
    }

    private bool IsCustomerQueued()
    {
        return CustomersSeenToday > CustomersServedToday;
    }

    public int GetAmountOfCustomersInQueue()
    {
        return CustomersSeenToday - CustomersServedToday;
    }

    public override void DisplayStationCompletion()
    {
        TextMeshProUGUI ChalkboardText = GetComponentInChildren<TextMeshProUGUI>();
        if (ChalkboardText)
        {
            ChalkboardText.SetText(CustomersServedToday + " / " + CustomersPerDay + "\nDaily Orders\nFulfilled");
        }
    }
}

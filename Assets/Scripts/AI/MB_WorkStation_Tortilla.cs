using UnityEngine;

public class MB_WorkStation_Tortilla : MB_WorkStation
{
    public int CustomersPerDay = 3;
    public Vector2 TimeRangeBetweenCustomerEntry;

    private int CustomersSeenToday = 0;
    private int CustomersServedToday = 0;
    private float TimeUntilNextCustomer;

    public override bool CanWork()
    {
        return base.CanWork() && IsCustomerQueued();
    }

    void Start()
    {
        if (TimeRangeBetweenCustomerEntry == Vector2.zero)
        {
            TimeRangeBetweenCustomerEntry = new Vector2(5, 10);
        }

        RandomizeTimeForNextCustomerEntry();
        Debug.Log("First Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");
    }

    public override void Update()
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
            MenuManager.LoadBreakroomScene();
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

    protected override void UpdateProduction()
    {
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

    public override void DisplayStationCompletion()
    {
        if (NPC.IsWorking)
        {
            Debug.Log("Tortilla Station Completion: " + ProductionPercentage.ToString());
        }
    }
}

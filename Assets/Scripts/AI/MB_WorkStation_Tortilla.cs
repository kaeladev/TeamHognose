using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MB_WorkStation_Tortilla : MB_WorkStation
{
    public float TimeToServeCusomter = 3.0f;
    public int CustomersPerDay = 3;
    public Vector2 TimeRangeBetweenCustomerEntry;
    public Door DoorToControl;

    private Canvas Chalkboard;
    private Canvas Countdown;
    private int CustomersSeenToday = 0;
    private int CustomersServedToday = 0;
    private float TimeUntilNextCustomer;
    private float TimeUntilCurrentCustomerServed;
    private float BreakroomCountdown = 3;

    [HideInInspector]
    public bool IsShiftComplete = false;

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

        Canvas[] Canvases = GetComponentsInChildren<Canvas>(true);
        if (Canvases.Length != 2)
        {
            Debug.Log("MB_WorkStation_Tortilla: Shop cannot open, Tortilla doesn't have door access.");
            return;
        }

        Chalkboard = Canvases[0];
        Countdown = Canvases[1];

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

        if (!IsShiftComplete && CustomersServedToday == CustomersPerDay)
        {
            if (StorySceneManager.PersistentStoryInstance)
            {
                StorySceneManager.PersistentStoryInstance.MarkCompletedBakeryShift();
            }
            DoorToControl.SwapSign();
            IsShiftComplete = true;

            Countdown.gameObject.SetActive(true);
        }

        if (IsShiftComplete)
        {
            UpdateBreakroomCountdown();
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
        TextMeshProUGUI ChalkboardText = Chalkboard.GetComponentInChildren<TextMeshProUGUI>();
        if (ChalkboardText)
        {
            ChalkboardText.SetText(CustomersServedToday + " / " + CustomersPerDay + "\nDaily Orders\nFulfilled");
        }
    }

    private void UpdateBreakroomCountdown()
    {
        BreakroomCountdown -= Time.deltaTime;

        if (BreakroomCountdown < 0)
        {
            MenuManager.LoadBreakroomScene();
            return;
        }

        TextMeshProUGUI CountdownText = Countdown.GetComponentInChildren<TextMeshProUGUI>();
        if (CountdownText)
        {
            CountdownText.SetText("Shift Completed! Break Room in " + ((int)BreakroomCountdown + 1) + "...");
        }
    }
}

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

    private GameObject CustomerQueue;
    private Canvas Chalkboard;
    private Canvas Countdown;
    private int CustomersSeenToday = 0;
    private int CustomersServedToday = 0;
    private float TimeUntilNextCustomer;
    private float TimeUntilCurrentCustomerServed;
    private float BreakroomCountdown = 3;
    private float FirstCustomerEmoteTicker = 3.0f;

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
        if (Canvases.Length < 2)
        {
            Debug.Log("MB_WorkStation_Tortilla: Shop cannot open, Tortilla doesn't have door access.");
            return;
        }

        Chalkboard = Canvases[0];
        Countdown = Canvases[1];

        CustomerQueue = GameObject.FindWithTag("Queue");

        RandomizeTimeForNextCustomerEntry();
        Debug.Log("First Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");
    }

    public override void Update()
    {
        base.Update();

        int CustomersInQueue = GetAmountOfCustomersInQueue();

        FirstCustomerEmoteTicker += Time.deltaTime;
        if (FirstCustomerEmoteTicker >= 3)
        {
            FirstCustomerEmoteTicker = 0;
        }

        SpriteRenderer[] Customers = CustomerQueue.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < Customers.Length; i++)
        {
            if (i < CustomersInQueue)
            {
                Customers[i].gameObject.SetActive(true);
            }
            else
            {
                Customers[i].gameObject.SetActive(false);
            }
        }

        if (CustomersInQueue == 0)
        {
            return;
        }

        string CustomerEmoteString;
        if (IsMakingProgress())
        {
            CustomerEmoteString = "!";
        }
        else if (FirstCustomerEmoteTicker < 1)
        {
            CustomerEmoteString = "..";
        }
        else if (FirstCustomerEmoteTicker < 2)
        {
            CustomerEmoteString = "...";
        }
        else
        {
            CustomerEmoteString = "...?";
        }

        Customers[0].GetComponentInChildren<TextMeshProUGUI>(true).text = CustomerEmoteString;
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
                DoorToControl.RingBell();

                Debug.Log("Customer #" + CustomersServedToday + " Served!");
            }
        }

        if (CustomersSeenToday < CustomersPerDay)
        {
            TimeUntilNextCustomer -= Time.deltaTime;
            if (TimeUntilNextCustomer < 0)
            {
                SpawnCustomer();
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

    private void SpawnCustomer()
    {
        CustomersSeenToday++;
        Debug.Log("Customer Arrived: " + CustomersSeenToday.ToString() + "/" + CustomersPerDay.ToString());

        DoorToControl.RingBell();

        if (CustomersSeenToday < CustomersPerDay)
        {
            RandomizeTimeForNextCustomerEntry();
            Debug.Log("Next Customer Arriving in " + TimeUntilNextCustomer.ToString() + " Seconds");
        }
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

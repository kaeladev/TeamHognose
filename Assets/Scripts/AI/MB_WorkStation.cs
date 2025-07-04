using UnityEngine;

abstract public class MB_WorkStation : MonoBehaviour
{
    public MB_NPCBehavior_Work NPC;

    // 0.0-1.0 value aka 0% to 100%
    protected float WorkCompletionPercentage = 0.0f;

    // What % of work is completed per second when in use?
    public float RateOfCompletion = 1.0f;
     
    void Start()
    {
        if (!NPC)
        {
            Debug.Log("Did not find NPC in WorkStation: " + this.ToString());
        }
    }

    void Update()
    {
        if (NPC.IsWorking && WorkCompletionPercentage < 1)
        {
            WorkCompletionPercentage += ((RateOfCompletion / 100.0f) * Time.deltaTime);
            WorkCompletionPercentage = Mathf.Min(WorkCompletionPercentage, 1.0f);
        }

        DisplayStationCompletion();
    }

    // Each station will display their completion differently
    abstract public void DisplayStationCompletion();

    public float GetWorkCompletionPercentage() { return WorkCompletionPercentage; }

    public bool HasAmount(float Amount) { return WorkCompletionPercentage >= Amount; }
}

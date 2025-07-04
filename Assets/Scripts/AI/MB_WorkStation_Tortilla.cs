using UnityEngine;

public class MB_WorkStation_Tortilla : MB_WorkStation
{
    public override void DisplayStationCompletion()
    {
        if (NPC.IsWorking)
        {
            Debug.Log("Tortilla Station Completion: " + WorkCompletionPercentage.ToString());
        }
    }
}

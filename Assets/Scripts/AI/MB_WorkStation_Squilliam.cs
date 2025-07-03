using UnityEngine;

public class MB_WorkStation_Squilliam : MB_WorkStation
{
    public override void DisplayStationCompletion()
    {
        Debug.Log("Squilliam Station Completion: " + WorkCompletionPercentage.ToString());
    }
}

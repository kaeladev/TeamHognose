using UnityEngine;

public class MB_WorkStation_Squilliam : MB_WorkStation
{
    public override void DisplayStationCompletion()
    {
        if (NPC.IsWorking)
        {
            Debug.Log("Squilliam Station Completion: " + ProductionPercentage.ToString());
        }
    }
}

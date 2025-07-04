using UnityEngine;

public class MB_WorkStation_LilSoup : MB_WorkStation
{
    // TODO: Implement visual swapping
    // Dough rolling stages to change at different marks under 50%
    // Dough shaping stages to change at marks over 50%
    public override void DisplayStationCompletion()
    {
        if (NPC.IsWorking)
        {
            Debug.Log("Lil Soup Station Completion: " + WorkCompletionPercentage.ToString());
        }
    }
}

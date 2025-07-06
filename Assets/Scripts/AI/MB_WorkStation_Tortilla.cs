using UnityEngine;

public class MB_WorkStation_Tortilla : MB_WorkStation
{
    // TODO
    // public override bool CanWork()
    // {
    //     return base.CanWork() && IsCustomerQueued();
    // }

    protected override void UpdateProduction()
    {
        base.Update();
        DisplayStationCompletion();
    }

    public override void DisplayStationCompletion()
    {
        if (NPC.IsWorking)
        {
            Debug.Log("Tortilla Station Completion: " + ProductionPercentage.ToString());
        }
    }
}

using UnityEngine;

// All plush with work stations use this class
public class MB_NPCBehavior_Work : MB_NPCBehavior
{
    public bool IsWorking = false;

    float TimeToIdle = 0.0f;

    MB_WorkStation WorkStation;

    public override void Update()
    {
        if (TimeToIdle > 0)
        {
            TimeToIdle -= Time.deltaTime;
        }
        else
        {
            IsWorking = false;
        }

        AnimController.SetBool("IsWorking", IsWorking);
        // TODO: AnimController.SetFloat("WorkCompletion", WorkStation.GetWorkCompletionPercentage());
    }

    public override void OnMouseOver()
    {
        base.OnMouseOver();
        if (Input.GetMouseButtonDown(0))
        {
            IsWorking = true;
            TimeToIdle = 5.0f;
        }
    }
}

using UnityEngine;

// All plush with work stations use this class
public class MB_NPCBehavior_Work : MB_NPCBehavior
{
    public Texture2D HoverCursorTexture = null;
    public Texture2D InteractCursorTexture = null;

    [HideInInspector]
    public bool IsWorking = false;

    private float TimeToIdle = 0.0f;

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

    protected override void OnMouseOver()
    {
        base.OnMouseOver();
        if (Input.GetMouseButton(0))
        {
            IsWorking = true;
            TimeToIdle = 5.0f;
        }
    }
    protected override Texture2D GetCustomCursor()
    {
        return Input.GetMouseButton(0) ? InteractCursorTexture : HoverCursorTexture;
    }
}

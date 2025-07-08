using UnityEngine;

// All plush with work stations use this class
public class MB_NPCBehavior_Work : MB_NPCBehavior
{
    public Texture2D HoverCursorTexture = null;
    public Texture2D InteractCursorTexture = null;

    [HideInInspector]
    public bool IsWorking = false;
    [HideInInspector]
    public MB_WorkStation WorkStation;

    private float TimeToIdle = 0.0f;

    public override void Update()
    {
        if (TimeToIdle > 0)
        {
            TimeToIdle -= Time.deltaTime;
        }
        else if (IsWorking)
        {
            IsWorking = false;
            AnimController.SetBool("IsWorking", IsWorking);
        }

        AnimController.SetBool("IsWorking", WorkStation.IsMakingProgress());

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

        if (Input.GetMouseButtonDown(0))
        {
            AnimController.SetBool("IsWorking", IsWorking);
        }
    }
    protected override Texture2D GetCustomCursor()
    {
        return Input.GetMouseButton(0) ? InteractCursorTexture : HoverCursorTexture;
    }
}

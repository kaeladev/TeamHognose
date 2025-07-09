using UnityEngine;

// For Inky only :3 All hail The Void
public class MB_NPCBehavior_Inky : MB_NPCBehavior
{
    private float InkyAppearanceTimeRemaining = 0;
    private Vector3 OriginalLocation;
    private int OriginalDrawLayer;
    private Animator Tentacles;

    public override void Start()
    {
        base.Start();
        OriginalLocation = transform.position;
        OriginalDrawLayer = SpriteRend.sortingOrder;
    }

    public override void Update()
    {
        base.Update();

        InkyAppearanceTimeRemaining -= Time.deltaTime;

        if (InkyAppearanceTimeRemaining < 0)
        {
            ResetPosition();
        }
    }

    public void OnInkyFetch(Vector2 FetchLocation, int DrawLayer)
    {
        PlayReaction();
        InkyAppearanceTimeRemaining = 2.0f;

        //Tentacles.gameObject.transform.position = FetchLocation;
        gameObject.transform.position = FetchLocation;
        SpriteRend.sortingOrder = DrawLayer;
    }

    public void ResetPosition()
    {
        transform.position = OriginalLocation;
        SpriteRend.sortingOrder = OriginalDrawLayer;
    }
}

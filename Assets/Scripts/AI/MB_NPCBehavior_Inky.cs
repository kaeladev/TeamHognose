using UnityEngine;

// For Inky only :3 All hail The Void
public class MB_NPCBehavior_Inky : MB_NPCBehavior
{
    // TODO: TEMP - replace with tentacles and actual anims
    private float InkyAppearanceTimeRemaining = 0;
    private Vector3 OriginalLocation;
    private int OriginalDrawLayer;

    public override void Start()
    {
        base.Start();
        OriginalLocation = transform.position;
        OriginalDrawLayer = SpriteRend.sortingOrder;
    }

    public override void Update()
    {
        InkyAppearanceTimeRemaining -= Time.deltaTime;

        if (InkyAppearanceTimeRemaining < 0)
        {
            ResetPosition();
        }
    }

    // TODO: For now just have Inky pop up in the location
    public void OnInkyFetch(Vector2 FetchLocation, int DrawLayer)
    {
        PlayReaction();
        InkyAppearanceTimeRemaining = 2.0f;

        // TODO: Make Inky look like theyre actually fetching from location
        gameObject.transform.position = FetchLocation;
        SpriteRend.sortingOrder = DrawLayer;
    }

    public void ResetPosition()
    {
        transform.position = OriginalLocation;
        SpriteRend.sortingOrder = OriginalDrawLayer;
    }
}

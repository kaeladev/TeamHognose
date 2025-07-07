using UnityEngine;

// For Inky only :3 All hail The Void
public class MB_NPCBehavior_Inky : MB_NPCBehavior
{
    // TODO: TEMP - replace with tentacles and actual anims
    private Color CurrentColor = Color.white;
    private float InkyAppearanceTimeRemaining = 0;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        InkyAppearanceTimeRemaining -= Time.deltaTime;

        if (InkyAppearanceTimeRemaining < 0)
        {
            CurrentColor.a = 0;
        }

        SpriteRend.color = CurrentColor;
    }

    // TODO: For now just have Inky pop up in the location
    public void OnInkyFetch(Vector2 FetchLocation)
    {
        PlayReaction();
        InkyAppearanceTimeRemaining = 2.0f;
        CurrentColor.a = 255;

        // TODO: Make Inky look like theyre actually fetching from location
        gameObject.transform.position = FetchLocation;
    }
}

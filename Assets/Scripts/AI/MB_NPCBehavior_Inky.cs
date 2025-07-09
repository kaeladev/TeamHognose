using UnityEngine;

// For Inky only :3 All hail The Void
public class MB_NPCBehavior_Inky : MB_NPCBehavior
{
    public SpriteRenderer Portal;
    private float InkyAppearanceTimeRemaining = 0;
    private Animator Tentacles;

    public override void Start()
    {
        base.Start();
        Tentacles = GetComponentInChildren<Animator>(true);
    }

    public override void Update()
    {
        base.Update();

        if (InkyAppearanceTimeRemaining > -1)
        {
            InkyAppearanceTimeRemaining -= Time.deltaTime;

            if (InkyAppearanceTimeRemaining <= 0)
            {
                Tentacles.gameObject.SetActive(false);
            }
            if (InkyAppearanceTimeRemaining <= -1)
            {
                Portal.gameObject.SetActive(false);
            }
        }
    }

    public void OnInkyFetch(Vector2 FetchLocation, int DrawLayer, Vector2 XYScale)
    {
        if (InkyAppearanceTimeRemaining <= 0)
        {
            PlayReaction();
        }

        InkyAppearanceTimeRemaining = 1.2f;

        Portal.gameObject.SetActive(true);
        Tentacles.gameObject.SetActive(true);
        Tentacles.gameObject.transform.position = FetchLocation;
        Tentacles.gameObject.transform.localScale = XYScale;
        Tentacles.GetComponent<SpriteRenderer>().sortingOrder = DrawLayer;
    }
}

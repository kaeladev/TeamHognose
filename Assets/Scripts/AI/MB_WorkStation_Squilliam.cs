using UnityEngine;

public class MB_WorkStation_Squilliam : MB_WorkStation
{
    private SpriteRenderer SpriteRend;
    public Sprite[] MixingBowlSprites;
    public string SoundEffectPath;

    protected float RawIngredients = 0.0f;

    public void Start()
    {
        SpriteRend = GetComponent<SpriteRenderer>();
    }

    public override void Update()
    {
        base.Update();

        DisplayStationCompletion();

        TryGetIngredientsIfRequired();
    }

    public override void DisplayStationCompletion()
    {
        if (IsMakingProgress())
        {
            Debug.Log("Squilliam Station Completion: " + ProductionPercentage.ToString());

            SpriteRend.sprite = MixingBowlSprites[1];
            return;
        }

        SpriteRend.sprite = MixingBowlSprites[0];
    }

    void TryGetIngredientsIfRequired()
    {
        if (RawIngredients > 0)
        {
            return;
        }

        Debug.Log("Squilliam Station: Refilling Ingredients");

        int IngredientsGrabbed = BakeryManager.CurrentBakeryInstance.GrabIngredients();
        RawIngredients = 0.5f * IngredientsGrabbed;

        // Show Inky tentacles moving ingredients to bowl
        NPC.ProductionComplete.Invoke(gameObject.transform.position, NPC.GetDrawLayer() + 4);
    }

    protected override void UpdateProduction()
    {
        if (IsMakingProgress())
        {
            float ProductionThisFrame = (ProductionRate / 100.0f) * Time.deltaTime;
            RawIngredients -= ProductionThisFrame;
            ProductionPercentage += ProductionThisFrame;
            ProductionPercentage = Mathf.Min(ProductionPercentage, 1.0f);
        }
    }

    public override bool CanWork()
    {
        return base.CanWork() && ProductionPercentage < 1 && RawIngredients > 0;
    }
}

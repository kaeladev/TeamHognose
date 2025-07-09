using UnityEngine;

public class MB_WorkStation_LilSoup : MB_WorkStation
{
    private SpriteRenderer SpriteRend;
    public Sprite[] DoughSprites;
    bool HasDoughToRoll;

    public void Start()
    {
        SpriteRend = GetComponent<SpriteRenderer>();
        HasDoughToRoll = true;
        ProductionPercentage = .1f;
    }

    public override void Update()
    {
        base.Update();

        // If the production is 0, then we need to grab from Squilliam
        if (ProductionPercentage == 0)
        {
            if (BakeryManager.CurrentBakeryInstance.GrabDoughBall())
            {
                HasDoughToRoll = true;
                Debug.Log("Lil Soup Station Beginning!");
            }
        }

        if (ProductionPercentage >= 1)
        {
            NPC.ProductionComplete.Invoke(InkyGrabPosition, 20);
            ProductionPercentage = 0;
            HasDoughToRoll = false;
            Debug.Log("Lil Soup Station Complete!");
        }

        DisplayStationCompletion();
    }

    protected override void UpdateProduction()
    {
        if (IsMakingProgress())
        {
            float ProductionThisFrame = (ProductionRate / 100.0f) * Time.deltaTime;
            ProductionPercentage += ProductionThisFrame;
            ProductionPercentage = Mathf.Min(ProductionPercentage, 1.0f);
        }
    }

    public override bool IsMakingProgress()
    {
        return base.IsMakingProgress() && HasDoughToRoll;
    }

    public override void DisplayStationCompletion()
    {
        if (IsMakingProgress())
        {
            Debug.Log("Lil Soup Station Completion: " + ProductionPercentage.ToString());
        }

        if (!HasDoughToRoll)
        {
            SpriteRend.sprite = null;
            Debug.Log("Lil Soup Station: No Dough, Boss!");
            return;
        }

        if (ProductionPercentage < 0.2)
        {
            SpriteRend.sprite = DoughSprites[0];
        }
        else if (ProductionPercentage < 0.4)
        {
            SpriteRend.sprite = DoughSprites[1];
        }
        else if (ProductionPercentage < 0.6)
        {
            SpriteRend.sprite = DoughSprites[2];
        }
        else if (ProductionPercentage < 0.8)
        {
            SpriteRend.sprite = DoughSprites[3];
        }
        else
        {
            SpriteRend.sprite = DoughSprites[4];
        }
    }
}

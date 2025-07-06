using UnityEngine;

public class MB_WorkStation_Squilliam : MB_WorkStation
{
    protected float RawIngredients = 0.0f;

    public override void DisplayStationCompletion()
    {
        if (NPC.IsWorking)
        {
            Debug.Log("Squilliam Station Completion: " + ProductionPercentage.ToString());
        }
    }

    public override void Update()
    {
        base.Update();

        TryGetIngredientsIfRequired();

        if (CanWork())
        {
            float ProductionThisFrame = (ProductionRate / 100.0f) * Time.deltaTime;
            RawIngredients -= ProductionThisFrame;
            ProductionPercentage += ProductionThisFrame;
            ProductionPercentage = Mathf.Min(ProductionPercentage, 1.0f);
        }

        if (ProductionPercentage >= 1)
        {
            NPC.ProductionComplete.Invoke(gameObject.transform.position);
        }

        DisplayStationCompletion();
    }

    void TryGetIngredientsIfRequired()
    {
        if (RawIngredients > 0)
        {
            return;
        }

        int IngredientsGrabbed = BakeryManager.CurrentBakeryInstance.GrabIngredients();
        RawIngredients = 0.5f * IngredientsGrabbed;
    }

    protected override void UpdateProduction()
    {
        float ProductionThisFrame = (ProductionRate / 100.0f) * Time.deltaTime;
        RawIngredients -= ProductionThisFrame;
        ProductionPercentage += ProductionThisFrame;
        ProductionPercentage = Mathf.Min(ProductionPercentage, 1.0f);
    }

    protected override bool CanWork()
    {
        return base.CanWork() && ProductionPercentage < 1 && RawIngredients > 0;
    }
}

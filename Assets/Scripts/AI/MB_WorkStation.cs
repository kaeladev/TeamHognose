using UnityEngine;
using UnityEngine.Events;

abstract public class MB_WorkStation : MonoBehaviour
{
    // What % of work is completed per second when in use?
    public float ProductionRate = 1.0f;

    // Measured in 0.0-1.0 value aka 0% to 100%
    protected float RawIngredients = 0.0f;
    protected float ProductionPercentage = 0.0f;

    [HideInInspector]
    public MB_NPCBehavior_Work NPC;

    [HideInInspector]
    public UnityEvent<Vector2> ProductionReadyAtLocation;

    void Start()
    {
        if (!NPC)
        {
            Debug.Log("Did not find NPC in WorkStation: " + this.ToString());
        }
    }

    void Update()
    {
        if (CanWork())
        {
            ProductionPercentage += ((ProductionRate / 100.0f) * Time.deltaTime);
            ProductionPercentage = Mathf.Min(ProductionPercentage, 1.0f);
        }

        if (ProductionPercentage >= 1)
        {
            ProductionReadyAtLocation.Invoke(gameObject.transform.position);
        }

        DisplayStationCompletion();
    }

    public bool IsMakingProgress()
    {
        return CanWork() && NPC.IsWorking;
    }

    // Each station will display their completion differently
    abstract public void DisplayStationCompletion();

    public bool CanWork()
    {
        return NPC && ProductionPercentage < 1;
    }

    public float GetWorkCompletionPercentage() { return ProductionPercentage; }

    public bool HasAmount(float Amount) { return ProductionPercentage >= Amount; }
}

using UnityEngine;
using UnityEngine.Events;

abstract public class MB_WorkStation : MonoBehaviour
{
    // What % of production is completed per second when in use?
    public float ProductionRate = 1.0f;

    // Measured in 0.0-1.0 value aka 0% to 100%
    protected float ProductionPercentage = 0.0f;

    [HideInInspector]
    public MB_NPCBehavior_Work NPC;

    public virtual void Update()
    {
        if (ProductionPercentage >= 1)
        {
            // TODO: Production doesnt auto finish for all workstations; 
            // ex: if soup cant take more dough
            NPC.ProductionComplete.Invoke(gameObject.transform.position);
        }
    }

    public bool IsMakingProgress()
    {
        return CanWork() && NPC.IsWorking;
    }

    // Each station will display their completion differently
    public abstract void DisplayStationCompletion();

    protected virtual bool CanWork()
    {
        return NPC;
    }

    protected abstract void UpdateProduction();

    public float GetWorkCompletionPercentage() { return ProductionPercentage; }

    public bool HasAmount(float Amount) { return ProductionPercentage >= Amount; }
}

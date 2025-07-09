using FMOD.Studio;
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

    public string StationProgressAudioPath;
    private EventInstance SFXInstance;
    private bool IsSoundOn = false;

    public virtual void Update()
    {
        UpdateProduction();

        if (StationProgressAudioPath.Length == 0)
        {
            return;
        }

        if (!IsSoundOn && IsMakingProgress())
        {
            SFXInstance = FMODUnity.RuntimeManager.CreateInstance(StationProgressAudioPath);
            SFXInstance.start();
            IsSoundOn = true;
        }
        else if (IsSoundOn && !IsMakingProgress())
        {
            SFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            SFXInstance.release();
            IsSoundOn = false;
        }
    }

    public virtual bool IsWaitingToWork()
    {
        return !CanWork() && NPC.IsWorking;
    }

    public virtual bool IsMakingProgress()
    {
        return CanWork() && NPC.IsWorking;
    }

    // Each station will display their completion differently
    public abstract void DisplayStationCompletion();

    public virtual bool CanWork()
    {
        return NPC;
    }

    protected abstract void UpdateProduction();

    public float GetWorkCompletionPercentage() { return ProductionPercentage; }

    public bool HasAmount(float Amount) { return ProductionPercentage >= Amount; }

    public bool TakeAmount(float Amount)
    {
        if (HasAmount(Amount))
        {
            ProductionPercentage -= Amount;
            return true;
        }
        return false; 
    }
}

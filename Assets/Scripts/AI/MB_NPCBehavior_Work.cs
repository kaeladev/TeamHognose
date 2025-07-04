using UnityEngine;

// This is the behavior only for in-bakery, not for post-shift
public class MB_NPCBehavior_Work : MonoBehaviour
{
    public bool IsWorking = false;

    float TimeToIdle = 0.0f;

    Animator AnimController;

    MB_WorkStation WorkStation;
    
    void Start()
    {
        AnimController = GetComponent<Animator>();
    }

    void Update()
    {
        if (TimeToIdle > 0)
        {
            TimeToIdle -= Time.deltaTime;
        }
        else
        {
            IsWorking = false;
        }

        AnimController.SetBool("IsWorking", IsWorking);
        // TODO: AnimController.SetFloat("WorkCompletion", WorkStation.GetWorkCompletionPercentage());
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsWorking = true;
            TimeToIdle = 5.0f;
        }
    }
}

using FMODUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// This should be Yuzu (+ any other deliverers) pathing to the door, dropping off delivery inside,
// then walking off screen and waiting a few seconds before repeating process
public class MB_NPCBehavior_Deliver : MB_NPCBehavior
{
    public float WalkingSpeed = 2.0f;
    public float RunningSpeed = 5.0f;
    public float AcceptableDistanceToPathingGoal = 10.0f;

    public Vector2 DeliveryPickupLocation; // Should be offscreen/not visible
    public Vector2 StoreDoorLocation;
    public Vector2 DeliveryDropoffLocation;

    public string ArrivalSoundPath = "event:/SFX/SFX_Yuzu_Arrival_01";
    public string LeaveSoundPath = "event:/SFX/SFX_Yuzu_Leaving";
    public Texture2D HoverCursorTexture = null;
    public Texture2D InteractCursorTexture = null;

    private bool HasDelivery = true;
    private bool IsInStore = false;
    private float WaitInStoreTime = 0.0f;
    private Vector2 CurrentPathingGoal;
    private int RenderLayerOutsideStore;

    public override void Start()
    {
        base.Start();
        RenderLayerOutsideStore = SpriteRend.sortingOrder;
        gameObject.transform.position = DeliveryPickupLocation;
        PickUpDelivery();
    }

    public override void Update()
    {
        bool IsIdling = WaitInStoreTime > 0;

        if (IsIdling)
        {
            // Deliverer is just idling
            WaitInStoreTime -= Time.deltaTime;
            AnimController.SetBool("IsIdling", WaitInStoreTime > 0);
        }
        else if (HasReachedPathingGoal())
        {
            if (HasDelivery && !IsInStore)
            {
                if (BakeryManager.CurrentBakeryInstance && BakeryManager.CurrentBakeryInstance.StoredIngredients == 3)
                {
                    // Yuzu will chill offscreen until she needs to deliver again
                    return;
                }
                EnterStore();
            }
            else if (HasDelivery && IsInStore)
            {
                DropOffDelivery();
            }
            else if (!HasDelivery && IsInStore)
            {
                ExitStore();
            }
            else if (!HasDelivery && !IsInStore)
            {
                PickUpDelivery();
            }
        }
        else
        {
            gameObject.transform.position += (Vector3)(GetNormalizedDirectionTowardsPathingGoal() * GetMovementSpeed() * Time.deltaTime);
        }

        AnimController.SetBool("IsIdling", IsIdling);
        AnimController.SetBool("UseWalkingSpeed", HasDelivery);
    }

    bool HasReachedPathingGoal()
    {
        return Vector2.Distance(gameObject.transform.position, CurrentPathingGoal) < AcceptableDistanceToPathingGoal;
    }

    float GetMovementSpeed()
    {
        return HasDelivery ? WalkingSpeed : RunningSpeed;
    }

    Vector2 GetNormalizedDirectionTowardsPathingGoal()
    {
        return (CurrentPathingGoal - (Vector2)gameObject.transform.position).normalized;
    }

    void PickUpDelivery()
    {
        HasDelivery = true;
        CurrentPathingGoal = StoreDoorLocation;
    }

    void DropOffDelivery()
    {
        ProductionComplete.Invoke(CurrentPathingGoal);
        BakeryManager.CurrentBakeryInstance.StoredIngredients++;
        HasDelivery = false;
        CurrentPathingGoal = StoreDoorLocation;
        WaitInStoreTime = 3.0f;
    }

    void EnterStore()
    {
        // Play door opening anim
        FMODUnity.RuntimeManager.PlayOneShot(ArrivalSoundPath, CurrentPathingGoal);
        CurrentPathingGoal = DeliveryDropoffLocation;
        IsInStore = true;
        SpriteRend.sortingOrder = RenderLayerOutsideStore + 4;
    }

    void ExitStore()
    {
        // Play door opening anim
        FMODUnity.RuntimeManager.PlayOneShot(LeaveSoundPath, CurrentPathingGoal);
        CurrentPathingGoal = DeliveryPickupLocation;
        IsInStore = false;
        SpriteRend.sortingOrder = RenderLayerOutsideStore;
    }

    void PetNPC()
    {
        // Play reaction anim or VFX

        if (!StorySceneManager.PersistentStoryInstance)
        {
            Debug.Log("StorySceneManager cannot increase pets because no persistent instance has been created");
            return;
        }
        StorySceneManager.PersistentStoryInstance.PetYuzu();
        Debug.Log("YUZU HAS BEEN PETTED!!! ;D");
    }

    protected override void OnMouseOver()
    {
        if (IsInStore)
        {
            base.OnMouseOver();
            if (Input.GetMouseButtonDown(0))
            {
                PetNPC();
            }
        }
    }
    protected override Texture2D GetCustomCursor()
    {
        if (!IsInStore)
        {
            return null;
        }

        return Input.GetMouseButton(0) ? InteractCursorTexture : HoverCursorTexture;
    }
}

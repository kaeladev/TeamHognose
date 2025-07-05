using FMODUnity;
using UnityEditor;
using UnityEngine;

// This should be Yuzu pathing to the door, waiting for Inky to grab ingredients/open door, 
// then walking off screen and waiting a few seconds before repeating process
public class MB_NPCBehavior_Deliver : MonoBehaviour
{
    public float WalkingSpeed = 2.0f;
    public float RunningSpeed = 5.0f;
    public float AcceptableDistanceToPathingGoal = 10.0f;

    public Vector2 DeliveryPickupLocation; // Should be offscreen/not visible
    public Vector2 StoreDoorLocation;
    public Vector2 DeliveryDropoffLocation;

    private bool HasDelivery = true;
    private bool IsInStore = false;
    private float WaitInStoreTime = 0.0f;
    private Vector2 CurrentPathingGoal;
    private int RenderLayerOutsideStore;

    private Animator AnimController;
    private SpriteRenderer SpriteRend;
    private StudioEventEmitter SoundMaker;

    void Start()
    {
        AnimController = GetComponent<Animator>();
        SpriteRend = GetComponent<SpriteRenderer>();
        SoundMaker = GetComponent<StudioEventEmitter>();
        RenderLayerOutsideStore = SpriteRend.sortingOrder;
        gameObject.transform.position = DeliveryPickupLocation;
        PickUpDelivery();
    }

    void Update()
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
        // Alert Inky to tentacle over, add to ingredients
        HasDelivery = false;
        CurrentPathingGoal = StoreDoorLocation;
        WaitInStoreTime = 3.0f;
    }

    void EnterStore()
    {
        // Play door opening anim
        SoundMaker.Play();
        CurrentPathingGoal = DeliveryDropoffLocation;
        IsInStore = true;
        SpriteRend.sortingOrder = RenderLayerOutsideStore + 4;
    }

    void ExitStore()
    {
        // Play door opening anim
        // Play audio event for leaving
        CurrentPathingGoal = DeliveryPickupLocation;
        IsInStore = false;
        SpriteRend.sortingOrder = RenderLayerOutsideStore;
    }

    void OnMouseOver()
    {
        if (IsInStore && Input.GetMouseButtonDown(0))
        {
            PetNPC();
        }
    }

    void PetNPC()
    {
        // Play audio event for pet reaction
        // Play reaction anim or VFX

        if (!StorySceneManager.PersistentInstance)
        {
            Debug.Log("StorySceneManager cannot increase pets because no persistent instance has been created");
            return;
        }
        StorySceneManager.PersistentInstance.PetYuzu();
        Debug.Log("YUZU HAS BEEN PETTED!!! ;D");
    }
}

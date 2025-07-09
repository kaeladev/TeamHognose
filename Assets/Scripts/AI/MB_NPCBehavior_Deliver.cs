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
    public string GrabBagAudioPath;

    public Vector2 DeliveryPickupLocation;
    public Vector2 StoreDoorLocation;
    public Vector2 DeliveryDropoffLocation;

    public Door DoorToUse;
    public string ArrivalSoundPath = "event:/SFX/SFX_Yuzu_Arrival_01";
    public Texture2D HoverCursorTexture = null;
    public Texture2D InteractCursorTexture = null;

    public SpriteRenderer MouthObject;
    public Sprite EnteringObject;
    public Sprite ExitingObject;

    private bool HasDelivery = true;
    private bool IsInStore = false;
    private float WaitInStoreTime = 0.0f;
    private Vector2 CurrentPathingGoal;
    private int[] RenderLayersOutsideStore;

    public GameObject Flour1;
    public GameObject Flower2;
    public GameObject Floor3;

    public override void Start()
    {
        base.Start();
        gameObject.transform.position = DeliveryPickupLocation;

        SpriteRenderer[] BodySprites = GetComponentsInChildren<SpriteRenderer>();
        RenderLayersOutsideStore = new int[BodySprites.Length];
        for (int i = 0; i < RenderLayersOutsideStore.Length; i++)
        {
            RenderLayersOutsideStore[i] = BodySprites[i].sortingOrder;
        }
    }

    public override void Update()
    {
        base.Update();

        Flour1.SetActive(BakeryManager.CurrentBakeryInstance.StoredIngredients > 0);
        Flower2.SetActive(BakeryManager.CurrentBakeryInstance.StoredIngredients > 1);
        Floor3.SetActive(BakeryManager.CurrentBakeryInstance.StoredIngredients > 2);

        bool IsIdling = WaitInStoreTime > 0;

        if (IsIdling)
        {
            WaitInStoreTime -= Time.deltaTime;
            if (AnimController.runtimeAnimatorController)
            {
                AnimController.SetBool("IsIdling", WaitInStoreTime > 0);
            }
        }
        else if (HasReachedPathingGoal())
        {
            if (!HasDelivery && !IsInStore)
            {
                if (BakeryManager.CurrentBakeryInstance && BakeryManager.CurrentBakeryInstance.StoredIngredients == 3)
                {
                    // Yuzu will chill offscreen until she needs to deliver again
                    return;
                }
                PickUpDelivery();
            }
            else if (HasDelivery && !IsInStore)
            {
                EnterStore();
            }
            else if (!HasDelivery && IsInStore)
            {
                ExitStore();
            }
            else if (HasDelivery && IsInStore)
            {
                DropOffDelivery();
            }
        }
        else
        {
            Vector3 SwappedXTransform = gameObject.transform.localScale;
            Vector3 NormalizedDirection = GetNormalizedDirectionTowardsPathingGoal();
            
            if ((Vector3.Dot(NormalizedDirection, new Vector3(1, 0, 0)) > 0 && gameObject.transform.localScale.x > 0)
                || (Vector3.Dot(NormalizedDirection, new Vector3(1, 0, 0)) < 0 && gameObject.transform.localScale.x < 0))
            {
                SwappedXTransform.x *= -1;
                gameObject.transform.localScale = SwappedXTransform;
            }
            gameObject.transform.position += (NormalizedDirection * GetMovementSpeed() * Time.deltaTime);
        }
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
        MouthObject.sprite = EnteringObject;
    }

    void DropOffDelivery()
    {
        ProductionComplete.Invoke(InkyGrabPosition, GetDrawLayer(), InkyGrabScale);
        FMODUnity.RuntimeManager.PlayOneShot(GrabBagAudioPath, CurrentPathingGoal);

        BakeryManager.CurrentBakeryInstance.StoredIngredients++;

        HasDelivery = false;
        CurrentPathingGoal = StoreDoorLocation;
        WaitInStoreTime = 5.0f;
        MouthObject.sprite = ExitingObject;
    }

    void EnterStore()
    {
        FMODUnity.RuntimeManager.PlayOneShot(ArrivalSoundPath, CurrentPathingGoal);
        
        DoorToUse.OpenDoor();
        CurrentPathingGoal = DeliveryDropoffLocation;
        IsInStore = true;
        UpdateDrawLayers();
    }

    void ExitStore()
    {
        DoorToUse.RingBell();
        CurrentPathingGoal = DeliveryPickupLocation;
        IsInStore = false;
        UpdateDrawLayers();
    }

    protected override void PetNPC()
    {
        base.PetNPC();

        if (!StorySceneManager.PersistentStoryInstance)
        {
            Debug.Log("StorySceneManager cannot increase pets because no persistent instance has been created");
            return;
        }
        StorySceneManager.PersistentStoryInstance.PetYuzu();
    }

    public override int GetDrawLayer()
    {
        return 16;
    }

    protected override void OnMouseOver()
    {
        if (IsInStore)
        {
            base.OnMouseOver();
            if (Input.GetMouseButton(0))
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

    private void UpdateDrawLayers()
    {
        SpriteRenderer[] BodySprites = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < BodySprites.Length; i++)
        {
            int LayerForSprite = RenderLayersOutsideStore[i];
            if (IsInStore)
            {
                LayerForSprite += 7;
            }
            BodySprites[i].sortingOrder = LayerForSprite;
        }
    }
}

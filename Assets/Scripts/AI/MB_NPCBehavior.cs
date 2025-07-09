using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StudioEventEmitter))]
abstract public class MB_NPCBehavior : MonoBehaviour
{
    public Animator HeartAnimController;

    protected Animator AnimController;
    protected SpriteRenderer SpriteRend;
    protected StudioEventEmitter ReactionSoundMaker;
    protected bool IsPlayerHovering = false;
    protected float HeartEmotingTime = 0;

    [HideInInspector]
    public UnityEvent<Vector2, int> ProductionComplete;

    public virtual void Start()
    {
        AnimController = GetComponent<Animator>();
        SpriteRend = GetComponent<SpriteRenderer>();
        ReactionSoundMaker = GetComponent<StudioEventEmitter>();
    }

    public virtual void Update()
    {
        UpdateHeartEmote();
    }

    protected virtual void PetNPC()
    {
        SpriteRenderer Heart = HeartAnimController.gameObject.GetComponent<SpriteRenderer>();
        Heart.color = Color.white;
        HeartEmotingTime = 1.0f;
    }

    public virtual int GetDrawLayer()
    {
        if (SpriteRend)
        {
            return SpriteRend.sortingOrder;
        }

        SpriteRenderer ChildSpriteRend = GetComponentInChildren<SpriteRenderer>();
        if (ChildSpriteRend)
        {
            return ChildSpriteRend.sortingOrder;
        }

        return 0;
    }

    protected virtual void PlayReaction()
    {
        ReactionSoundMaker.Play();
    }

    protected virtual Texture2D GetCustomCursor()
    {
        return null;
    }

    protected virtual void OnMouseEnter()
    {
        IsPlayerHovering = true;
    }

    protected virtual void OnMouseExit()
    {
        IsPlayerHovering = false;
        MenuManager.UpdateCursor(null);
    }

    protected virtual void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)) // Only true for first frame of press
        {
            PlayReaction();
        }

        if (Input.GetMouseButton(0))
        {
            PetNPC();
        }

        MenuManager.UpdateCursor(GetCustomCursor());
      }

    void UpdateHeartEmote()
    {
        if (!HeartAnimController)
        {
            return;
        }

        if (IsPlayerHovering && Input.GetMouseButton(0))
        {
            HeartEmotingTime = 1; // Reset heart emote time
        }
        else if (HeartEmotingTime > 0)
        {
            HeartEmotingTime -= Time.deltaTime;
        }
        else
        {
            SpriteRenderer Heart = HeartAnimController.gameObject.GetComponent<SpriteRenderer>();
            Heart.color = Color.clear;
        }
    }
}

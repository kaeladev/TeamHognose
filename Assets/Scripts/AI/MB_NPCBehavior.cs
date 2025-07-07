using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StudioEventEmitter))]
abstract public class MB_NPCBehavior : MonoBehaviour
{
    protected Animator AnimController;
    protected SpriteRenderer SpriteRend;
    protected StudioEventEmitter ReactionSoundMaker;
    protected bool IsPlayerHovering = false;

    [HideInInspector]
    public UnityEvent<Vector2> ProductionComplete;

    public virtual void Start()
    {
        AnimController = GetComponent<Animator>();
        SpriteRend = GetComponent<SpriteRenderer>();
        ReactionSoundMaker = GetComponent<StudioEventEmitter>();
    }

    public virtual void Update()
    {

    }

    protected virtual void PetNPC()
    {
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
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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

        Cursor.SetCursor(GetCustomCursor(), Vector2.zero, CursorMode.Auto);
    }
}

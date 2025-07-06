using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StudioEventEmitter))]
abstract public class MB_NPCBehavior : MonoBehaviour
{
    protected Animator AnimController;
    protected SpriteRenderer SpriteRend;
    protected StudioEventEmitter ReactionSoundMaker;

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

    public virtual void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayReaction();
        }
    }

    protected virtual void PlayReaction()
    {
        ReactionSoundMaker.Play();
    }
}

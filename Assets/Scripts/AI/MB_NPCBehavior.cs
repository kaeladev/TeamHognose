using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(StudioEventEmitter))]
abstract public class MB_NPCBehavior : MonoBehaviour
{
    public string ReactionSoundPath;

    protected Animator AnimController;
    protected SpriteRenderer SpriteRend;
    protected StudioEventEmitter ReactionSoundMaker;

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
        if (Input.GetMouseButtonDown(0) && ReactionSoundPath.Length != 0)
        {
            ReactionSoundMaker.Play();
        }
    }
}

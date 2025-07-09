using UnityEditor;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string DoorBellAudioPath = "event:/SFX/SFX_Yuzu_Leaving";
    public Sprite ClosedDoorSign;
    public float TimeDoorSwingsOpen = 2.5f;
    private float TimeToCloseDoor = 0;
    private SpriteRenderer SpriteRend;

    void Start()
    {
        SpriteRend = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (TimeToCloseDoor > 0)
        {
            TimeToCloseDoor -= Time.deltaTime;
            if (TimeToCloseDoor < 0)
            {
                CloseDoor();
            }
        }
    }
    public void SwapSign()
    {
        SpriteRend.sprite = ClosedDoorSign;
    }

    public void OpenDoor()
    {
        // TODO: Play door opening anim
        TimeToCloseDoor = TimeDoorSwingsOpen;
    }

    public void CloseDoor()
    {
        
    }

    public void RingBell()
    {
        OpenDoor();
        FMODUnity.RuntimeManager.PlayOneShot(DoorBellAudioPath, gameObject.transform.position);
    }
}

using UnityEditor;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string DoorBellAudioPath = "event:/SFX/SFX_Yuzu_Leaving";
    public Sprite ClosedDoorSign;
    public Sprite OpenedDoor;
    public float TimeDoorSwingsOpen = 2.5f;
    private float TimeToCloseDoor = 0;
    private SpriteRenderer SpriteRend;
    private Sprite OriginalClosedDoor;

    void Start()
    {
        SpriteRend = GetComponent<SpriteRenderer>();
        OriginalClosedDoor = SpriteRend.sprite;
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
        SpriteRend.sprite = OpenedDoor;
        TimeToCloseDoor = TimeDoorSwingsOpen;
    }

    public void CloseDoor()
    {
        SpriteRend.sprite = OriginalClosedDoor;
    }

    public void RingBell()
    {
        OpenDoor();
        if (TimeToCloseDoor < 0) // Is door already open?, If so, dont ring bell
        {
            FMODUnity.RuntimeManager.PlayOneShot(DoorBellAudioPath, gameObject.transform.position);
        }
    }
  
}

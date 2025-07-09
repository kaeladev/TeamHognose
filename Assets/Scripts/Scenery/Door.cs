using UnityEditor;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string DoorBellAudioPath = "event:/SFX/SFX_Yuzu_Leaving";
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
        }

        Animator AnimController = GetComponentInChildren<Animator>();
        if (AnimController)
        {
            AnimController.SetBool("IsOpen", TimeToCloseDoor > 0);
        }
    }
    public void SwapSign()
    {
        SpriteRend.color = Color.white;

        GetComponentInChildren<Animator>().gameObject.SetActive(false);
    }

    public void OpenDoor()
    {
        TimeToCloseDoor = TimeDoorSwingsOpen;
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

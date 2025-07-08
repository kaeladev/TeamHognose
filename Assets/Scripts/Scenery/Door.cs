using UnityEditor;
using UnityEngine;

public class Door : MonoBehaviour
{
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
        SpriteRend.color = Color.black;
        TimeToCloseDoor = TimeDoorSwingsOpen;
    }

    public void CloseDoor()
    {
        SpriteRend.color = Color.white;
    }
}

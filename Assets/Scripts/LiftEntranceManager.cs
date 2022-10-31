using UnityEngine;

public class LiftEntranceManager : MonoBehaviour
{
    public LiftCageManager liftCageManager;
    public Transform leftDoor;
    public Transform rightDoor;
    public bool IsClosed = true;
    public FloorCode floorCode;
    public float floorHeight;

    public float GetFloorHeight() { return floorHeight; }

   
}

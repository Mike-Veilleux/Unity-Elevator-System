using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LiftCageManager : MonoBehaviour
{
    event EventHandler DoorAreOpen;
    event EventHandler DoorAreClosed;
    event EventHandler LiftReachedDestination;

    public float liftSpeed = 4f;
    public float doorSpeed = 2f;
    public float doorOpeningDistance = 0.95f;

    public Transform cageLeftDoor;
    public Transform cageRightDoor;
    public GameObject floorButtonsUI;
    public RectTransform floorButtonsUIFrame;
    public GameObject prefabButtonUI;
    public List<LiftEntranceManager> liftEntranceManagers;

    [HideInInspector] public GameObject liftCage;

    private LiftEntranceManager currentLiftEntrance;
    private LiftEntranceManager selectedLiftEntrance;

    //private bool IsClosed = true;


    private void Awake()
    {
        liftCage = this.gameObject;
        currentLiftEntrance = liftEntranceManagers[0];
        PopulateButtonsInUI();
    }



    private void OnEnable()
    {
        LiftFloorSelectorButton.floorSelected += StartLiftSequenceFromCagePanelSelection;

        DoorAreOpen += OnDoorAreOpen;
        DoorAreClosed += OnDoorAreClosed;
        LiftReachedDestination += OnLiftReachedDestination;

    }



    private void OnDisable()
    {
        LiftFloorSelectorButton.floorSelected -= StartLiftSequenceFromCagePanelSelection;

        DoorAreOpen -= OnDoorAreOpen;
        DoorAreClosed -= OnDoorAreClosed;
        LiftReachedDestination -= OnLiftReachedDestination;
    }



    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectEntrancePanelButtonHit();
            DetectCagePanelButtonHit();
        }
    }



    private void PopulateButtonsInUI()
    {
        for (int i = liftEntranceManagers.Count - 1; i >= 0; i--)
        {
            var newButton = Instantiate(prefabButtonUI);
            newButton.transform.SetParent(floorButtonsUIFrame, false);

            var newLiftFloorSelectorButton = newButton.GetComponent<LiftFloorSelectorButton>();

            newLiftFloorSelectorButton.liftEntranceManager = liftEntranceManagers[i].GetComponent<LiftEntranceManager>();
            newLiftFloorSelectorButton.FloorCodeToButtonText();
        }
    }



    private void CloseAllEntranceDoors()
    {

        if (liftEntranceManagers.Any(x => x.IsClosed == false))
        {
            foreach (var item in liftEntranceManagers)
            {
                if (!item.IsClosed)
                {
                    StartCoroutine(CloseDoorFromEntranceCall(item));
                }
            }
        }
        else
        {
            StartCoroutine(MoveCageToFloor());
        }


    }



    private void OnLiftReachedDestination(object sender, EventArgs e)
    {
        StartCoroutine(OpenDoorsFromCageCall());
    }



    private void OnDoorAreClosed(object sender, EventArgs e)
    {
        StartCoroutine(MoveCageToFloor());
    }



    private void OnDoorAreOpen(object sender, EventArgs e)
    {

    }



    private void DetectEntrancePanelButtonHit()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.transform.gameObject.CompareTag("LiftCallButton"))
            {
                selectedLiftEntrance = hit.transform.parent.parent.GetComponent<LiftEntranceManager>();

                CloseAllEntranceDoors();

            }
        }
    }



    private void DetectCagePanelButtonHit()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.transform.gameObject.CompareTag("LiftPanelButton"))
            {
                ShowFloorButtonUI();
            }
        }
    }
       



    private IEnumerator MoveCageToFloor()
    {
        Vector3 startingPos = transform.position;
        Vector3 finalPos = new Vector3(liftCage.transform.position.x, selectedLiftEntrance.GetFloorHeight(), liftCage.transform.position.z);

        float time = Mathf.Abs(finalPos.y - startingPos.y) / liftSpeed;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = finalPos;

        currentLiftEntrance = selectedLiftEntrance;

        LiftReachedDestination?.Invoke(this, new EventArgs());
    }



    private IEnumerator OpenDoorsFromCageCall()
    {
        float time = 1 / doorSpeed;
        float elapsedTime = 0;
        float _moveDist = -(MathF.Abs(doorOpeningDistance));

        Transform entranceLeftDoor = currentLiftEntrance.leftDoor;
        Transform entranceRightDoor = currentLiftEntrance.rightDoor;
        Vector3 startingPosEntranceLeftDoor = entranceLeftDoor.position;
        Vector3 startingPosEntranceRightDoor = entranceRightDoor.position;
        Vector3 startingPosCageLeftDoor = cageLeftDoor.position;
        Vector3 startingPosCageRightDoor = cageRightDoor.position;
        Vector3 finalPosEntranceLeftDoor = new Vector3(entranceLeftDoor.position.x, entranceLeftDoor.position.y, entranceLeftDoor.position.z + _moveDist);
        Vector3 finalPosEntranceRightDoor = new Vector3(entranceRightDoor.position.x, entranceRightDoor.position.y, entranceRightDoor.position.z - _moveDist);
        Vector3 finalPosCageLeftDoor = new Vector3(cageLeftDoor.position.x, cageLeftDoor.position.y, cageLeftDoor.position.z + _moveDist);
        Vector3 finalPosCageRightDoor = new Vector3(cageRightDoor.position.x, cageRightDoor.position.y, cageRightDoor.position.z - _moveDist);

        while (elapsedTime < time)
        {
            entranceLeftDoor.position = Vector3.Lerp(startingPosEntranceLeftDoor, finalPosEntranceLeftDoor, (elapsedTime / time));
            entranceRightDoor.position = Vector3.Lerp(startingPosEntranceRightDoor, finalPosEntranceRightDoor, (elapsedTime / time));
            cageLeftDoor.position = Vector3.Lerp(startingPosCageLeftDoor, finalPosCageLeftDoor, (elapsedTime / time));
            cageRightDoor.position = Vector3.Lerp(startingPosCageRightDoor, finalPosCageRightDoor, (elapsedTime / time));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        entranceLeftDoor.position = finalPosEntranceLeftDoor;
        entranceRightDoor.position = finalPosEntranceRightDoor;
        cageLeftDoor.position = finalPosCageLeftDoor;
        cageRightDoor.position = finalPosCageRightDoor;

        currentLiftEntrance.IsClosed = false;
        DoorAreOpen?.Invoke(this, new EventArgs());

    }



    private IEnumerator CloseDoorsFromCageCall()
    {
        float time = 1 / doorSpeed;
        float elapsedTime = 0;
        float _moveDist = MathF.Abs(doorOpeningDistance);

        Transform entranceLeftDoor = currentLiftEntrance.leftDoor;
        Transform entranceRightDoor = currentLiftEntrance.rightDoor;
        Vector3 startingPosEntranceLeftDoor = entranceLeftDoor.position;
        Vector3 startingPosEntranceRightDoor = entranceRightDoor.position;
        Vector3 startingPosCageLeftDoor = cageLeftDoor.position;
        Vector3 startingPosCageRightDoor = cageRightDoor.position;
        Vector3 finalPosEntranceLeftDoor = new Vector3(entranceLeftDoor.position.x, entranceLeftDoor.position.y, entranceLeftDoor.position.z + _moveDist);
        Vector3 finalPosEntranceRightDoor = new Vector3(entranceRightDoor.position.x, entranceRightDoor.position.y, entranceRightDoor.position.z - _moveDist);
        Vector3 finalPosCageLeftDoor = new Vector3(cageLeftDoor.position.x, cageLeftDoor.position.y, cageLeftDoor.position.z + _moveDist);
        Vector3 finalPosCageRightDoor = new Vector3(cageRightDoor.position.x, cageRightDoor.position.y, cageRightDoor.position.z - _moveDist);

        while (elapsedTime < time)
        {
            entranceLeftDoor.position = Vector3.Lerp(startingPosEntranceLeftDoor, finalPosEntranceLeftDoor, (elapsedTime / time));
            entranceRightDoor.position = Vector3.Lerp(startingPosEntranceRightDoor, finalPosEntranceRightDoor, (elapsedTime / time));
            cageLeftDoor.position = Vector3.Lerp(startingPosCageLeftDoor, finalPosCageLeftDoor, (elapsedTime / time));
            cageRightDoor.position = Vector3.Lerp(startingPosCageRightDoor, finalPosCageRightDoor, (elapsedTime / time));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        entranceLeftDoor.position = finalPosEntranceLeftDoor;
        entranceRightDoor.position = finalPosEntranceRightDoor;
        cageLeftDoor.position = finalPosCageLeftDoor;
        cageRightDoor.position = finalPosCageRightDoor;


        currentLiftEntrance.IsClosed = true;
        DoorAreClosed?.Invoke(this, new EventArgs());

    }



    private IEnumerator CloseDoorFromEntranceCall(LiftEntranceManager _liftEntranceManager)
    {
        float time = 1 / doorSpeed;
        float elapsedTime = 0;
        float _moveDist = MathF.Abs(doorOpeningDistance);

        Transform entranceLeftDoor = _liftEntranceManager.leftDoor;
        Transform entranceRightDoor = _liftEntranceManager.rightDoor;
        Vector3 startingPosEntranceLeftDoor = entranceLeftDoor.position;
        Vector3 startingPosEntranceRightDoor = entranceRightDoor.position;
        Vector3 startingPosCageLeftDoor = cageLeftDoor.position;
        Vector3 startingPosCageRightDoor = cageRightDoor.position;
        Vector3 finalPosEntranceLeftDoor = new Vector3(entranceLeftDoor.position.x, entranceLeftDoor.position.y, entranceLeftDoor.position.z + _moveDist);
        Vector3 finalPosEntranceRightDoor = new Vector3(entranceRightDoor.position.x, entranceRightDoor.position.y, entranceRightDoor.position.z - _moveDist);
        Vector3 finalPosCageLeftDoor = new Vector3(cageLeftDoor.position.x, cageLeftDoor.position.y, cageLeftDoor.position.z + _moveDist);
        Vector3 finalPosCageRightDoor = new Vector3(cageRightDoor.position.x, cageRightDoor.position.y, cageRightDoor.position.z - _moveDist);

        while (elapsedTime < time)
        {
            entranceLeftDoor.position = Vector3.Lerp(startingPosEntranceLeftDoor, finalPosEntranceLeftDoor, (elapsedTime / time));
            entranceRightDoor.position = Vector3.Lerp(startingPosEntranceRightDoor, finalPosEntranceRightDoor, (elapsedTime / time));
            cageLeftDoor.position = Vector3.Lerp(startingPosCageLeftDoor, finalPosCageLeftDoor, (elapsedTime / time));
            cageRightDoor.position = Vector3.Lerp(startingPosCageRightDoor, finalPosCageRightDoor, (elapsedTime / time));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        entranceLeftDoor.position = finalPosEntranceLeftDoor;
        entranceRightDoor.position = finalPosEntranceRightDoor;
        cageLeftDoor.position = finalPosCageLeftDoor;
        cageRightDoor.position = finalPosCageRightDoor;

        _liftEntranceManager.IsClosed = true;

        bool NotAllDoorClosed = liftEntranceManagers.Any(x => x.IsClosed == false);

        if (!NotAllDoorClosed)
        {
            StartCoroutine(MoveCageToFloor());
        }




    }



    public void StartLiftSequenceFromCagePanelSelection(object sender, LiftEntranceManager e)
    {
        selectedLiftEntrance = e;
        HideFloorButtonUI();
        StartCoroutine(CloseDoorsFromCageCall());
    }



    public void ShowFloorButtonUI() => floorButtonsUI.SetActive(true);



    public void HideFloorButtonUI() => floorButtonsUI.SetActive(false);


}

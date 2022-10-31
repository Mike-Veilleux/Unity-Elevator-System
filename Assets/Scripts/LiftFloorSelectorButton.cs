using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiftFloorSelectorButton : MonoBehaviour
{
    public static event EventHandler<LiftEntranceManager> floorSelected;

    public LiftEntranceManager liftEntranceManager;
    [SerializeField] TMP_Text textButton;



    private void OnEnable()
    {
        this.transform.GetComponent<Button>().onClick.AddListener(() => { OnClickedfloorSelected(); });
    }

    

    public void FloorCodeToButtonText()
    {
        textButton.text = Regex.Replace(liftEntranceManager.floorCode.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
    }



    public void OnClickedfloorSelected()
    {
        floorSelected?.Invoke(this, liftEntranceManager);
    }

}

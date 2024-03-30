using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ToggleSettingsPopup : MonoBehaviour
{
    public GameObject settingPopup; 
    private bool isPopupOpen = false; 

    void Start()
    {
        settingPopup.SetActive(false);
    }

    public void TogglePopup()
    {
        isPopupOpen = !isPopupOpen;
        settingPopup.SetActive(isPopupOpen);
    }
}

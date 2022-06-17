using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelChacter : MonoBehaviour
{
   public Toggle[] toggles;

    private void Start()
    {     
        toggles[(int)UserData.CharName].isOn = true;
    }


    public void SelChar1(bool isOn) {  if (isOn) UserData.CharName = CharName.Virtual_Guy ; }
    public void SelChar2(bool isOn) {  if (isOn) UserData.CharName = CharName.Pink_Man; }
    public void SelChar3(bool isOn) {  if (isOn) UserData.CharName = CharName.Mask_Dude; }
    public void SelChar4(bool isOn) { if (isOn) UserData.CharName = CharName.Ninja_Frog; }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HapticGUI;

public class MentorGuiding : MonoBehaviour
{
    // Start is called before the first frame update
    private HapticPlugin HPlugin = null;
    public string DeviceIdentifier = "Right Device";
    private int[] LastButtonsT = new int[4];
    private int[] Buttons = new int[4];
    private int inkwell = 0;
    public HapticPlugin hapticPlugin;
    void Start()
    {
        if (HPlugin == null)
        {

            HapticPlugin[] HPs = (HapticPlugin[])Object.FindObjectsOfType(typeof(HapticPlugin));
            foreach (HapticPlugin HP in HPs)
            {
                if (HP.DeviceIdentifier == "Right Device")
                {
                    HPlugin = HP;
                    Debug.Log("Rentre dans boucle identifiant");

                }
            }

        }
        
        for(int i = 0; i<4; i++)
        {
            Buttons[i] = 0;
            LastButtonsT[i] = 0;
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        
        Debug.Log("ok");
        // getButtons(DeviceIdentifier, Buttons, LastButtonsT, inkwell);
    }
}

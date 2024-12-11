using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System;
using HapticGUI;

public class MentorGuiding : MonoBehaviour
{
    
    [DllImport("HapticsDirect")] public static extern void getButtons(string configName, int[] buttons4, int[] last_buttons4,ref int inkwell); // Get the button, last button states and get whether the inkwell switch, if one exists is active.// Start is called before the first frame update
    [DllImport("HapticsDirect")] public static extern void setSpringValues(string configName, double[] anchor, double magnitude); // Set the parameters of the Spring FX
    private HapticPlugin HPlugin = null;
    public string DeviceIdentifier = "Right Device";
    public string DeviceIdentifierLocal = "Default Device";
    private int[] LastButtonsT = new int[4];
    private int[] Buttons = new int[4];
    private int inkwell = 0;
    public HapticPlugin hapticPlugin;
    public Transform anchorGameObject;
    [Slider(0, 1)]
    public float SpringGMag;

    void Start()
    {        
        for(int i = 0; i<4; i++)
        {
            Buttons[i] = 0;
            LastButtonsT[i] = 0;
            
        }
        
        //Initialisation des paramètre du ressort ici 
        double[] anchor =   new double[3];
        Vector3 position = anchorGameObject.transform.position;
        anchor[0] = (double)position.x;
        anchor[1] = (double)position.y;
        anchor[2] = (double)position.z;
        setSpringValues(DeviceIdentifierLocal, anchor, SpringGMag);
        
    }

    // Update is called once per frame
    void Update()
    {
        // Handling Buttons : For this code to work you have to change the value of the Button 1 and Button 2 corresponding haptic Actor
        // of the Device with DeviceIdentifier identifier. 
        int[] LastButtonsT = new int[4];

        LastButtonsT[0] = Buttons[0];
        LastButtonsT[1] = Buttons[1];
        LastButtonsT[2] = Buttons[2];
        LastButtonsT[3] = Buttons[3];

        getButtons(DeviceIdentifier, Buttons, LastButtonsT,ref inkwell);

        Debug.Log("Button1 = " + Buttons[0] + "  " + LastButtonsT[0]);
        Debug.Log("Button2 = " + Buttons[1] + "  " + LastButtonsT[1]);
        Debug.Log("Button3 = " + Buttons[2] + "  " + LastButtonsT[2]);
        Debug.Log("Button4 = " + Buttons[3] + "  " + LastButtonsT[3]);
        
        //Si le bouton du bas est pressé on enclanche le ressort entre les deux bras. 
        if (Buttons[0] == 1)
        {
            HPlugin.EnableSpring();
        }

    }

}

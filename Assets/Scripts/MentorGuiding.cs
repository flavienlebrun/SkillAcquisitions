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
    [DllImport("HapticsDirect")] public static extern void setForce(string configName, double[] lateral3, double[] torque3); //!< Adds an additional force to the haptic device. 
    [DllImport("HapticsDirect")] public static extern void getCurrentForce(string configName, double[] currentforce3);  //!< Get the current force in N of the device. (Unity CSys)

    private HapticPlugin HPlugin = null;
    public string DeviceIdentifier = "Right Device";
    public string DeviceIdentifierLocal = "Default Device";
    private int[] LastButtonsT = new int[4];
    private int[] Buttons = new int[4];
    private int inkwell = 0;
    private double[] Direction = new double[3];
    private double[] Torque = new double[3] { 10.0, 0.0, 0.0 };
    public HapticPlugin hapticPlugin;
    public Transform anchorGameObject;
    [Slider(0, 1)]
    public float SpringGMag;
    public Vector3 CurrentForce;
    

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
    private static Vector3 DoubleArrayToVector3(double[] darray)
    {
        Vector3 vec3out;

        vec3out.x = (float)darray[0];
        vec3out.y = (float)darray[1];
        vec3out.z = (float)darray[2];

        return vec3out;
    }


    // Update is called once per frame
    void Update()
    {
        // Handling Buttons : For this code to work you have to change the value of the Button 1 and Button 2 corresponding haptic Actor
        // of the Device with DeviceIdentifier identifier. 
        int[] LastButtonsT = new int[4];
        double magnitude = 0.0;
        LastButtonsT[0] = Buttons[0];
        LastButtonsT[1] = Buttons[1];
        LastButtonsT[2] = Buttons[2];
        LastButtonsT[3] = Buttons[3];

        getButtons(DeviceIdentifier, Buttons, LastButtonsT,ref inkwell);

        // Debug.Log("Button1 = " + Buttons[0] + "  " + LastButtonsT[0]);
        // Debug.Log("Button2 = " + Buttons[1] + "  " + LastButtonsT[1]);
        // Debug.Log("Button3 = " + Buttons[2] + "  " + LastButtonsT[2]);
        // Debug.Log("Button4 = " + Buttons[3] + "  " + LastButtonsT[3]);
        
        //Si le bouton du bas est pressé on enclanche le ressort entre les deux bras. 
        if (Buttons[0] == 1)
        {
            double[] temp_double_array = new double[3];
            Debug.Log("On rentre dans la boucle de la force");

            Vector3 positionA = transform.position;
            Debug.Log(positionA);
            Vector3 positionB = anchorGameObject.transform.position;
            Debug.Log(positionB);
            // Calcule le vecteur direction 
            Vector3 direction = positionB - positionA;

            // Normalise le vecteur direction pour obtenir une direction unitaire
            Vector3 directionNormalized = direction.normalized;

            // Convertit le Vector3 en un tableau de double
            double[] directionArray = new double[] { direction.x, direction.y, direction.z };
            double[] directionNormalizedArray = new double[] { directionNormalized.x, directionNormalized.y, directionNormalized.z };

            //ça ne marche pas 
            magnitude = 1.0;
            // setConstantForceValues(DeviceIdentifierLocal, direction, magnitude);

            getCurrentForce(DeviceIdentifierLocal, temp_double_array);
            CurrentForce = DoubleArrayToVector3(temp_double_array);
            Debug.Log(CurrentForce);

        }
        else
        {
            // hapticPlugin.DisableSpring();
        }

    }

}

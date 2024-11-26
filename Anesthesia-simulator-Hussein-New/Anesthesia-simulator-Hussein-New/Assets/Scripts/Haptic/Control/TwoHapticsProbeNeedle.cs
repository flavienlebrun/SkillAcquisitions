using UnityEngine;
using System;
using HD;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using Phidget22;
using Phidget22.Events;
using System.IO;
//using UnityEngine; // Import Unity engine namespace if using Unity
struct Vector6
{
    public float X1 { get; set; }
    public float X2 { get; set; }
    public float X3 { get; set; }
    public float X4 { get; set; }
    public float X5 { get; set; }
    public float X6 { get; set; }

    public Vector6(float x1, float x2, float x3, float x4, float x5, float x6)
    {
        X1 = x1;
        X2 = x2;
        X3 = x3;
        X4 = x4;
        X5 = x5;
        X6 = x6;
    }

    // Method to display the vector
    public override string ToString()
    {
        return $"[{X1}, {X2}, {X3}, {X4}, {X5}, {X6}]";
    }
}
public class VelocityChecker
{
    public int VelocityCheck(float velocity)
    {
        if (Mathf.Abs(velocity) > 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}

public class GeomagicTouchController : MonoBehaviour
{
    // Import the necessary functions from the Geomagic Touch SDK DLL
    [DllImport("HD")]
    private static extern void hdGetDoublev(int pname, double[] values);

    // Constants representing the joint angle queries
    private const int HD_CURRENT_JOINT_ANGLES = 0x2000;

    void Start()
    {
        // Initialize the Geomagic Touch device
        InitializeDevice();
    }

    void Update()
    {
        // Get the joint angles
        float[] jointAngles = GetJointAngles();

        // Print the joint angles to the console
        Debug.Log("Joint Angles: " + string.Join(", ", jointAngles));
    }

    void InitializeDevice()
    {
        // Implementation to initialize the Geomagic Touch device
        // This may involve connecting to the device, starting the scheduler, etc.
    }

    float[] GetJointAngles()
    {
        double[] jointAnglesDouble = new double[6]; // Assuming a 6-DOF arm
        hdGetDoublev(HD_CURRENT_JOINT_ANGLES, jointAnglesDouble);

        float[] jointAnglesFloat = new float[jointAnglesDouble.Length];
        for (int i = 0; i < jointAnglesDouble.Length; i++)
        {
            jointAnglesFloat[i] = (float)jointAnglesDouble[i];
        }
        return jointAnglesFloat;
    }
}





public class TwoHapticsProbeNeedle : MonoBehaviour
{
    // Import the necessary functions from the Geomagic Touch SDK DLL
    [DllImport("HD")]
    private static extern void hdGetDoublev(int pname, double[] values);

    // Constants representing the joint angle queries
    private const int HD_CURRENT_JOINT_ANGLES = 0x2000;
    private StreamWriter writer;
    void Start()
    {
        // Initialize the Geomagic Touch device
        InitializeDevice();
        InitializePhidget();
        writer = new StreamWriter("NeedleForceData.csv");
        writer.WriteLine("NeedleVerticalPosition,ForceY");
    }

    /*void Update()
    {
        // Get the joint angles
        float[] jointAngles = GetJointAngles();

        // Print the joint angles to the console
        Debug.Log("Joint Angles: " + string.Join(", ", jointAngles));
    }*/

    void InitializeDevice()
    {
        // Implementation to initialize the Geomagic Touch device
        // This may involve connecting to the device, starting the scheduler, etc.
    }

    float[] GetJointAngles()
    {
        double[] jointAnglesDouble = new double[6]; // Assuming a 6-DOF arm
        hdGetDoublev(HD_CURRENT_JOINT_ANGLES, jointAnglesDouble);

        float[] jointAnglesFloat = new float[jointAnglesDouble.Length];
        for (int i = 0; i < jointAnglesDouble.Length; i++)
        {
            jointAnglesFloat[i] = (float)jointAnglesDouble[i];
        }
        return jointAnglesFloat;
    }
    public static TwoHapticsProbeNeedle instance { get; private set; }
    public event Action<int> InsertAnesthesic;
    private VoltageRatioInput potentiometer;
    private const float maxPotentiometerValue = 30.0f; // Maximum value of the potentiometer

    private void InitializePhidget()
    {
        potentiometer = new VoltageRatioInput();
        potentiometer.Channel = 0; // Set to the correct channel for your potentiometer
        potentiometer.VoltageRatioChange += Potentiometer_VoltageRatioChange;
        potentiometer.Open();
    }

    private void Potentiometer_VoltageRatioChange(object sender, VoltageRatioInputVoltageRatioChangeEventArgs e)
    {
        float potentiometerValue = (float)e.VoltageRatio * maxPotentiometerValue; // Scale to the max potentiometer value
        float anestheticAmount = CalculateAnestheticAmount(potentiometerValue);

        // Trigger the application of the anesthetic amount based on potentiometer value
        if (anestheticAmount > 1)
        {
            int roundedAnestheticAmount = Mathf.RoundToInt(anestheticAmount);
            MainThreadDispatcher.Enqueue(() => TriggerInsertAnesthesic(roundedAnestheticAmount));
        }
    }

    private float CalculateAnestheticAmount(float potentiometerValue)
    {
        // Calculate the anesthetic amount as a percentage of the potentiometer value relative to the maximum value
        return (potentiometerValue / maxPotentiometerValue) * 100.0f;
    }
    public void TriggerInsertAnesthesic(int amount)
    {
        InsertAnesthesic?.Invoke(amount);
    }
    private void OnDestroy()
    {
        if (potentiometer != null)
        {
            potentiometer.VoltageRatioChange -= Potentiometer_VoltageRatioChange;
            potentiometer.Close();
            potentiometer = null;
        }

    }
    public object GeomagicTouchAPI { get; private set; }

    private PhantomUnityController Phantoms = null;

    [SerializeField]
    private PhantomDeviceInfo ProbeDevice = null;

    [SerializeField]
    public PhantomDeviceInfo NeedleDevice = null;

    [SerializeField]
    private HapticConfig HapticConfig = null;


    /* PROBE PARAMETERS*/

    private float FirstPlanePosition;
    private float SecondPlanePosition;

    // Stiffnes, i.e.k value, of the plane.  Higher stiffness results
    // in a harder surface.
    private float FirstPlaneStiffness;
    private float SecondPlaneStiffness;

    private float ProbeForceStiffness;

    /// <summary>
    /// For determining X, Z restriction movements inside skin layers
    /// </summary>
    private const float OUTSIDE_POSITION = -999;

    /// <summary>
    /// Dimensions (half) of the tissue [mm] to know if it is inside the lateral boundaries
    /// </summary>
    private Vector3 TISSUE_DIMENSIONS;

    /// <summary>
    /// Top position of first layer (Unity units)
    /// </summary>
    private float FIRST_LAYER_TOP;

    /// <summary>
    /// X position of the tip when entering the skin layers
    /// </summary>
    private float NeedleContactPositionX = OUTSIDE_POSITION;

    /// <summary>
    /// Z position of the tip when entering the skin layers
    /// </summary>
    private float NeedleContactPositionZ = OUTSIDE_POSITION;

    /// <summary>
    /// The gimbal position [mm] stored when contact with first membrane (reseted when transpasing it)
    /// </summary>
    private Vector3 NeedlecontactPosition = Vector3.zero;

    /// <summary>
    /// Position of the tip before reaching table
    /// </summary>
    private Vector3 previousPosition;

    /// <summary>
    /// Stiffness coefficient for Skin Layer [N/m]
    /// </summary>
    private float SkinLayerStiffness;

    /// <summary>
    /// Position of the tip when entering the skin layers
    /// </summary>
    private Vector3 lastPosDevice = Vector3.zero;

    /// <summary>
    /// Rotation of the tip when entering the skin layers
    /// </summary>
    private Quaternion lastRotDevice = Quaternion.identity;

    /// <summary>
    /// Rotation matrix of the needle
    /// </summary>
    private double[] RotationMatrix;

    /// <summary>
    /// Stiffness force corresponding to first layer
    /// </summary>
    private float FirstLayerForceStiffness = 0f;

    /// <summary>
    /// Friction force corresponding to first layer
    /// </summary>
    private float FirstLayerForceFriction = 0f;

    /// <summary>
    /// Cutting force corresponding to first layer
    /// </summary>
    private float FirstLayerForceCutting = 0f;

    /// <summary>
    /// Addition of forces in the Y direction
    /// </summary>
    private float forceTotalY = 0f;

    /// <summary>
    /// Membrane forces before traspasing the membrane
    /// </summary>
    private Vector3 membraneForce;

    /// <summary>
    /// Membrane forces before traspasing the membrane
    /// </summary>
    private Vector3 membraneForce2;

    /// <summary>
    /// Exert force scale
    /// </summary>
    private float DEVICE_FORCE_SCALE;

    /// <summary>
    /// Damping coefficient for Skin Layer [N/m]
    /// </summary>
    private float FirstLayerDamping = 1.67f;

    /// <summary>
    /// Cutting coefficient for Skin Layer [N/m]
    /// </summary>
    private float SkinLayerCutting = 1.22f;

    //---------------------------------------------------------------------------
    // SYSTEM CONSTANTS
    //---------------------------------------------------------------------------

    /// <summary>
    /// Unit conversion from mm to Unity
    /// </summary>
    private float UnitLength = 0.01f;

    public Action ZoomUp;
    public Action ZoomDown;

    //public Action<int> InsertAnesthesic;
    public Action<float> ForceProbeApply;

    // way to set up GetButtonDown function
    private Buttons lastLeftButtonsState;
    private Buttons lastRighttButtonsState;

    /// <summary>
    /// Runs only once when it is first activated
    /// </summary>
    private void Awake()
    {
        if (!GameManager.Instance.EnabledHaptic)
        {
            return;
        }

        if (Phantoms == null)
        {
            try
            {
                Debug.Log("Initializing phantoms");

                // Instantiation of Phantoms
                Phantoms = new PhantomUnityController();

                try
                {
                    ProbeDevice.hHdAPI = Phantoms.InitDevice(ProbeDevice.Name);
                    NeedleDevice.hHdAPI = Phantoms.InitDevice(NeedleDevice.Name);
                }
                catch (UnityException)
                {
                    Phantoms = null;
                }
            }
            catch (UnityException)
            {
                Debug.Log("EXCEPTION >> Error trying to conect to PHANTOM devices.\nVerify connection and try again!");
            }
        }
    }

    /// <summary>
    /// When enabled
    /// </summary>
    private void OnEnable()
    {
        if (!GameManager.Instance.EnabledHaptic)
        {
            return;
        }

        Init();

        Debug.Log("INITIALIZING DEVICE...");
        Phantoms.Start();

        // It specifies the method to be executed repeatedly
        Phantoms.AddSchedule(PhantomUpdate, HdAPI.Priority.HD_DEFAULT_SCHEDULER_PRIORITY);
    }

    /// <summary>
    /// When disabled
    /// </summary>
    private void OnDisable()
    {
        if (!GameManager.Instance.EnabledHaptic)
        {
            return;
        }

        Debug.Log("CLOSING DEVICE...");
        try
        {
            if (Phantoms != null)
            {
                //Phantoms.exitHandler();
                Phantoms.Close();
                Phantoms = null;
                Debug.Log("DEVICES CLOSED");
            }
            else
                Debug.Log("DEVICES NOT CONNECTED");
        }
        catch (Exception e)
        {
            Debug.Log("EXCEPTION ON OnDisable");
            Debug.LogException(e);
        }
    }


    /// <summary>
    /// Initialization of the manager
    /// </summary>
    private void Init()
    {
        // Save singleton instance
        if (instance == null)
        {
            instance = this;
        }

        else
            Debug.Log("Multiple instances of HapticManager");
    }

    /// <summary>
    /// Process each frame
    /// </summary>
    private void Update()
    {
        // Get the joint angles
        float[] jointAngles = GetJointAngles();

        // Print the joint angles to the console
        Debug.Log("Joint Angles: " + string.Join(", ", jointAngles));
        if (!GameManager.Instance.EnabledHaptic)
        {
            return;
        }

        FirstPlanePosition = HapticConfig.FirstPlanePosition;
        SecondPlanePosition = HapticConfig.SecondPlanePosition;
        FirstPlaneStiffness = HapticConfig.FirstPlaneStiffness;
        SecondPlaneStiffness = HapticConfig.SecondPlaneStiffness;
        SkinLayerStiffness = HapticConfig.SkinLayerStiffness;

        TISSUE_DIMENSIONS = HapticConfig.TISSUE_DIMENSIONS;
        FIRST_LAYER_TOP = HapticConfig.FIRST_LAYER_TOP;
        DEVICE_FORCE_SCALE = HapticConfig.DEVICE_FORCE_SCALE;
        FirstLayerDamping = HapticConfig.FirstLayerDamping;
        SkinLayerCutting = HapticConfig.SkinLayerCutting;

        UnitLength = HapticConfig.UnitLength;


        // Mise à jour du visuel
        ProbeDevice.tool.transform.localPosition = ProbeDevice.position;
        ProbeDevice.tool.transform.localRotation = ProbeDevice.rotation;

        if (!NeedleDevice.inside)
        {
            NeedleDevice.tool.transform.localPosition = NeedleDevice.position;
            NeedleDevice.tool.transform.localRotation = NeedleDevice.rotation;
        }
        else
        {
            NeedleDevice.tool.transform.localPosition = NeedleDevice.position;
            NeedleDevice.tool.transform.localRotation = NeedleDevice.correctionRotation;
        }

        //VoltageRatioInput potentiometer1;
        //potentiometer.VoltageRatioChange += Potentiometer_VoltageRatioChange;
        float potentiometerValue = (float)potentiometer.VoltageRatio * maxPotentiometerValue; // Scale to the max potentiometer value
        float anestheticAmount = CalculateAnestheticAmount(potentiometerValue);
        InsertAnesthesic.Invoke(Mathf.RoundToInt(anestheticAmount));
        // Mise à jour des boutons 
        /*HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
        Buttons bStateLeft = Phantoms.GetButton();

        if (bStateLeft == Buttons.Button1 && lastLeftButtonsState != bStateLeft)
        {
            ZoomDown.Invoke();
        }

        if (bStateLeft == Buttons.Button2 && lastLeftButtonsState != bStateLeft)
        {
            ZoomUp.Invoke();
        }

        HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);
        Buttons bStateRight = Phantoms.GetButton();
        if ((bStateRight == Buttons.Button1 || bStateRight == Buttons.Button2) && lastRighttButtonsState != bStateRight)
        {
            InsertAnesthesic.Invoke(20);
        }

        lastLeftButtonsState = bStateLeft;
        lastRighttButtonsState = bStateRight;*/

        ForceProbeApply.Invoke(ProbeForceStiffness);
    }

    float probeDop = 0f;
    float probeDopStiffness = 0f;

    Vector3 ProbeCurrentPosition;
    /// <summary>
    /// Method that is repeatedly called in PHANTOM's cycle (default rate 1 [kHz])
    /// </summary>
    /// <returns><c>true</c>, if update was phantomed, <c>false</c> otherwise.</returns>
    bool PhantomUpdate()
    {
        /* PROBE */

        HdAPI.hdBeginFrame(ProbeDevice.hHdAPI);

        Vector3 ProbeHandPosition = Phantoms.GetPosition();
        Quaternion ProbeHandRotation = Phantoms.GetRotation();

        // If the user has penetrated the plane, set the device force to 
        // repel the user in the direction of the surface normal of the plane.
        // Penetration occurs if the plane is facing in +Y and the user's Y position
        // is negative, or vice versa.
        if (Mathf.Abs(ProbeHandPosition.x) < TISSUE_DIMENSIONS.x && Mathf.Abs(ProbeHandPosition.z) < TISSUE_DIMENSIONS.z)
        {
            // get position from top 1st layer position
            ProbeCurrentPosition = ProbeHandPosition * UnitLength;
            float ProbeDopStiffness = FirstPlanePosition + 0.075f - ProbeCurrentPosition.y;

            if (ProbeDopStiffness > 0)
            {
                //---------------------------------------------------------------------------
                // MEMBRANE STIFFNESS FORCE (before penetration)
                //---------------------------------------------------------------------------

                Vector3 ProbeHandVelocity = Phantoms.GetVelocity();
                // get velocity and limit it
                float ProbeVelocity = ProbeHandVelocity.y;
                ProbeVelocity = Mathf.Clamp(ProbeVelocity, -0.5f, 0.5f);

                // calculate stiffness force (Y direction)
                ProbeForceStiffness = (2.5f + SkinLayerStiffness) * ProbeDopStiffness + FirstLayerDamping * (-ProbeVelocity) * ProbeDopStiffness;

                // apply scale factor for forces
                ProbeForceStiffness *= DEVICE_FORCE_SCALE;

                float membraneDamping = 0.003f;
                float membraneStiffness = 1f;
                float distanceCoeficient = 0.008f;
                float ClampValue = 2f;
                float coeffOfFriction = 0.1f;
                Vector3 frictionForce = -coeffOfFriction * ProbeHandVelocity.normalized; //friction force due to friction between probe and skin
                // lateral forces within the membrane: damping force
                membraneForce2 = -membraneDamping * ProbeHandVelocity;
                if (membraneForce2.magnitude > ClampValue)
                {
                    membraneForce2.Normalize();
                    membraneForce2 *= ClampValue;
                }

                Vector3 ForceS = Vector3.zero;
                ForceS.x += membraneForce2.x - ProbeDopStiffness * distanceCoeficient + frictionForce.x;
                ForceS.y += membraneForce2.y + frictionForce.y;
                ForceS.z += membraneForce2.z - ProbeDopStiffness * distanceCoeficient + frictionForce.z;

                // lateral forces within the membrane: dynamic stiffness force
                ClampValue = (float)Phantoms.GetContinuousForceLimit();
                membraneForce2 = membraneStiffness * (Vector3.zero - ProbeHandPosition);
                if (membraneForce2.magnitude > ClampValue)
                {
                    membraneForce2.Normalize();
                    membraneForce2 *= ClampValue;
                }

                ForceS.x += membraneForce2.x;
                ForceS.y += membraneForce2.y;
                ForceS.z += membraneForce2.z;
                ProbeDevice.force = ForceS;
            }

            if (ProbeHandPosition.y <= FirstPlanePosition) //0 la pos en y du plane
            {
                // Create a force vector repelling the user from the plane proportional
                // to the penetration distance, using F=kx where k is the plane 
                // stiffness and x is the penetration vector.  Since the plane is 
                // oriented at the Y=0, the force direction is always either directly 
                // upward or downward, i.e. either (0,1,0) or (0,-1,0).
                // Hooke's law explicitly
                float penetrationDistance = Mathf.Abs(ProbeHandPosition.y);
                Vector3 ProbeHandVelocity = Phantoms.GetVelocity();
                if (ProbeHandPosition.y > SecondPlanePosition)
                {
                    ProbeDevice.force += new Vector3(0, (float)(penetrationDistance * FirstPlaneStiffness), 0);
                    ProbeHandPosition *= UnitLength;
                    ProbeDevice.position = new Vector3(ProbeHandPosition.x, ProbeHandPosition.y, ProbeHandPosition.z);
                    ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
                }
                else
                {
                    ProbeDevice.force += new Vector3(0, (float)(penetrationDistance * SecondPlaneStiffness), 0);
                    ProbeHandPosition *= UnitLength;
                    ProbeDevice.position = new Vector3(ProbeHandPosition.x, SecondPlanePosition * UnitLength, ProbeHandPosition.z);
                    ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
                }
            }
            else
            {
                ProbeDevice.force += Vector3.zero;
                ProbeHandPosition *= UnitLength;
                ProbeDevice.position = new Vector3(ProbeHandPosition.x, ProbeHandPosition.y, ProbeHandPosition.z);
                ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
            }
        }
        else
        {
            ProbeDevice.force = Vector3.zero;
            ProbeHandPosition *= UnitLength;
            ProbeDevice.position = new Vector3(ProbeHandPosition.x, ProbeHandPosition.y, ProbeHandPosition.z);
            ProbeDevice.rotation = new Quaternion(ProbeHandRotation.x, ProbeHandRotation.y, ProbeHandRotation.z, ProbeHandRotation.w);
        }

        HdAPI.hdMakeCurrentDevice(ProbeDevice.hHdAPI);
        Phantoms.SetForce(ProbeDevice.force);

        /***************************************************/

        /* NEEDLE */

        HdAPI.hdBeginFrame(NeedleDevice.hHdAPI);
        HdAPI.hdMakeCurrentDevice(NeedleDevice.hHdAPI);

        // Get the position of the hand (gimbal part) [mm]
        Vector3 NeedleHandPosition = Phantoms.GetPosition();

        // Get the hand posture (orientation)
        Quaternion NeedleHandRotation = NeedleDevice.correctionRotation = Phantoms.GetRotation();

        // Get the speed of the hand [mm/s]
        Vector3 NeedleHandVelocity = Phantoms.GetVelocity();

        // Re-init force feedback to 0
        //Vector3 Force = Vector3.zero;
        NeedleDevice.force = Vector3.zero;

        // Hand position & rotation in the Unity world
        Vector3 currentPosition = NeedleHandPosition * UnitLength;
        Quaternion currentRotation = NeedleHandRotation;
        //currentPosition.y = currentPosition.y - 0.25f;
        // init forces to apply to haptic in the Y direction
        FirstLayerForceStiffness = FirstLayerForceFriction = FirstLayerForceCutting = forceTotalY = 0f;

        // get rotation matrix to get direction of the needle when penetrating
        Phantoms.GetRotationMatrix(out RotationMatrix); // sortir de cette boucle je crois pour le point pivot

        //---------------------------------------------------------------------------
        // FORCES FROM TISSUE - NEEDLE INTERACTION (1st and 2nd layer)
        //---------------------------------------------------------------------------

        // if within the square of tissue in X, Z coordinates (big cube with all tissue layers inside)
        if (Mathf.Abs(NeedleHandPosition.x) < TISSUE_DIMENSIONS.x && Mathf.Abs(NeedleHandPosition.z) < TISSUE_DIMENSIONS.z)
        {
            // get vertical position of the needle
            float NeedleVerticalPosition = NeedleHandPosition.y * UnitLength;

            // if it has traspased the membrane
            if (NeedleVerticalPosition < FIRST_LAYER_TOP)// - 0.05)
            {
                NeedlecontactPosition = Vector3.zero;

                // set contact position, store position and rotation of needle at the moment of penetration
                if (NeedleContactPositionX == OUTSIDE_POSITION && NeedleContactPositionZ == OUTSIDE_POSITION)
                {
                    NeedleContactPositionX = currentPosition.x;
                    NeedleContactPositionZ = currentPosition.z;
                    lastPosDevice = currentPosition;
                    lastRotDevice = NeedleHandRotation;

                    // needle is inside tissue
                    NeedleDevice.correctionPosition = new Vector3(NeedleContactPositionX, FIRST_LAYER_TOP, NeedleContactPositionZ);
                    NeedleDevice.inside = true;
                }
            }
            else
            {
                NeedleDevice.inside = false;
                NeedleContactPositionX = NeedleContactPositionZ = OUTSIDE_POSITION;
            }

            // init depth variables and velocity
            probeDop = 0f;
            probeDopStiffness = 0f;
            float NeedleVelocity = 0f;

            // limit visual direction if needle inside tissue and calculate lateral forces
            if (NeedleContactPositionX != OUTSIDE_POSITION && NeedleContactPositionZ != OUTSIDE_POSITION)
            {
                //---------------------------------------------------------------------------
                // INSIDE SKIN LAYERS LATERAL FORCES ADDITION
                //---------------------------------------------------------------------------

                currentRotation = lastRotDevice;
                float t = (currentPosition.y - lastPosDevice.y) / -(float)RotationMatrix[9];

                // Temporal variable to calculate the lateral forces to limit position
                Vector3 lateralInsideForce = Vector3.zero;
                lateralInsideForce.x = CalculateLateralForce(currentPosition, NeedleHandVelocity, lastPosDevice, 0).x;
                lateralInsideForce.z = CalculateLateralForce(currentPosition, NeedleHandVelocity, lastPosDevice, 2).z;

                // add lateral forces
                NeedleDevice.force += lateralInsideForce;

                //---------------------------------------------------------------------------

                // update current position with limits in the X and Z position
                currentPosition = new Vector3(t * -(float)RotationMatrix[8] + lastPosDevice.x, currentPosition.y, t * (float)RotationMatrix[10] + lastPosDevice.z);

                // depth in the skin from penetration point
                probeDop = (currentPosition - lastPosDevice).magnitude;
            }

            //---------------------------------------------------------------------------
            // FIRST LAYER FORCE ADDITION
            //---------------------------------------------------------------------------

            // limit depthness
            probeDop = Mathf.Clamp(probeDop, 0f, 0.35f);

            // get position from top 1st layer position
            probeDopStiffness = FIRST_LAYER_TOP + 0.025f - currentPosition.y;

            if (probeDopStiffness > 0 && probeDop == 0)
            {
                //---------------------------------------------------------------------------
                // MEMBRANE STIFFNESS FORCE (before penetration)
                //---------------------------------------------------------------------------

                if (NeedlecontactPosition == Vector3.zero)
                    NeedlecontactPosition = NeedleHandPosition;

                // get velocity and limit it
                NeedleVelocity = NeedleHandVelocity.y;
                NeedleVelocity = Mathf.Clamp(NeedleVelocity, -0.5f, 0.5f);
                float a1 = 48f;
                float a2 = 5.2f;
                float Cuttingforce = 0f;
                // calculate stiffness force (Y direction)
                if (currentPosition.y <= FIRST_LAYER_TOP && currentPosition.y >= FIRST_LAYER_TOP - 0.022)
                {
                    FirstLayerForceStiffness = a1 * (currentPosition.y - FIRST_LAYER_TOP) + a2 * (currentPosition.y - FIRST_LAYER_TOP) * (currentPosition.y - FIRST_LAYER_TOP);
                    //FirstLayerForceStiffness = a1 * probeDopStiffness + a2 * probeDopStiffness * probeDopStiffness;
                    //FirstLayerForceStiffness = (2.5f + SkinLayerStiffness) * probeDopStiffness + FirstLayerDamping * (-NeedleVelocity) * probeDopStiffness;
                    FirstLayerForceCutting = 30f;
                    Cuttingforce = (FirstLayerForceCutting / 0.00125f) * (currentPosition.y - FIRST_LAYER_TOP);
                }


                // apply scale factor for forces
                //FirstLayerForceStiffness *= DEVICE_FORCE_SCALE;
                FirstLayerForceCutting = SkinLayerCutting;
                forceTotalY = FirstLayerForceStiffness + Cuttingforce;

                float membraneDamping = 0.003f;
                float membraneStiffness = 0.04f;
                float distanceCoeficient = 0.08f;
                float ClampValue = 0.4f;

                // lateral forces within the membrane: damping force
                membraneForce = -membraneDamping * NeedleHandVelocity;
                if (membraneForce.magnitude > ClampValue)
                {
                    membraneForce.Normalize();
                    membraneForce *= ClampValue;
                }

                NeedleDevice.force.x += membraneForce.x - probeDopStiffness * distanceCoeficient;
                NeedleDevice.force.y += membraneForce.y;
                NeedleDevice.force.z += membraneForce.z - probeDopStiffness * distanceCoeficient; ;

                // lateral forces within the membrane: dynamic stiffness force
                ClampValue = (float)Phantoms.GetContinuousForceLimit();
                membraneForce = membraneStiffness * (NeedlecontactPosition - NeedleHandPosition);
                if (membraneForce.magnitude > ClampValue)
                {
                    membraneForce.Normalize();
                    membraneForce *= ClampValue;
                }

                NeedleDevice.force += membraneForce;

                if ((NeedlecontactPosition - NeedleHandPosition).magnitude > 5)
                    NeedlecontactPosition = NeedleHandPosition;

                //---------------------------------------------------------------------------
            }
            else if (probeDop > 0 && NeedleVerticalPosition < FIRST_LAYER_TOP - 0.022)
            {
                //---------------------------------------------------------------------------
                // TISSUE FRICTION + CUTTING FORCE (after penetration)
                //---------------------------------------------------------------------------
                // Define the target force and initialize the current force
                float Gapforce = 0f;
                /*if (NeedleVerticalPosition - FIRST_LAYER_TOP > -0.028)
                {
                    Gapforce = 0.851083f * (FIRST_LAYER_TOP - currentPosition.y);//8.51083f
                    Gapforce *= DEVICE_FORCE_SCALE;
                }*/
                if (NeedleVerticalPosition - FIRST_LAYER_TOP < -0.025)
                {   //float f0 = 0.185f;
                    //float a0 = 0.12f;
                    //float b0 = -0.097f;
                    float Cn = -11.96f;
                    float Cp = 10.57f;
                    float bn = -320f;
                    float bp = 260f;
                    float Dn = -18.23f;
                    float Dp = 18.45f;
                    // get velocity and limit it
                    NeedleVelocity = NeedleHandVelocity.y * UnitLength;
                    NeedleVelocity = Mathf.Clamp(NeedleVelocity, -2f, 2f);
                    FirstLayerForceFriction = Gapforce;
                    if (NeedleVelocity < -0.01)
                    {
                        FirstLayerForceFriction = (Cn * Math.Sign(NeedleVelocity) + bn * NeedleVelocity) * 0.008f;
                    }
                    else if (NeedleVelocity >= 0.01)
                    {
                        FirstLayerForceFriction = (Cp * Math.Sign(NeedleVelocity) + bp * NeedleVelocity) * 0.008f;
                    }
                    else if (NeedleVelocity <= 0 && NeedleVelocity > -0.01)
                    {
                        FirstLayerForceFriction = Math.Max(forceTotalY, Dn) * 0.008f;
                    }
                    if (NeedleVelocity > 0 && NeedleVelocity < 0.01)
                    {
                        FirstLayerForceFriction = Math.Max(forceTotalY, Dp) * 0.008f;
                    }
                }
                // calculate friction force
                //FirstLayerForceFriction = (-NeedleVelocity * 3 + 800 * ((f0 + b0) * Mathf.Exp(a0 * probeDopStiffness) + b0)) / FirstLayerDamping;
                float ClampValue = 0.4f;
                ClampValue = (float)Phantoms.GetContinuousForceLimit();
                // apply scale factor for forces
                FirstLayerForceFriction *= DEVICE_FORCE_SCALE;
                FirstLayerForceFriction *= ClampValue;
                // add cutting force (= constant)
                FirstLayerForceCutting = SkinLayerCutting;
                VelocityChecker checker = new VelocityChecker();
                //forceTotalY = 0f;
                forceTotalY = checker.VelocityCheck(NeedleHandVelocity.y) * FirstLayerForceFriction + Gapforce + 1f;// + FirstLayerForceFriction; //+ checker.VelocityCheck(NeedleHandVelocity.y) * FirstLayerForceCutting;
                //Console.WriteLine(forceTotalY);


            }
            else
            {
            NeedlecontactPosition = Vector3.zero;
            }

            //NeedleDevice.force.y += 0f;
            NeedleDevice.force.y += forceTotalY;
            float needleForceY = NeedleDevice.force.y;
            float relativePosition = -NeedleVerticalPosition + FIRST_LAYER_TOP;

            // Log data to file
            writer.WriteLine($"{relativePosition},{needleForceY}");
            void OnDestroy()
            {
                writer.Close();
            }
            //---------------------------------------------------------------------------
        }
        else
        {
            NeedlecontactPosition = Vector3.zero;
            NeedleDevice.inside = false;

            // reset contact position
            NeedleContactPositionX = NeedleContactPositionZ = OUTSIDE_POSITION;
        }
        float Theta1;
        float Theta2;
        float Theta3;
        float Theta4;
        float Theta5;
        float Theta6;
        float[] jointAngles = new float[6];
        // Get the joint angles
        jointAngles = GetJointAngles();

        // Update your robotic arm with the new joint angles
        //UpdateRobotArm(jointAngles);

        
        Theta1 = jointAngles[0];
        Theta2 = jointAngles[1];
        Theta3 = jointAngles[2];
        Theta4 = jointAngles[3];
        Theta5 = jointAngles[4];
        Theta6 = jointAngles[5];
        float sinTheta1 = (float)Math.Sin(Theta1);
        float cosTheta1 = (float)Math.Cos(Theta1);
        float sinTheta2PlusTheta3 = (float)Math.Sin(Theta2 + Theta3);
        float cosTheta2PlusTheta3 = (float)Math.Cos(Theta2 + Theta3);
        float sinTheta4 = (float)Math.Sin(Theta4);
        float cosTheta4 = (float)Math.Cos(Theta4);
        float sinTheta5 = (float)Math.Sin(Theta5);
        float cosTheta5 = (float)Math.Cos(Theta5);
        float[,] T = new float[4, 4]
        {
            {
                (sinTheta2PlusTheta3 * sinTheta5 + cosTheta2PlusTheta3 * cosTheta4 * cosTheta5) * cosTheta1 - cosTheta1 * cosTheta5 * sinTheta4,
                -sinTheta1,
                (sinTheta2PlusTheta3 * cosTheta5 - cosTheta2PlusTheta3 * cosTheta4 * sinTheta5) * cosTheta1 + cosTheta1 * sinTheta4 * sinTheta5,
                cosTheta1 * ((float)Math.Cos(Theta2) + (float)Math.Cos(Theta2 + Theta3))
            },
            {
                (sinTheta2PlusTheta3 * sinTheta5 + cosTheta2PlusTheta3 * cosTheta4 * cosTheta5) * sinTheta1 - cosTheta5 * sinTheta1 * sinTheta4,
                cosTheta1,
                (sinTheta2PlusTheta3 * cosTheta5 - cosTheta2PlusTheta3 * cosTheta4 * sinTheta5) * sinTheta1 + sinTheta1 * sinTheta4 * sinTheta5,
                sinTheta1 * ((float)Math.Cos(Theta2) + (float)Math.Cos(Theta2 + Theta3))
            },
            {
                -cosTheta2PlusTheta3 * sinTheta5 + sinTheta2PlusTheta3 * cosTheta4 * cosTheta5,
                0,
                -cosTheta2PlusTheta3 * cosTheta5 - sinTheta2PlusTheta3 * cosTheta4 * sinTheta5,
                1 + (float)Math.Cos(Theta2) + (float)Math.Cos(Theta2 + Theta3)
            },
            { 0, 0, 0, 1 }
        };
        float[,] R = new float[3, 3]
        {
            {T[0, 0], T[0, 1], T[0, 2]},
            {T[1, 0], T[1, 1], T[1, 2]},
            {T[2, 0], T[2, 1], T[2, 2]},
        };
        Vector3 S = new Vector3(T[0, 3], T[1, 3], T[2, 3]);
        Vector3 armForce = MultiplyMatrixByVector(R, NeedleDevice.force);
        /*NeedleDevice.force.y = armForce.y;
        NeedleDevice.force.x = armForce.x;
        NeedleDevice.force.z = -armForce.z;*/
        //---------------------------------------------------------------------------
        // Force feedback to PHANTOM device [N]
        Phantoms.SetForce(armForce);
        //Phantoms.SetForce(new Vector3(0, 0, 0));

        // set position and orientation for graphic needle
        NeedleDevice.position = currentPosition;
        NeedleDevice.rotation = currentRotation;

        previousPosition = NeedleDevice.position;

        HdAPI.hdEndFrame(NeedleDevice.hHdAPI);
        HdAPI.hdEndFrame(ProbeDevice.hHdAPI);
        return true;
        
        
    }

    public static Vector3 MultiplyMatrixByVector(float[,] matrix, Vector3 vector)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3)
            throw new ArgumentException("The matrix must be 3x3.");

        float x = matrix[0, 0] * vector.x + matrix[0, 1] * vector.y + matrix[0, 2] * vector.z;
        float y = matrix[1, 0] * vector.y + matrix[1, 1] * vector.y + matrix[1, 2] * vector.z;
        float z = -(matrix[2, 0] * vector.x + matrix[2, 1] * vector.y + matrix[2, 2] * vector.z);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Seek the forces generated when the operating point is in contact with the lateral membrane
    /// </summary>
    /// <param name="tipPosition">The position of the tip [mm]</param>
    /// <param name="tipVelocity">The speed of the tip [mm/s]</param>
    /// <param name="lateralPosition">The stored lateral position</param>
    /// <param name="axe">To determine if it is X (<code>0</code>) or Z (<code>2</code>)</param>
    /// <returns>The force to apply</returns>
    private Vector3 CalculateLateralForce(Vector3 tipPosition, Vector3 tipVelocity, Vector3 lateralPosition, int axe)
    {
        // local constants
        const float BOUNDARY = 0.0f;
        const float STIFFNESS = 25f;
        const float DUMPING = 0.0f;
        const float FORCE_LIMIT = 3.0f;

        // Calculate the difference from the tip to the object center
        Vector3 differencePositions = tipPosition - lateralPosition;

        // The distance to planar object is assumed in the Y axis
        float distance = differencePositions[axe];

        // No force in the outside of the BOUNDARY with object and no visual feedback
        if (distance == 0) return Vector3.zero;

        // No forces with other planes different from axe
        for (int i = 0; i < 3; i++)
            if (i != axe) differencePositions[i] = 0;

        // Normalisation
        differencePositions /= distance;

        // STIFFNESS force calculation
        float force = STIFFNESS * (BOUNDARY - distance);

        // Restrict to max force
        if (force > FORCE_LIMIT) force = FORCE_LIMIT;

        return (force * differencePositions) - DUMPING * tipVelocity;
    }
    static float[] MultiplyMatrixByVector(float[,] matrix, float[] vector)
    {
        int matrixRows = matrix.GetLength(0);
        int matrixCols = matrix.GetLength(1);

        if (matrixCols != vector.Length)
            throw new ArgumentException("Matrix columns must match vector length.");

        float[] result = new float[matrixRows];

        for (int i = 0; i < matrixRows; i++)
        {
            float sum = 0;
            for (int j = 0; j < matrixCols; j++)
            {
                sum += matrix[i, j] * vector[j];
            }
            result[i] = sum;
        }

        return result;
    }





}



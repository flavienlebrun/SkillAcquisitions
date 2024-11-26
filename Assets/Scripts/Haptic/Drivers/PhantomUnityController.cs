/**
 * ------------------------------------------------
 * ManagedPhantom
 * 
 * General PHANToM for Unity
 * 
 * Copyright (c) 2017 Ricca
 * 
 * REQUIREMENTS
 *  - PHANTOM haptic device
 *  - PHANTOM Device Drivers
 *  - HdAPI.dll (Sensable OpenHaptics Toolkit)
 * 
 * ------------------------------------------------
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using HD;

public class PhantomUnityController
{
    //uint HdAPI; //todelete
    List<uint> HdAPIHandles;                                   // the haptic devices handles
    int ConnectedDevices;                                   // the number of devices instances
    List<HdAPI.SchedulerCallback> CallbackMethods;             // the methods when GC lost reference
    private List<ulong> ScheduleHandles;                    // the handle at the time of scheduling in HdAPIAPI
    private Buttons CurrentButtons = Buttons.None;          // current PHANTOM buttons state
    private Buttons LastButtons = Buttons.None;             // PHANTOM buttons at the time of the last Update

    /// <summary>
    /// Pen tip coordinates relative to the gimbal portion [mm] (PHANTOM coordinate system)
    /// </summary>
    public Vector3 TipOffset = new Vector3(0.0f, 0.0f, -40.0f);

    /// <summary>
    /// Movable range limit [mm]
    /// </summary>
    public Vector3 WorkspaceMinimum { get; private set; }

    /// <summary>
    /// Movable range limit [mm]
    /// </summary>
    public Vector3 WorkspaceMaximum { get; private set; }

    /// <summary>
    /// Recommended movable range limit [mm]
    /// </summary>
    public Vector3 UsableWorkspaceMinimum { get; private set; }

    /// <summary>
    /// Recommended movable range limit [mm]
    /// </summary>
    public Vector3 UsableWorkspaceMaximum { get; private set; }

    /// <summary>
    /// Y-coordinate corresponding to the surface of the desk [mm]
    /// </summary>
    public float TableTopOffset { get; private set; }

    /// <summary>
    /// is true if running process in PHANTOM
    /// </summary>
    public bool IsRunning = false;

    /// <summary>
    /// This method is called asynchronously
    /// </summary>
    /// <returns>true: main ongoing, false: completion</returns>
    public delegate bool Callback();

    /// <summary>
    /// 
    /// </summary>
    public enum Devices : int
    {
        PHANToM_01 = -1,
        PHANToM_02 = 1,
        DEFAULT_PHANToM = 0
    }

    /// <summary>
    /// Connect to the two devices
    /// </summary>
    public PhantomUnityController()
    {
        IsRunning = false;
        ConnectedDevices = 0;
        HdAPIHandles = new List<uint>();

        // List to hold the callback methods
        CallbackMethods = new List<HdAPI.SchedulerCallback>();

        // Hold the handle of the scheduled methods
        ScheduleHandles = new List<ulong>();

        // Get the movable range
        //LoadWorkspaceLimit();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deviceName"></param>
    /// <returns></returns>
    public uint InitDevice(string deviceName)
    {
        uint hHdAPI = (uint)ConnectedDevices;
        try
        {
            hHdAPI = HdAPI.hdInitDevice(deviceName);
            ErrorCheck("Initialize device " + deviceName);
        }
        catch (UnityException)
        {
            hHdAPI = (uint)ConnectedDevices;
        }

        //string modelType = HdAPI.HdAPIGetString(HdAPI.ParameterNameGet.HdAPI_DEVICE_MODEL_TYPE);

        Debug.Log("New PHANToM device '" + deviceName + " created with handler " + hHdAPI);

        HdAPIHandles.Add(hHdAPI);
        HdAPI.hdEnable(HdAPI.Capability.HD_FORCE_OUTPUT);
        Debug.Log("Enabled force output for device " + deviceName + " [handler=" + hHdAPI + "]");
        HdAPI.hdEnable(HdAPI.Capability.HD_FORCE_RAMPING);
        Debug.Log("Enabled force ramping for device " + deviceName + " [handler=" + hHdAPI + "]");
        //ErrorCheck("Enable force output");
        ConnectedDevices++;

        return hHdAPI;
    }

    /// <summary>
    /// Finished using the device, and then disconnect
    /// </summary>
    public void Close()
    {
        Stop();
        ClearSchedule();

        foreach (var hHdAPI in HdAPIHandles)
        {
            //HdAPI.hdDisableDevice(hHdAPI);
            //ErrorCheck("Disable device");

            Debug.Log("Device with handler " + hHdAPI + " disabled");

            //hHdAPI = (uint)HdAPI.DeviceHandle.HdAPI_INVALID_HANDLE;
        }

        /*if (hHdAPI != (uint)HdAPI.DeviceHandle.HdAPI_INVALID_HANDLE)
        {
            HdAPI.HdAPIDisableDevice(hHdAPI);
            ErrorCheck("Disable device");

            hHdAPI = (uint)HdAPI.DeviceHandle.HdAPI_INVALID_HANDLE;
        }*/
    }

    #region Scheduling

    /// <summary>
    /// To start the asynchronous processing
    /// </summary>
    public void Start()
    {
        //if (!IsAvailable || IsRunning) return;

 /*       // ON : the standard is to generate a force
        Debug.Log("Enabling force output");
        HdAPI.HdAPIEnable(HdAPI.Capability.HdAPI_FORCE_OUTPUT);
        ErrorCheck("Enable force output");
        */
        // Also started asynchronous processing
        HdAPI.hdStartScheduler();
        ErrorCheck("Start scheduler");

        LoadWorkspaceLimit();
        
        uint hHdAPI = HdAPIHandles[0];

        HdAPI.hdMakeCurrentDevice(hHdAPI);

        IsRunning = true;
    }

    /// <summary>
    /// Stop the asynchronously processing
    /// </summary>
    public void Stop()
    {
        //if (!IsAvailable || !IsRunning) return;

        IsRunning = false;

        ////System.Threading.Thread.Sleep (10);
        //foreach (uint handle in ScheduleHandles)
        //{
        //	HdAPI.HdAPIWaitForCompletion(handle, HdAPI.WaiteCode.HdAPI_WAIT_INFINITE);
        //	ErrorCheck("Waiting for completion");
        //}
        HdAPI.hdStopScheduler();
        ErrorCheck("StopScheduler");

        Debug.Log("Scheduler stoped");
        /*

		// Force also stop
		HdAPI.hdDisable(HdAPI.Capability.HdAPI_FORCE_OUTPUT);
		ErrorCheck("Disable force output");*/
    }

    /// <summary>
    /// Call the synchronously processing
    /// </summary>
    public void Do(Callback callback)
    {
        HdAPI.hdScheduleSynchronous(
            (data) => { return DoCallback(callback); },
            IntPtr.Zero,
            HdAPI.Priority.HD_DEFAULT_SCHEDULER_PRIORITY
            );
        ErrorCheck("ScheduleSynchronous");
    }

    /// <summary>
    /// Add a method to the asynchronous execution
    /// </summary>
    /// <param name="callback">Callback method that returns true if main continues</param>
    public void AddSchedule(Callback callback, ushort priority)
    {
        HdAPI.SchedulerCallback method = (data) =>
        {
            return DoCallback(callback);
        };
        CallbackMethods.Add(method);

        ulong handle = HdAPI.hdScheduleAsynchronous(
            method,
            IntPtr.Zero,
            priority
            );
        ErrorCheck("ScheduleAsynchronous");
        Debug.Log("Scheduler handler " + handle + " added");
        ScheduleHandles.Add(handle);
    }

    /// <summary>
    /// Wrap in order to further simplify the callback method calls
    /// </summary>
    /// <param name="callback">Methods only to simplify return a bool with no arguments</param>
    /// <returns>Once completed</returns>
    private HdAPI.CallbackResult DoCallback(Callback callback)
    {
        bool result = true;
        result = callback();
        /*foreach (var hHdAPI in HdAPIHandles)
        {
            HdAPI.HdAPIBeginFrame(hHdAPI);
            ErrorCheck("BeginFrame");
        }

        result = callback();

        foreach (var hHdAPI in HdAPIHandles)
        {
            HdAPI.HdAPIEndFrame(hHdAPI);
            ErrorCheck("EndFrame");
        }*/

        return (result ? HdAPI.CallbackResult.HD_CALLBACK_CONTINUE : HdAPI.CallbackResult.HD_CALLBACK_DONE);
    }

    /// <summary>
    /// Clear all the asynchronous execution processing registered
    /// </summary>
    public void ClearSchedule()
    {
        Debug.Log("Removing scheduler handles");

        foreach (uint handle in ScheduleHandles)
        {
            HdAPI.hdUnschedule(handle);
            ErrorCheck("Unschedule #" + handle.ToString());

            Debug.Log("Scheduler handle " + handle + " was removed");
        }
        ScheduleHandles.Clear();
        CallbackMethods.Clear();
    }

    /// <summary>
    /// Process to specify whether this will be called many times per second
    /// </summary>
    /// <param name="rate">Cycle [Hz] 500 or 1000</param>
    public void SetSchedulerRate(uint rate)
    {
        HdAPI.hdSetSchedulerRate(rate);
        Debug.Log("Scheduler working rate set to " + rate);
        ErrorCheck("Set scheduler rate");
    }

    #endregion

    #region Information Acquisition

    /// <summary>
    /// It returns the current PHANTOM hand coordinate
    /// </summary>
    /// <returns>Gimbal coordinate [mm]</returns>
    public Vector3 GetPosition()
    {
        double[] position = new double[3] { 0, 0, 0 };
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_POSITION, position);
        return new Vector3((float)position[0], (float)position[1], -(float)position[2]);
    }

    /// <summary>
    /// It returns the current PHANTOM hand speed
    /// </summary>
    /// <returns>Velocity vector [mm / s]</returns>
    public Vector3 GetVelocity()
    {
        double[] velocity = new double[3] { 0, 0, 0 };
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_VELOCITY, velocity);
        return new Vector3((float)velocity[0], (float)velocity[1], -(float)velocity[2]);
    }

    /// <summary>
    /// It returns the coordinates of the pen tip
    /// </summary>
    /// <returns>Pen tip coordinate [mm]</returns>
    public Vector3 GetTipPosition()
    {
        double[] position = new double[3] { 0, 0, 0 };
        double[] matrix = new double[16];
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_POSITION, position);
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_TRANSFORM, matrix);

        Vector3 tipPosition;
        tipPosition.x = (float)(position[0] + matrix[0] * TipOffset.x + matrix[4] * TipOffset.y + matrix[8] * TipOffset.z);
        tipPosition.y = (float)(position[1] + matrix[1] * TipOffset.x + matrix[5] * TipOffset.y + matrix[9] * TipOffset.z);
        tipPosition.z = -(float)(position[2] + matrix[2] * TipOffset.x + matrix[6] * TipOffset.y + matrix[10] * TipOffset.z);

        return tipPosition;
    }

    //  ↓ you probably do not use, because it does not have a conversion to Unity coordinate system
    //	/// <summary>
    //	/// 現在のPHANTOMジンバル姿勢を返します
    //	/// <remarks>手先の姿勢ではありません。ジンバル部エンコーダの値です。</remarks>
    //	/// </summary>
    //	/// <returns>ジンバル部分の内、根元からペン部にかけて X～Z に対応した角度 [rad]</returns>
    //	public Vector3 GetGimbalAngles()
    //	{
    //		double[] gimbals = new double[3] { 0, 0, 0 };
    //		HdAPI.HdAPIGetDoublev(HdAPI.ParameterNameGet.HdAPI_CURRENT_GIMBAL_ANGLES, gimbals);
    //		return new Vector3((float)gimbals[0], (float)gimbals[1], (float)gimbals[2]);
    //	}

    double[] matrixRotation = new double[16];

    /// <summary>
    /// TODO - get rotation matrix
    /// </summary>
    /// <returns></returns>
    public double[] GetRotationMatrix()
    {
        double[] matrix = new double[16];
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_TRANSFORM, matrix);

        /*Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 4; i++)
        {
            result.SetRow(i, new Vector4((float)matrix[10 * i + 0], (float)matrix[10 * i + 1], (float)matrix[10 * i + 2], (float)matrix[10 * i + 3]));
        }*/

        return matrixRotation;
    }
    public void GetRotationMatrix(out double[] matrix)
    {
        matrix = new double[16];
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_TRANSFORM, matrix);
    }

    /// <summary>
    /// It returns the current PHANTOM hand posture
    /// </summary>
    /// <returns>Quaternion that represents the orientation</returns>
    public Quaternion GetRotation()
    {
        //double[] matrix = new double[16];
        matrixRotation = new double[16];
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_CURRENT_TRANSFORM, matrixRotation);
        //
        //		double qw = Math.Sqrt(1f + matrixRotation[0] + matrixRotation[5] + matrixRotation[10]) / 2;
        //		double w = 4 * qw;
        //		double qx = (matrixRotation[6] - matrixRotation[9]) / w;
        //		double qy = (matrixRotation[8] - matrixRotation[2]) / w;
        //		double qz = (matrixRotation[1] - matrixRotation[4]) / w;
        //		return new Quaternion((float)-qx, (float)-qy, (float)qz, (float)qw);

        double t = 1.0 + matrixRotation[0] + matrixRotation[5] + matrixRotation[10];
        double s;
        double qw, qx, qy, qz;
        if (t >= 1.0)
        {
            s = 0.5 / Math.Sqrt(t);
            qw = 0.25 / s;
            qx = (matrixRotation[6] - matrixRotation[9]) * s;
            qy = (matrixRotation[8] - matrixRotation[2]) * s;
            qz = (matrixRotation[1] - matrixRotation[4]) * s;
        }
        else
        {
            double max;
            if (matrixRotation[5] > matrixRotation[10])
            {
                max = matrixRotation[5];
            }
            else
            {
                max = matrixRotation[10];
            }

            if (max < matrixRotation[0])
            {
                t = Math.Sqrt(matrixRotation[0] - (matrixRotation[5] + matrixRotation[10]) + 1.0);
                s = 0.5 / t;
                qw = (matrixRotation[6] - matrixRotation[9]) * s;
                qx = t * 0.5;
                qy = (matrixRotation[1] + matrixRotation[4]) * s;
                qz = (matrixRotation[8] + matrixRotation[2]) * s;
            }
            else if (max == matrixRotation[5])
            {
                t = Math.Sqrt(matrixRotation[5] - (matrixRotation[10] + matrixRotation[0]) + 1.0);
                s = 0.5 / t;
                qw = (matrixRotation[8] - matrixRotation[2]) * s;
                qx = (matrixRotation[1] + matrixRotation[4]) * s;
                qy = t * 0.5;
                qz = (matrixRotation[6] + matrixRotation[9]) * s;
            }
            else
            {
                t = Math.Sqrt(matrixRotation[10] - (matrixRotation[0] + matrixRotation[5]) + 1.0);
                s = 0.5 / t;
                qw = (matrixRotation[1] - matrixRotation[4]) * s;
                qx = (matrixRotation[8] + matrixRotation[2]) * s;
                qy = (matrixRotation[6] + matrixRotation[9]) * s;
                qz = t * 0.5;
            }
        }
        return new Quaternion(-(float)qx, -(float)qy, (float)qz, (float)qw);
    }

    /// <summary>
    /// It gets the button that is currently pressed
    /// </summary>
    /// <returns>Button1 | Button2 | Button3 | Button4</returns>
    public Buttons GetButton()
    {
        int[] button = new int[1];
        HdAPI.hdGetIntegerv(HdAPI.ParameterNameGet.HD_CURRENT_BUTTONS, button);
        return (Buttons)button[0];
    }

    /// <summary>
    /// Update the button pressing situation of the PHANTOM
    /// </summary>
    /// <returns>The buttons.</returns>
    public Buttons UpdateButtons()
    {
        LastButtons = CurrentButtons;
        CurrentButtons = GetButton();
        return CurrentButtons;
    }

    /// <summary>
    /// It returns true if the place that has been designated by button is just pressed
    /// </summary>
    /// <returns><c>true</c>, if button was down, <c>false</c> otherwise.</returns>
    /// <param name="button">Button.</param>
    public bool GetButtonDown(Buttons button)
    {
        return (((LastButtons & button) == Buttons.None) && ((CurrentButtons & button) == button));
    }

    /// <summary>
    /// It returns true if the place that has been designated by button has been just released
    /// </summary>
    /// <returns><c>true</c>, if button was up, <c>false</c> otherwise.</returns>
    /// <param name="button">Button.</param>
    public bool GetButtonUp(Buttons button)
    {
        return (((LastButtons & button) == button) && ((CurrentButtons & button) == Buttons.None));
    }

    /// <summary>
    /// It gets the elapsed time from the servo loop start
    /// </summary>
    /// <returns>Time [s]</returns>
    public double GetSchedulerTimeStamp()
    {
        return HdAPI.hdGetSchedulerTimeStamp();
    }

    /// <summary>
    /// Gets the instantaneous update rate of the device [Hz]
    /// </summary>
    /// <returns>The Hz instantaneous rate</returns>
    public double GetInstantaneousUpdateRate()
    {
        double[] rate = new double[1] { 0 };
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_INSTANTANEOUS_UPDATE_RATE, rate);
        return rate[0];
    }

    /// <summary>
    /// Gets the max force limit of the device [N]
    /// </summary>
    /// <returns>The max force</returns>
    public double GetForceLimit()
    {
        double[] maxForce = new double[1] { 0 };

        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_NOMINAL_MAX_FORCE, maxForce);
        return maxForce[0];
    }

    /// <summary>
    /// Gets the max continuous force limit of the device [N]
    /// </summary>
    /// <returns>The max force</returns>
    public double GetContinuousForceLimit()
    {
        double[] maxForce = new double[1] { 0 };

        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_NOMINAL_MAX_CONTINUOUS_FORCE, maxForce);
        return maxForce[0];
    }

    /// <summary>
    /// verifies if the capability of force clamping is enabled
    /// </summary>
    /// <returns><code>true</code> if the max force campling is enabled, <code>false</code> otherwise</returns>
    public bool IsEnabledMaxForceClamping()
    {
        return HdAPI.hdIsEnabled(HdAPI.Capability.HD_MAX_FORCE_CLAMPING);
    }

    /// <summary>
    /// verifies if the capability of software force limit
    /// </summary>
    /// <returns><code>true</code> if the software force limit is enabled, <code>false</code> otherwise</returns>
    public bool IsEnabledSwForceLimit()
    {
        return HdAPI.hdIsEnabled(HdAPI.Capability.HD_SOFTWARE_FORCE_LIMIT);
    }

    #endregion

    #region Information Setting

    /// <summary>
    /// Set the PHANTOM display force
    /// </summary>
    /// <param name="force">力のベクトル [N] - Vector of the force [N]</param>
    public void SetForce(Vector3 force)
    {
        double[] forceArray = new double[3];
        forceArray[0] = force.x;
        forceArray[1] = force.y;
        forceArray[2] = -force.z;
        HdAPI.hdSetDoublev(HdAPI.ParameterNameSet.HD_CURRENT_FORCE, forceArray);
    }

    #endregion

    #region Internal

    /// <summary>
    /// Get the movable range
    /// </summary>
    private void LoadWorkspaceLimit()
    {
        double[] val = new double[6];

        // Get the movable limit range
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_MAX_WORKSPACE_DIMENSIONS, val);
        ErrorCheck("Getting max workspace");
        WorkspaceMinimum = new Vector3((float)val[0], (float)val[1], -(float)val[2]);
        WorkspaceMaximum = new Vector3((float)val[3], (float)val[4], -(float)val[5]);

        // Get the recommended range of movement
        HdAPI.hdGetDoublev(HdAPI.ParameterNameGet.HD_USABLE_WORKSPACE_DIMENSIONS, val);
        ErrorCheck("Getting usable workspace");
        UsableWorkspaceMinimum = new Vector3((float)val[0], (float)val[1], -(float)val[2]);
        UsableWorkspaceMaximum = new Vector3((float)val[3], (float)val[4], -(float)val[5]);

        // Gets the height of the desk
        float[] offset = new float[1];
        HdAPI.hdGetFloatv(HdAPI.ParameterNameGet.HD_TABLETOP_OFFSET, offset);
        ErrorCheck("Getting table-top offset");
        TableTopOffset = (float)offset[0];
    }

    /// <summary>
    /// If there is an error in the previous API call, an exception will be raised
    /// </summary>
    static private void ErrorCheck()
    {
        ErrorCheck("");
    }

    /// <summary>
    /// If there is an error in the previous API call, an exception will be raised
    /// </summary>
    /// <param name="situation">string to know if the situation happened or not</param>
    static private void ErrorCheck(string situation)
    {
        HdAPI.ErrorInfo error;

        if (HdAPI.IsError(error = HdAPI.hdGetError()))
        {
            string errorMessage = HdAPI.GetErrorString(error.ErrorCode);

            Debug.LogError("err : " + situation + " = " + error.ErrorCode + "|" + error.hHD + "|" + error.InternalErrorCode);
            if (situation.Equals(""))
            {
                throw new UnityException("HDAPI : [" + error.ErrorCode+ "] " + errorMessage);
            }
            else
            {
                throw new UnityException(situation + " / HDAPI : [" + error.ErrorCode + "] " + errorMessage);
            }
        }
    }

    internal void GetJointAngles(float[] jointAngles)
    {
        throw new NotImplementedException();
    }

    #endregion
}

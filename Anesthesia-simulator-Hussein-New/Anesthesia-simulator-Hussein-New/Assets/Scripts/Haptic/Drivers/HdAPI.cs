/**
 * ------------------------------------------------
 * ManagedPhantom
 * 
 * HD API Wrapper
 * 
 * Copyright (c) 2013-2014 Kirurobo
 * http://twitter.com/kirurobo
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 * 
 * 
 * REQUIREMENTS
 *  - PHANTOM haptic device
 *  - PHANTOM Device Drivers
 *  - hd.dll (Sensable OpenHaptics Toolkit)
 * 
 * ------------------------------------------------
 */

// HEADER

// INCLUDES
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Namespace for API HD
/// </summary>
namespace HD
{
    /// <summary>
    /// OpenHaptics 3.4 HDAPI wrapper class
    /// </summary>
    public class HdAPI
    {
        #region Defines

        /// <summary>
        /// Version information
        /// </summary>
        public enum Version : int
        {
            HD_VERSION_MAJOR_NUMBER = 3,
            HD_VERSION_MINOR_NUMBER = 30,
            HD_VERSION_BUILD_NUMBER = 0
        }

        /// <summary>
        /// Error code
        /// </summary>
        public enum ErrorCode : ushort
        {
            HD_SUCCESS = 0x0000,
            // Function errors
            HD_INVALID_ENUM = 0x0100,
            HD_INVALID_VALUE = 0x0101,
            HD_INVALID_OPERATION = 0x0102,
            HD_INVALID_INPUT_TYPE = 0x0103,
            HD_BAD_HANDLE = 0x0104,
            // Force errors
            HD_WARM_MOTORS = 0x0200,
            HD_EXCEEDED_MAX_FORCE = 0x0201,
            HD_EXCEEDED_MAX_FORCE_IMPULSE = 0x0202,
            HD_EXCEEDED_MAX_VELOCITY = 0x0203,
            HD_FORCE_ERROR = 0x0204,
            // Device errors
            HD_DEVICE_FAULT = 0x0300,
            HD_DEVICE_ALREADY_INITIATED = 0x0301,
            HD_COMM_ERROR = 0x0302,
            HD_COMM_CONFIG_ERROR = 0x0303,
            HD_TIMER_ERROR = 0x0304,
            // Haptic rendering context
            HD_ILLEGAL_BEGIN = 0x0400,
            HD_ILLEGAL_END = 0x0401,
            HD_FRAME_ERROR = 0x0402,
            // Scheduler errors
            HD_INVALID_PRIORITY = 0x0500,
            HD_SCHEDULER_FULL = 0x0501,
            // Licensing errors
            HD_INVALID_LICENSE = 0x0600
        }

        /// <summary>
        /// GET parameter options
        /// </summary>
        public enum ParameterNameGet : ushort
        {
            // Raw values
            HD_CURRENT_BUTTONS = 0x2000,
            HD_CURRENT_SAFETY_SWITCH = 0x2001,
            HD_CURRENT_INKWELL_SWITCH = 0x2002,
            HD_CURRENT_ENCODER_VALUES = 0x2010,
            HD_CURRENT_PINCH_VALUE = 0x2011,
            HD_LAST_PINCH_VALUE = 0x2012,
            // Cartesian space values
            HD_CURRENT_POSITION = 0x2050,
            HD_CURRENT_VELOCITY = 0x2051,
            HD_CURRENT_TRANSFORM = 0x2052,
            HD_CURRENT_ANGULAR_VELOCITY = 0x2053,
            HD_CURRENT_JACOBIAN = 0x2054,
            // joint space values
            HD_CURRENT_JOINT_ANGLES = 0x2100,
            HD_CURRENT_GIMBAL_ANGLES = 0x2150,
            HD_LAST_BUTTONS = 0x2200,
            HD_LAST_SAFETY_SWITCH = 0x2201,
            HD_LAST_INKWELL_SWITCH = 0x2202,
            HD_LAST_ENCODER_VALUES = 0x2210,
            HD_LAST_POSITION = 0x2250,
            HD_LAST_VELOCITY = 0x2251,
            HD_LAST_TRANSFORM = 0x2252,
            HD_LAST_ANGULAR_VELOCITY = 0x2253,
            HD_LAST_JACOBIAN = 0x2254,
            HD_LAST_JOINT_ANGLES = 0x2300,
            HD_LAST_GIMBAL_ANGLES = 0x2350,
            // Identification
            HD_VERSION = 0x2500,
            HD_DEVICE_MODEL_TYPE = 0x2501,
            HD_DEVICE_DRIVER_VERSION = 0x2502,
            HD_DEVICE_VENDOR = 0x2503,
            HD_DEVICE_SERIAL_NUMBER = 0x2504,
            HD_DEVICE_FIRMWARE_VERSION = 0x2505,
            // Device hardware properties
            HD_MAX_WORKSPACE_DIMENSIONS = 0x2550,
            HD_USABLE_WORKSPACE_DIMENSIONS = 0x2551,
            HD_TABLETOP_OFFSET = 0x2552,
            HD_INPUT_DOF = 0x2553,
            HD_OUTPUT_DOF = 0x2554,
            HD_CALIBRATION_STYLE = 0x2555,
            // Device forces and measurements
            HD_UPDATE_RATE = 0x2600,
            HD_INSTANTANEOUS_UPDATE_RATE = 0x2601,
            HD_NOMINAL_MAX_STIFFNESS = 0x2602,
            HD_NOMINAL_MAX_DAMPING = 0x2609,
            HD_NOMINAL_MAX_FORCE = 0x2603,
            HD_NOMINAL_MAX_CONTINUOUS_FORCE = 0x2604,
            HD_MOTOR_TEMPERATURE = 0x2605,
            HD_SOFTWARE_VELOCITY_LIMIT = 0x2606,
            HD_SOFTWARE_FORCE_IMPULSE_LIMIT = 0x2607,
            HD_FORCE_RAMPING_RATE = 0x2608,
            HD_NOMINAL_MAX_TORQUE_STIFFNESS = 0x2620,
            HD_NOMINAL_MAX_TORQUE_DAMPING = 0x2621,
            HD_NOMINAL_MAX_TORQUE_FORCE = 0x2622,
            HD_NOMINAL_MAX_TORQUE_CONTINUOUS_FORCE = 0x2623,
            // Cartesian space values
            HD_CURRENT_FORCE = 0x2700,
            HD_CURRENT_TORQUE = 0x2701,
            HD_JOINT_ANGLE_REFERENCES = 0x2702,
            // Motor space values
            HD_CURRENT_JOINT_TORQUE = 0x2703,
            HD_CURRENT_GIMBAL_TORQUE = 0x2704,
            // Motor space values
            HD_LAST_FORCE = 0x2800,
            HD_LAST_JOINT_TORQUE = 0x2802,
            HD_LAST_GIMBAL_TORQUE = 0x2803,
            // LED status light
            HD_USER_STATUS_LIGHT = 0x2900
        }

        /// <summary>
        /// SET parameter options
        /// </summary>
        public enum ParameterNameSet : ushort
        {
            HD_SOFTWARE_VELOCITY_LIMIT = 0x2606,
            HD_SOFTWARE_FORCE_IMPULSE_LIMIT = 0x2607,
            HD_FORCE_RAMPING_RATE = 0x2608,
            HD_CURRENT_FORCE = 0x2700,
            HD_CURRENT_TORQUE = 0x2701,
            HD_CURRENT_JOINT_TORQUE = 0x2703,
            HD_CURRENT_GIMBAL_TORQUE = 0x2704
        }

        /// <summary>
        /// ENABLE/DISABLE Capabilities
        /// </summary>
        public enum Capability : ushort
        {
            HD_FORCE_OUTPUT = 0x4000,
            HD_MAX_FORCE_CLAMPING = 0x4001,
            HD_FORCE_RAMPING = 0x4002,
            HD_SOFTWARE_FORCE_LIMIT = 0x4003,
            HD_ONE_FRAME_LIMIT = 0x4004
        }

        /// <summary>
        /// Scheduler priority ranges
        /// </summary>
        public class Priority
        {
            public const ushort HD_MAX_SCHEDULER_PRIORITY = 0xffff;
            public const ushort HD_MIN_SCHEDULER_PRIORITY = 0;
            public const ushort HD_DEFAULT_SCHEDULER_PRIORITY = (HD_MAX_SCHEDULER_PRIORITY + HD_MIN_SCHEDULER_PRIORITY) / 2;
            public const ushort HD_RENDER_EFFECT_FORCE_PRIORITY = HD_MAX_SCHEDULER_PRIORITY - 1;
        }

        /// <summary>
        /// Calibration return values
        /// </summary>
        public enum CalibrationResult : ushort
        {
            HD_CALIBRATION_OK = 0x5000,
            HD_CALIBRATION_NEEDS_UPDATE = 0x5001,
            HD_CALIBRATION_NEEDS_MANUAL_INPUT = 0x5002
        }

        /// <summary>
        /// Calibration styles
        /// </summary>
        [Flags]
        public enum CalibrationStyle : ushort
        {
            HD_CALIBRATION_ENCODER_RESET = 1,
            HD_CALIBRATION_AUTO = 2,
            HD_CALIBRATION_INKWELL = 4
        }

        /// <summary>
        /// Button masks
        /// </summary>
        [Flags]
        public enum Button : ushort
        {
            HD_DEVICE_BUTTON_1 = 1,
            HD_DEVICE_BUTTON_2 = 2,
            HD_DEVICE_BUTTON_3 = 4,
            HD_DEVICE_BUTTON_4 = 8
        }

        /// <summary>
        /// Device Handle number
        /// </summary>
        public enum DeviceHandle : uint
        {
            HD_INVALID_HANDLE = 0xFFFFFFFF,
            HD_DEFAULT_DEVICE = 0x0
        }

        /// <summary>
        /// LED status
        /// </summary>
        public enum LedStatus : ushort
        {
            LED_MASK = 0x07,
            LED_STATUS_FAST_GRNYEL = 0x00,
            LED_STATUS_SLOW_YEL = 0x01,
            LED_STATUS_SLOW_GRN = 0x02,
            LED_STATUS_FAST_GRN = 0x03,
            LED_STATUS_SOLID_GRNYEL = 0x04,
            LED_STATUS_SOLID_YEL = 0x05,
            LED_STATUS_SOLID_GRN = 0x06,
            LED_STATUS_FAST_YEL = 0x07
        }

        /// <summary>
        /// Callback return code
        /// </summary>
        public enum CallbackResult : uint
        {
            HD_CALLBACK_DONE = 0,
            HD_CALLBACK_CONTINUE = 1
        }

        /// <summary>
        /// WaitForCompletion codes
        /// </summary>
        public enum WaitCode : uint
        {
            HD_WAIT_CHECK_STATUS = 0,   // returns whether the scheduler operation for the given handle is still scheduled
            HD_WAIT_INFINITE = 1        // waits for the scheduler operation to complete
        }

        /// <summary>
        /// Error information
        /// </summary>
        public struct ErrorInfo
        {
            public ErrorCode ErrorCode;
            public int InternalErrorCode;
            public uint hHD;
        }

        #endregion

        #region hdDevice

        /// <summary>
        /// Initialize the device
        /// Must be called before any use of the device
        /// Makes the device current
        /// </summary>
        [DllImport("hd.dll")]
        public static extern uint hdInitDevice(string deviceName);

        /// <summary>
        /// Initialize the device 'Default Device'
        /// Must be called before any use of the device
        /// Makes the device current
        /// </summary>
        [DllImport("hd.dll")]
        public static extern uint hdInitDevice(DeviceHandle device);

        /// <summary>
        /// Makes the specified device current: all future calls will be performed on this device
        /// Requires a valid device id from an initialized device
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdMakeCurrentDevice(uint hHD);

        /// <summary>
        /// Disables the specified device
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdDisableDevice(uint hHD);

        /// <summary>
        /// Returns the ID of the current device
        /// </summary>
        [DllImport("hd.dll")]
        public static extern uint hdGetCurrentDevice();

        /// <summary>
        /// Begin grabs state information from the device
        /// All hdSet and state calls should be done within a begin/end frame
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdBeginFrame(uint hHD);

        /// <summary>
        /// End sends information such as forces to the device
        /// All hdSet and state calls should be done within a begin/end frame
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdEndFrame(uint hHD);

        /// <summary>
        /// Returns error information in the reverse order (i.e. most recent error first)
        /// Returns an error with code HD_SUCCESS if no error on stack
        /// </summary>
        [DllImport("hd.dll")]
        public static extern ErrorInfo hdGetError();

        /// <summary>
        /// Shorthand for querying whether hdGetError reported an error
        /// </summary>
        /// <param name="errorInfo">error information result from hdGetError</param>
        /// <returns>true if errorInfo is an error, false otherwise</returns>
        public static bool IsError(ErrorInfo errorInfo)
        {
            return (errorInfo.ErrorCode != ErrorCode.HD_SUCCESS);
        }

        /// <summary>
        /// Returns a readable description of the error code
        /// </summary>
        [DllImport("hd.dll")]
        public static extern IntPtr hdGetErrorString(ErrorCode errorCode);

        /// <summary>
        /// Returns the error message
        /// </summary>
        /// <returns>Message in a readable format</returns>
        public static string GetErrorString(ErrorCode errorCode)
        {
            return Marshal.PtrToStringAnsi(hdGetErrorString(errorCode));
        }

        /// <summary>
        /// Routine for enabling a capability
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdEnable(Capability cap);

        /// <summary>
        /// Routine for disabling a capability
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdDisable(Capability cap);

        /// <summary>
        /// Routine for checking whether a capability is enabled
        /// </summary>
        [DllImport("hd.dll")]
        public static extern bool hdIsEnabled(Capability cap);

        /// <summary>
        /// Reads the option into parameter array
        /// User is responsible for allocating parameter of correct size for the call
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdGetBooleanv(ParameterNameGet paramName, [Out] bool[] value);

        /// <summary>
        /// Reads the option into parameter array
        /// User is responsible for allocating parameter of correct size for the call
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdGetIntegerv(ParameterNameGet paramName, [Out] int[] value);

        /// <summary>
        /// Reads the option into parameter array
        /// User is responsible for allocating parameter of correct size for the call
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdGetFloatv(ParameterNameGet paramName, [Out]  float[] value);

        /// <summary>
        /// Reads the option into parameter array
        /// User is responsible for allocating parameter of correct size for the call
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdGetDoublev(ParameterNameGet paramName, [Out] double[] value);

        /// <summary>
        /// Reads the option into parameter array
        /// User is responsible for allocating parameter of correct size for the call
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdGetLongv(ParameterNameGet paramName, [Out] int[] value);

        /// <summary>
        /// Reads the option
        /// </summary>
        [DllImport("hd.dll")]
        public static extern string hdGetString(ParameterNameGet paramName);

        /// <summary>
        /// Writes the option with the params array
        /// User is responsible for supplying the correct number of parameter memory space
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdSetBooleanv(ParameterNameSet paramName, [In] bool[] value);

        /// <summary>
        /// Writes the option with the params array
        /// User is responsible for supplying the correct number of parameter memory space
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdSetIntegerv(ParameterNameSet paramName, [In] int[] value);

        /// <summary>
        /// Writes the option with the params array
        /// User is responsible for supplying the correct number of parameter memory space
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdSetFloatv(ParameterNameSet paramName, [In]  float[] value);

        /// <summary>
        /// Writes the option with the params array
        /// User is responsible for supplying the correct number of parameter memory space
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdSetDoublev(ParameterNameSet paramName, [In] double[] value);

        /// <summary>
        /// Writes the option with the params array
        /// User is responsible for supplying the correct number of parameter memory space
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdSetLongv(ParameterNameSet paramName, [In] int[] value);

        /// <summary>
        /// Calibration routine
        /// </summary>
        [DllImport("hd.dll")]
        public static extern CalibrationResult hdCheckCalibration();

        /// <summary>
        /// Calibration routine
        /// </summary>
        [DllImport("hd.dll")]
        public static extern CalibrationResult hdCheckCalibrationStyle();

        /// <summary>
        /// Calibration routine
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdUpdateCalibrationMessage(CalibrationStyle style);

        /// <summary>
        /// Calibration routine
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdUpdateCalibration(CalibrationStyle style);

        /// <summary>
        /// Gimbal Scaling Routine
        /// nT is of size 16
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdScaleGimbalAngles(double scaleX, double scaleY, double scaleZ, double[] nT);

        /// <summary>
        /// Licensing routines
        /// </summary>
        [DllImport("hd.dll")]
        public static extern bool hdDeploymentLicense(string vendorName, string applicationName, string password);

        #endregion

        #region hdScheduler

        /// <summary>
        /// Callback to scheduler operations
        /// </summary>
        [return: MarshalAs(UnmanagedType.U4)]
        public delegate CallbackResult SchedulerCallback(IntPtr data);

        /// <summary>
        /// Scheduler function start
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdStartScheduler();

        /// <summary>
        /// Scheduler function stop
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdStopScheduler();

        /// <summary>
        /// Scheduler function set rate of the scheduler
        /// It specifies how many times the callback is called per second
        /// </summary>
        /// <param name="rate">Hz</param>
        [DllImport("hd.dll")]
        public static extern void hdSetSchedulerRate([MarshalAs(UnmanagedType.U8)] ulong rate);

        /// <summary>
        /// Schedule a function to run in the scheduler thread and wait for completion
        /// </summary>
        /// <param name="callback">is the function to be run</param>
        /// <param name="userData">is user data to be passed into the function to be run</param>
        /// <param name="priority">is the scheduling priority, which determines what order the callbacks are run when multiple callbacks are scheduled (higher priority means run first)</param>
        [DllImport("hd.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hdScheduleSynchronous([MarshalAs(UnmanagedType.FunctionPtr)] SchedulerCallback callback, IntPtr userData, ushort priority);

        /// <summary>
        /// Schedule a function to run in the scheduler, do not wait for its completion
        /// Returned handle is used to unschedule or block waiting for completion
        /// </summary>
        [DllImport("hd.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong hdScheduleAsynchronous([MarshalAs(UnmanagedType.FunctionPtr)] SchedulerCallback callback, IntPtr userData, ushort priority);

        /// <summary>
        /// Unschedule the operation identified by the handle
        /// </summary>
        [DllImport("hd.dll")]
        public static extern void hdUnschedule([MarshalAs(UnmanagedType.U8)] ulong schedulerHandle);

        /// <summary>
        /// Wait until the operation associated with the handle is completed
        /// If HD_WAIT_CHECK_STATUS, returns true if the operation associated with the handle is still scheduled
        /// </summary>
        [DllImport("hd.dll")]
        public static extern bool hdWaitForCompletion([MarshalAs(UnmanagedType.U8)] ulong schedulerHandle, WaitCode waitCode);

        /// <summary>
        /// Get the time in seconds since the start of the frame
        /// </summary>
        [DllImport("hd.dll")]
        public static extern double hdGetSchedulerTimeStamp();

        #endregion
    }

    #region Extra

    /// <summary>
    /// Button
    /// </summary>
    [Flags]
    public enum Buttons : ushort
    {
        /// <summary>
        /// State where the button is not pressed
        /// </summary>
        None = 0,

        /// <summary>
        /// Button 1 is pressed
        /// </summary>
        Button1 = HdAPI.Button.HD_DEVICE_BUTTON_1,

        /// <summary>
        /// Button 2 is pressed
        /// </summary>
        Button2 = HdAPI.Button.HD_DEVICE_BUTTON_2,

        /// <summary>
        /// Button 3 is pressed
        /// </summary>
        Button3 = HdAPI.Button.HD_DEVICE_BUTTON_3,

        /// <summary>
        /// Button 4 is pressed
        /// </summary>
        Button4 = HdAPI.Button.HD_DEVICE_BUTTON_4,
    }

    /// <summary>
    /// Exceptions that occur at the time of error HDAPI in OpenHaptics
    /// </summary>
    public class HdApiException : Exception
    {
        /// <summary>
        /// It generates an exception with no message
        /// </summary>
        public HdApiException() : base() { }

        /// <summary>
        /// Specify the message and generates an exception
        /// </summary>
        /// <param name="message"></param>
        public HdApiException(string message) : base(message) { }
    }
    #endregion
}

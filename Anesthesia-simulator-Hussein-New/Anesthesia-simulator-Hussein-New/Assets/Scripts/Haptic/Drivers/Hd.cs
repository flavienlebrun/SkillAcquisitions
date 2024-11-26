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

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ManagedPhantom
{
    /// <summary>
    /// OpenHaptics 3.0 HDAPI wrapper class
    /// </summary>
    public static class Hd
    {
        #region Definitions
        /// <summary>
        /// エラー情報
        /// Error information
        /// </summary>
        public struct ErrorInfo
        {
            public ErrorCode ErrorCode;
            public int InternalErrorCode;
            public int hHD;
        }

        /// <summary>
        /// 実行結果
        /// Callback result
        /// </summary>
        public enum CallbackResult : uint
        {
            HD_CALLBACK_DONE = 0,
            HD_CALLBACK_CONTINUE = 1
        }

        /// <summary>
        /// パラメータ名
        /// Parameter name
        /// </summary>
        public enum ParameterName : ushort
        {
            HD_CURRENT_BUTTONS = 0x2000,
            HD_CURRENT_SAFETY_SWITCH = 0x2001,
            HD_CURRENT_INKWELL_SWITCH = 0x2002,
            HD_CURRENT_ENCODER_VALUES = 0x2010,
            HD_CURRENT_PINCH_VALUE = 0x2011,
            HD_LAST_PINCH_VALUE = 0x2012,
            HD_CURRENT_POSITION = 0x2050,
            HD_CURRENT_VELOCITY = 0x2051,
            HD_CURRENT_TRANSFORM = 0x2052,
            HD_CURRENT_ANGULAR_VELOCITY = 0x2053,
            HD_CURRENT_JACOBIAN = 0x2054,
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
            HD_VERSION = 0x2500,
            HD_DEVICE_MODEL_TYPE = 0x2501,
            HD_DEVICE_DRIVER_VERSION = 0x2502,
            HD_DEVICE_VENDOR = 0x2503,
            HD_DEVICE_SERIAL_NUMBER = 0x2504,
            HD_DEVICE_FIRMWARE_VERSION = 0x2505,
            HD_MAX_WORKSPACE_DIMENSIONS = 0x2550,
            HD_USABLE_WORKSPACE_DIMENSIONS = 0x2551,
            HD_TABLETOP_OFFSET = 0x2552,
            HD_INPUT_DOF = 0x2553,
            HD_OUTPUT_DOF = 0x2554,
            HD_CALIBRATION_STYLE = 0x2555,
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
            HD_CURRENT_FORCE = 0x2700,
            HD_JOINT_ANGLE_REFERENCES = 0x2702,
            HD_CURRENT_JOINT_TORQUE = 0x2703,
            HD_CURRENT_GIMBAL_TORQUE = 0x2704,
            HD_LAST_FORCE = 0x2800,
            HD_LAST_JOINT_TORQUE = 0x2802,
            HD_LAST_GIMBAL_TORQUE = 0x2803,
            HD_USER_STATUS_LIGHT = 0x2900,
        }

        /// <summary>
        /// エラーコード
        /// Error code
        /// </summary>
        public enum ErrorCode : ushort
        {
            HD_SUCCESS = 0x0000,
            HD_INVALID_ENUM = 0x0100,
            HD_INVALID_VALUE = 0x0101,
            HD_INVALID_OPERATION = 0x0102,
            HD_INVALID_INPUT_TYPE = 0x0103,
            HD_BAD_HANDLE = 0x0104,
            HD_WARM_MOTORS = 0x0200,
            HD_EXCEEDED_MAX_FORCE = 0x0201,
            HD_EXCEEDED_MAX_FORCE_IMPULSE = 0x0202,
            HD_EXCEEDED_MAX_VELOCITY = 0x0203,
            HD_FORCE_ERROR = 0x0204,
            HD_DEVICE_FAULT = 0x0300,
            HD_DEVICE_ALREADY_INITIATED = 0x0301,
            HD_COMM_ERROR = 0x0302,
            HD_COMM_CONFIG_ERROR = 0x0303,
            HD_TIMER_ERROR = 0x0304,
            HD_ILLEGAL_BEGIN = 0x0400,
            HD_ILLEGAL_END = 0x0401,
            HD_FRAME_ERROR = 0x0402,
            HD_INVALID_PRIORITY = 0x0500,
            HD_SCHEDULER_FULL = 0x0501,
            HD_INVALID_LICENSE = 0x0600,
        }

        /// <summary>
        /// 機能名
        /// Capability
        /// </summary>
        public enum Capability : ushort
        {
            HD_FORCE_OUTPUT = 0x4000,
            HD_MAX_FORCE_CLAMPING = 0x4001,
            HD_FORCE_RAMPING = 0x4002,
            HD_SOFTWARE_FORCE_LIMIT = 0x4003,
            HD_ONE_FRAME_LIMIT = 0x4004,
        }

        /// <summary>
        /// 優先度
        /// Priority
        /// </summary>
        public class Priority
        {
            public const ushort HD_MAX_SCHEDULER_PRIORITY = 0xffff;
            public const ushort HD_MIN_SCHEDULER_PRIORITY = 0;
            public const ushort HD_DEFAULT_SCHEDULER_PRIORITY = (HD_MAX_SCHEDULER_PRIORITY + HD_MIN_SCHEDULER_PRIORITY) / 2;
            public const ushort HD_RENDER_EFFECT_FORCE_PRIORITY = HD_MAX_SCHEDULER_PRIORITY - 1;
        }

        /// <summary>
        /// 処理待ちタイプ
        /// Waiting code
        /// </summary>
        public enum WaiteCode : uint
        {
            HD_WAIT_CHECK_STATUS = 0,
            HD_WAIT_INFINITE = 1
        }

        /// <summary>
        /// キャリブレーション結果
        /// Calibration result
        /// </summary>
        public enum CalibrationResult : ushort
        {
            HD_CALIBRATION_OK = 0x5000,
            HD_CALIBRATION_NEEDS_UPDATE = 0x5001,
            HD_CALIBRATION_NEEDS_MANUAL_INPUT = 0x5002,
        }

        /// <summary>
        /// キャリブレーション種類
        /// Calibration type
        /// </summary>
        [Flags]
        public enum CalibrationStyle : ushort
        {
            HD_CALIBRATION_ENCODER_RESET = 1,
            HD_CALIBRATION_AUTO = 2,
            HD_CALIBRATION_INKWELL = 4,
        }

        /// <summary>
        /// 押されたボタン
        /// Pressed button
        /// </summary>
        [Flags]
        public enum Button : ushort
        {
            /* Button Masks */
            HD_DEVICE_BUTTON_1 = 1,
            HD_DEVICE_BUTTON_2 = 2,
            HD_DEVICE_BUTTON_3 = 4,
            HD_DEVICE_BUTTON_4 = 8,
        }

        /// <summary>
        /// デバイスハンドル番号
        /// Device Handle number
        /// </summary>
        public enum DeviceHandle : uint
        {
            HD_INVALID_HANDLE = 0xFFFFFFFF,
            HD_DEFAULT_DEVICE = 0x0,
        }

        /// <summary>
        /// LEDの状態
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
            LED_STATUS_FAST_YEL = 0x07,
        }

        #endregion

        [DllImport("hd.dll")]
        public static extern uint hdInitDevice(string deviceName);

        [DllImport("hd.dll")]
        public static extern uint hdInitDevice(DeviceHandle device);

        [DllImport("hd.dll")]
        public static extern uint hdGetCurrentDevice();

        [DllImport("hd.dll")]
        public static extern void hdMakeCurrentDevice(uint hHD);

        [DllImport("hd.dll")]
        public static extern void hdBeginFrame(uint hHD);

        [DllImport("hd.dll")]
        public static extern void hdEndFrame(uint hHD);

        [DllImport("hd.dll")]
        public static extern void hdEnable(Capability cap);

        [DllImport("hd.dll")]
        public static extern void hdDisable(Capability cap);

        [DllImport("hd.dll")]
        public static extern void hdDisableDevice(uint hHD);

        [DllImport("hd.dll")]
        public static extern bool hdIsEnabled(Capability cap);

        [DllImport("hd.dll")]
        public static extern ErrorInfo hdGetError();

        [DllImport("hd.dll")]
        public static extern IntPtr hdGetErrorString(ErrorCode errorCode);

        [DllImport("hd.dll")]
        public static extern void hdGetBooleanv(ParameterName paramName, [Out] bool[] value);

        [DllImport("hd.dll")]
        public static extern void hdGetIntegerv(ParameterName paramName, [Out] int[] value);

        [DllImport("hd.dll")]
        public static extern void hdGetFloatv(ParameterName paramName, [Out]  float[] value);

        [DllImport("hd.dll")]
        public static extern void hdGetDoublev(ParameterName paramName, [Out] double[] value);

        [DllImport("hd.dll")]
        public static extern void hdGetLongv(ParameterName paramName, [Out] int[] value);

        [DllImport("hd.dll")]
        public static extern string hdGetString(ParameterName paramName);

        [DllImport("hd.dll")]
        public static extern void hdSetBooleanv(ParameterName paramName, [In] bool[] value);

        [DllImport("hd.dll")]
        public static extern void hdSetIntegerv(ParameterName paramName, [In] int[] value);

        [DllImport("hd.dll")]
        public static extern void hdSetFloatv(ParameterName paramName, [In]  float[] value);

        [DllImport("hd.dll")]
        public static extern void hdSetDoublev(ParameterName paramName, [In] double[] value);

        [DllImport("hd.dll")]
        public static extern void hdSetLongv(ParameterName paramName, [In] int[] value);

        [DllImport("hd.dll")]
        public static extern CalibrationResult hdCheckCalibration();

        [DllImport("hd.dll")]
        public static extern void hdUpdateCalibration(CalibrationStyle style);

        [DllImport("hd.dll")]
        public static extern void hdStartScheduler();

        [DllImport("hd.dll")]
        public static extern bool hdWaitForCompletion([MarshalAs(UnmanagedType.U4)] uint schedulerHandle, WaiteCode waitCode);

        [DllImport("hd.dll")]
        public static extern void hdStopScheduler();

        [DllImport("hd.dll")]
        public static extern double hdGetSchedulerTimeStamp();

        [DllImport("hd.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong hdScheduleAsynchronous([MarshalAs(UnmanagedType.FunctionPtr)] SchedulerCallback callback, IntPtr userData, ushort priority);

        [DllImport("hd.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hdScheduleSynchronous([MarshalAs(UnmanagedType.FunctionPtr)] SchedulerCallback callback, IntPtr userData, ushort priority);

        [DllImport("hd.dll")]
        public static extern void hdUnschedule([MarshalAs(UnmanagedType.U8)] ulong schedulerHandle);

        /// <summary>
        /// コールバックが1秒間に何回呼ばれるかを指定
        /// It specifies how many times the callback is called per second
        /// </summary>
        /// <param name="rate">Hz</param>
        [DllImport("hd.dll")]
        public static extern void hdSetSchedulerRate([MarshalAs(UnmanagedType.U4)] uint rate);

        /// <summary>
        /// コールバックのメソッドを設定
        /// Set up a call-back method
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.U4)]
        public delegate CallbackResult SchedulerCallback(IntPtr data);

        /// <summary>
        /// 実行結果がエラーかどうか判定
        /// Determines whether there is an error as result of execution
        /// </summary>
        /// <param name="errorInfo"></param>
        /// <returns></returns>
        public static bool IsError(ErrorInfo errorInfo)
        {
            return (errorInfo.ErrorCode != ErrorCode.HD_SUCCESS);
        }

        /// <summary>
        /// エラーメッセージを返す
        /// Returns the error message
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns>読める形式のメッセージ - Message in a readable format</returns>
        public static string GetErrorString(ErrorCode errorCode)
        {
            return Marshal.PtrToStringAnsi(hdGetErrorString(errorCode));
        }
    }

    static public class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    

    static public class Program_To_Close
    {
        static public string TrickToClose()
        {
            string answer = "Trick to close [";

            //IntPtr pDll = NativeMethods.LoadLibrary(@"‪C:\Windows\System32\hd.dll");
            //IntPtr pDll = NativeMethods.LoadLibrary(@"‪C:/Windows/System32/hd.dll");
            IntPtr pDll = NativeMethods.LoadLibrary(@"‪‪C:\WINDOWS\SYSTEM32\hd.dll");

            if (pDll == IntPtr.Zero)
            {
                Console.WriteLine("Didn't work... :'(");
                answer += "DID NOT WORK]\n";
            }
            else
            {
                Console.WriteLine("Pointer to loaded library = " + pDll);
                answer += pDll + "]\n";
            }

            bool result = NativeMethods.FreeLibrary(pDll);

            
            answer += "The result of the freeing library = " + result;
            return answer;
        }

        public static string UnloadModule(string moduleName)
        {
            string answer = "";

            foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
            {
                if (mod.ModuleName == moduleName)
                {
                    answer += mod.BaseAddress;
                    NativeMethods.FreeLibrary(mod.BaseAddress);
                }
            }

            return answer;
        }

        static public string ListProcessModules()
        {
            string answer = "";
            foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
            {
                answer += mod.ModuleName;
            }
            return answer;
        }
    }


    #region 追加クラス・構造体
    // Adding class structure
    /// <summary>
    /// ボタン名
    /// Button
    /// </summary>
    [Flags]
    public enum Buttons : ushort
    {
        /// <summary>
        /// ボタンが押されていない状態
        /// State where the button is not pressed
        /// </summary>
        None = 0,

        /// <summary>
        /// ボタン1が押されている
        /// Button 1 is pressed
        /// </summary>
        Button1 = Hd.Button.HD_DEVICE_BUTTON_1,

        /// <summary>
        /// ボタン2が押されている
        /// Button 2 is pressed
        /// </summary>
        Button2 = Hd.Button.HD_DEVICE_BUTTON_2,

        /// <summary>
        /// ボタン3が押されている
        /// Button 3 is pressed
        /// </summary>
        Button3 = Hd.Button.HD_DEVICE_BUTTON_3,

        /// <summary>
        /// ボタン4が押されている
        /// Button 4 is pressed
        /// </summary>
        Button4 = Hd.Button.HD_DEVICE_BUTTON_4,
    }

    /// <summary>
    /// OpenHaptics の HDAPI エラー時に発生する例外
    /// Exceptions that occur at the time of error HDAPI in OpenHaptics
    /// </summary>
    public class HdApiException : Exception
    {
        /// <summary>
        /// メッセージ無しで例外を生成します
        /// It generates an exception with no message
        /// </summary>
        public HdApiException() : base() { }

        /// <summary>
        /// メッセージを指定して例外を生成します
        /// Specify the message and generates an exception
        /// </summary>
        /// <param name="message"></param>
        public HdApiException(string message) : base(message) { }
    }
    #endregion
}

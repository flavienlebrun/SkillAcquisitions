using System;
using UnityEngine;
using ROS2;
using System.Threading;


public class Admittance_link : MonoBehaviour
{
    [Header("Target Objects")]
    public Rigidbody physicalSphere; // The physical sphere with mass
    public Transform robotBase; // object for displaying the robot base
    public Transform robotee; // to show the robot referece


    [Header("Optional Fixed Offset (initial position of the admittance sphere )")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("robot rotation in global unity refernce (in order, left hand) can't change after start")]
    public float rotx = 0.0f;
    public float roty = 0.0f;
    public float rotz = 0.0f;

    [Header("Flips to pass from right Hand to Left Hand (on current local axis,always flip teh same direction axis in both config)")]
    public bool flip_x = false;
    public bool flip_y = false;
    public bool flip_z = false;

    [Header("Topic Names")]
    public string poseTopic = "/robot/state/pose";
    public string twistTopic = "/robot/state/twist";
    public string wrenchTopic = "/robot/state/wrench";
    public string pose_commandTopic = "/robot/command/pose";
    public string twist_commandTopic = "/robot/command/twist";
    public bool interpolation_bool = false;
    public float decay_factor = 0.9f;

    [Header("Work sapce Limits")]

    public Vector3 max = new Vector3(0.36f, 0.23f, 0.191f);
    public Vector3 min = new Vector3(0.13f, -0.21f, -0.191f);

    [Header("Speed Limits")]

    public Vector3 maxLinear = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 minLinear = new Vector3(-0.2f, -0.2f, -0.2f);

    public Vector3 maxAngular = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 minAngular = new Vector3(-0.5f, -0.5f, -0.5f);



    [Header("Debug")]
    [SerializeField] private Vector3 rbt_force;
    [SerializeField] private Vector3 rbt_torque;
    [SerializeField] private Vector3 glb_rbt_force;
    [SerializeField] private Vector3 glb_rbt_torque;



    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private ISubscription<geometry_msgs.msg.WrenchStamped> wrench_sub;
    

    private IPublisher<geometry_msgs.msg.PoseStamped> pose_pub;
    private IPublisher<geometry_msgs.msg.TwistStamped> twist_pub;
    private Thread publishingThread;
    private bool isRunning = false;
    private readonly object dataLock = new object();
    private Vector3 thread_safe_pose;
    private Quaternion thread_safe_quaternion;
    private Vector3 thread_safe_lin_vel;
    private Vector3 thread_safe_ang_vel;
    private geometry_msgs.msg.PoseStamped poseMsg;
    private geometry_msgs.msg.TwistStamped twistMsg;

    private Matrix4x4 R;
    private Matrix4x4 R_inverse;
    private Vector3 GlobalTorque;
    private Vector3 GlobalForce;

    private GameObject box;
    private Material boxMaterial;
    private Vector3 maxUnity;
    private Vector3 minUnity;



    /////////these are for subscribing to the position and the speed  of 
    /// the robot , some are temp , some are not 
    /// 
    /// 
    private ISubscription<geometry_msgs.msg.PoseStamped> pose_sub;
    private Vector3 unityPosition;
    private Quaternion unityRotation;
    private Vector3 global_pos;
    private Quaternion global_orient;

    private ISubscription<geometry_msgs.msg.TwistStamped> twist_sub;
    private Vector3 unitylinTwist;
    private Vector3 unityangTwist;




    /// for interpolation correcting frequency issues  issues 
    /// </summary>
    private Vector3 lastPublishedPose = Vector3.zero;
    private Vector3 lastPublishedTwist = Vector3.zero;
    // private Quaternion lastPublishedRotation = Quaternion.identity;
    private int repeatedPublishCount = 0;
    private Vector3 lastEstimatedPose;

    // private DateTime lastPublishTime = DateTime.MinValue;
    // private double minPublishIntervalMs = 10.0; // Minimum 100Hz (publish every 10ms)
    //////////





    void Start()
    {
        if (physicalSphere == null)
        {
            Debug.LogError("Target Object is not assigned!");
            return;
        }
        ros2Unity = GetComponent<ROS2UnityComponent>();


        if (ros2Node == null && ros2Unity.Ok())
        {
            ros2Node = ros2Unity.CreateNode("UnitySimNode");
            wrench_sub = ros2Node.CreateSubscription<geometry_msgs.msg.WrenchStamped>(wrenchTopic, WrenchCallback);
            pose_pub = ros2Node.CreatePublisher<geometry_msgs.msg.PoseStamped>(pose_commandTopic);
            twist_pub = ros2Node.CreatePublisher<geometry_msgs.msg.TwistStamped>(twist_commandTopic);

            pose_sub = ros2Node.CreateSubscription<geometry_msgs.msg.PoseStamped>(poseTopic, PoseCallback);
            twist_sub = ros2Node.CreateSubscription<geometry_msgs.msg.TwistStamped>(twistTopic, TwistCallback);


            Debug.Log($"Subscribed to ROS2 topics: {wrenchTopic}");
            Debug.Log($"Initiliased a publisher for position: {pose_commandTopic}");
            Debug.Log($"Initiliased a publisher for position: {twist_commandTopic}");
        }

        poseMsg = new geometry_msgs.msg.PoseStamped();
        poseMsg.Pose.Position = new geometry_msgs.msg.Point();
        poseMsg.Pose.Orientation = new geometry_msgs.msg.Quaternion();
        twistMsg = new geometry_msgs.msg.TwistStamped();
        twistMsg.Twist.Linear = new geometry_msgs.msg.Vector3();
        twistMsg.Twist.Angular = new geometry_msgs.msg.Vector3();

        isRunning = true;
        publishingThread = new Thread(PublishingLoop);
        publishingThread.Priority = System.Threading.ThreadPriority.BelowNormal;
        publishingThread.Start();
        //physicalSphere.position = positionOffset;
        // physicalSphere.rotation = new Quaternion(0.5235f, 0.6423f, -0.3524f, 0.434812f);


        ///////////For visaulising the works psace of the robot 
        // Create box
        box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(box.GetComponent<Collider>()); // remove collider
        // Create semi-transparent material
        boxMaterial = new Material(Shader.Find("Standard"));
        boxMaterial.color = new Color(0f, 1f, 0f, 0.3f); // green, 30% opacity
        boxMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        boxMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        boxMaterial.SetInt("_ZWrite", 0);
        boxMaterial.DisableKeyword("_ALPHATEST_ON");
        boxMaterial.EnableKeyword("_ALPHABLEND_ON");
        boxMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        boxMaterial.renderQueue = 3000;
        box.GetComponent<MeshRenderer>().material = boxMaterial;
        

        Quaternion rotation = Quaternion.Euler(rotx, roty, rotz);
        Vector3 scale = new Vector3(1f, 1f, 1f);
        R = Matrix4x4.TRS(positionOffset, rotation, scale);
        R_inverse = R.inverse;




    }

    void Update()
    {
        Quaternion rotation = Quaternion.Euler(rotx, roty, rotz);
        Vector3 scale = new Vector3(1f, 1f, 1f);
        R = Matrix4x4.TRS(positionOffset, rotation, scale);
        R_inverse = R.inverse;

        ////Pour visualiser la base du robot , ou juste le robot 
        Vector3 robotBasePos = R.GetColumn(3);
        Quaternion robotBaseRot = R.rotation;
        if (flip_x) robotBasePos.x = -robotBasePos.x;
        if (flip_y) robotBasePos.y = -robotBasePos.y;
        if (flip_z) robotBasePos.z = -robotBasePos.z;
        robotBase.transform.position = robotBasePos;
        robotBase.transform.rotation = robotBaseRot;

        //For visaulising the workspace limit
        //////////////////////////////////////////
        maxUnity = R.MultiplyPoint3x4(max);
        minUnity = R.MultiplyPoint3x4(min);
        if (flip_x) { maxUnity.x = -maxUnity.x; minUnity.x = -minUnity.x; }
        if (flip_y) { maxUnity.y = -maxUnity.y; minUnity.y = -minUnity.y; }
        if (flip_z) { maxUnity.z = -maxUnity.z; minUnity.z = -minUnity.z; }
        Vector3 centerUnity = (maxUnity + minUnity) * 0.5f;
        Vector3 sizeUnity = new Vector3(
            Mathf.Abs(maxUnity.x - minUnity.x),
            Mathf.Abs(maxUnity.y - minUnity.y),
            Mathf.Abs(maxUnity.z - minUnity.z)
        );
        box.transform.position = centerUnity;
        box.transform.localScale = sizeUnity;
    }


    void WrenchCallback(geometry_msgs.msg.WrenchStamped msg)
    {
        var linear = msg.Wrench.Force;
        var angular = msg.Wrench.Torque;


        //passing the variabels to unity 
        //output force on the robot = - force from the user ( from observations)
        Vector3 unityLinearForce = new Vector3((float)-linear.X, (float)-linear.Y, (float)-linear.Z);
        Vector3 unityAngularForce = new Vector3((float)-angular.X, (float)-angular.Y, (float)-angular.Z);
        ///for debug
        rbt_force = unityLinearForce;
        rbt_torque = unityAngularForce;

        GlobalForce = R.MultiplyVector(unityLinearForce);
        GlobalTorque = R.MultiplyVector(unityAngularForce);
        GlobalTorque.x = -GlobalTorque.x;
        GlobalTorque.y = -GlobalTorque.y;
        GlobalTorque.z = -GlobalTorque.z;
        if (flip_x) GlobalForce.x = -GlobalForce.x;
        if (flip_y) GlobalForce.y = -GlobalForce.y;
        if (flip_z) GlobalForce.z = -GlobalForce.z;
        if (flip_x) GlobalTorque.x = -GlobalTorque.x;
        if (flip_y) GlobalTorque.y = -GlobalTorque.y;
        if (flip_z) GlobalTorque.z = -GlobalTorque.z;
        glb_rbt_force = GlobalForce;
        glb_rbt_torque = GlobalTorque;

        // ROS2MainThreadExecutor.RunOnMainThread(() =>
        // {
        //     physicalSphere.AddForce(GlobalForce);
        //     physicalSphere.AddTorque(GlobalTorque);
        //     lock (dataLock)
        //     {
        //         thread_safe_pose = physicalSphere.position - positionOffset;
        //         thread_safe_quaternion = physicalSphere.rotation;
        //     }
        // });

    }

    void PoseCallback(geometry_msgs.msg.PoseStamped msg)
    {
        var pos = msg.Pose.Position;
        var rot = msg.Pose.Orientation;
        /////position in the local referenc eof the robot translate it to unity 
        unityPosition = new Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);
        unityRotation = new Quaternion((float)rot.X, (float)rot.Y, (float)rot.Z, (float)rot.W);
        ///position transformed to the local reference of the face 
        global_pos = R.MultiplyPoint3x4(unityPosition);
        /////POST MULTIPLICATION : ROTATION DU ROBOT PUIS DU REPERE
        global_orient = R.rotation * unityRotation;
        /////////flipping 
        Vector3 eulerAngles = global_orient.eulerAngles;
        eulerAngles.x = -eulerAngles.x;
        eulerAngles.y = -eulerAngles.y;
        eulerAngles.z = -eulerAngles.z;
        if (flip_x) global_pos.x = -global_pos.x;
        if (flip_y) global_pos.y = -global_pos.y;
        if (flip_z) global_pos.z = -global_pos.z;
        if (flip_x) eulerAngles.x = -eulerAngles.x;
        if (flip_y) eulerAngles.y = -eulerAngles.y;
        if (flip_z) eulerAngles.z = -eulerAngles.z;
        global_orient = Quaternion.Euler(eulerAngles);

        lock (dataLock)
        {
            thread_safe_quaternion = unityRotation;
            /////////temp//////
            // thread_safe_pose = unityPosition; //- positionOffset;
        }
    }

    void TwistCallback(geometry_msgs.msg.TwistStamped msg)
    {
        var linear = msg.Twist.Linear;
        var angular = msg.Twist.Angular;
        unityangTwist = new Vector3((float)angular.X, (float)angular.Y, (float)angular.Z);
        unitylinTwist = new Vector3((float)linear.X, (float)linear.Y, (float)linear.Z);
        lock (dataLock)
        {
            thread_safe_ang_vel = unityangTwist;
            /////////temp//////
            // thread_safe_pose = unityPosition; //- positionOffset;
        }
    }

    void FixedUpdate()
    {
        physicalSphere.AddForce(GlobalForce);
        physicalSphere.AddTorque(GlobalTorque);

        ////clamp the position of the physical sphere 
        /// 
        Vector3 pos = physicalSphere.position;
        pos.x = Mathf.Clamp(pos.x, minUnity.x, maxUnity.x);
        pos.y = Mathf.Clamp(pos.y, minUnity.y, maxUnity.y);
        pos.z = Mathf.Clamp(pos.z, minUnity.z, maxUnity.z);
        physicalSphere.position = pos;



        robotee.position = global_pos;
        robotee.rotation = global_orient;

        lock (dataLock)
        {
            thread_safe_pose = physicalSphere.position; //- positionOffset;
            //thread_safe_quaternion = physicalSphere.rotation;
            thread_safe_lin_vel = physicalSphere.velocity;
            //thread_safe_ang_vel = physicalSphere.angularVelocity;
        }
    }



    private void PublishingLoop()
    {
        const long targetTicks = TimeSpan.TicksPerSecond / 800; // 1kHz in ticks
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long nextPublishTime = stopwatch.ElapsedTicks + targetTicks;

        while (isRunning)
        {
            Vector3 currentPose;
            Quaternion currentRotation;
            Vector3 currentlinVel;
            Vector3 currentangVel;

            lock (dataLock)
            {
                currentPose = thread_safe_pose;
                currentRotation = thread_safe_quaternion;
                currentangVel = thread_safe_ang_vel;
                currentlinVel = thread_safe_lin_vel;
            }
            
            bool posenotChanged = currentPose.Equals(lastPublishedPose);
            // bool twistnotChanged = currentlinVel.Equals(lastPublishedTwist);
            if (posenotChanged && interpolation_bool)
            {
                repeatedPublishCount++;
                float factor = Mathf.Pow(decay_factor, repeatedPublishCount); // 0.9^n for decay
                Vector3 basePose = (repeatedPublishCount == 1) ? lastPublishedPose : lastEstimatedPose;
                Vector3 estimatedPose = basePose + lastPublishedTwist * (1f / 1000f) * factor; 
                lastEstimatedPose = estimatedPose;
                PublishPose(estimatedPose, currentRotation);
                PublishTwist(lastPublishedTwist, currentangVel);
                
            }
            else
            {
                repeatedPublishCount = 0;
                lastPublishedPose = currentPose;
                lastPublishedTwist = currentlinVel;
                lastEstimatedPose = currentPose;
                PublishPose(currentPose, currentRotation);
                PublishTwist(currentlinVel, currentangVel);

            }
            

            //////temp///////
            // Check if pose or rotation has changed
            // poseMsg.Pose.Position.X = currentPose.x;
            // poseMsg.Pose.Position.Y = currentPose.y;
            // poseMsg.Pose.Position.Z = currentPose.z;
            // poseMsg.Pose.Orientation.X = currentRotation.x;
            // poseMsg.Pose.Orientation.Y = currentRotation.y;
            // poseMsg.Pose.Orientation.Z = currentRotation.z;
            // poseMsg.Pose.Orientation.W = currentRotation.w;
            // pose_pub.Publish(poseMsg);
            ////////temp////////



            // Precise timing using high-resolution timer
            while (stopwatch.ElapsedTicks < nextPublishTime && isRunning)
            {
                Thread.Yield(); // Give CPU to other threads briefly
            }
            nextPublishTime += targetTicks;
        }
    }

    void PublishPose(Vector3 position, Quaternion rotation)
    {
        if (ros2Unity.Ok())
        {
            if (flip_x) position.x = -position.x;
            if (flip_y) position.y = -position.y;
            if (flip_z) position.z = -position.z;
            Vector3 loc_to_robot_pose = R_inverse.MultiplyPoint(position);

            // Vector3 euler_angl = rotation.eulerAngles;
            // if (flip_x) euler_angl.x = -euler_angl.x;
            // if (flip_y) euler_angl.y = -euler_angl.y;
            // if (flip_z) euler_angl.z = -euler_angl.z;
            // Vector3 loc_to_robot_orient = R_inverse.MultiplyVector(euler_angl);
            // loc_to_robot_orient.x = -loc_to_robot_orient.x;
            // loc_to_robot_orient.y = -loc_to_robot_orient.y;
            // loc_to_robot_orient.z = -loc_to_robot_orient.z;

            // ////conversion from euler to quaternions 
            // float pitch = loc_to_robot_orient.x * Mathf.Deg2Rad * 0.5f; // X rotation
            // float yaw = loc_to_robot_orient.y * Mathf.Deg2Rad * 0.5f;   // Y rotation
            // float roll = loc_to_robot_orient.z * Mathf.Deg2Rad * 0.5f;
            // float cp = Mathf.Cos(pitch);
            // float sp = Mathf.Sin(pitch);
            // float cy = Mathf.Cos(yaw);
            // float sy = Mathf.Sin(yaw);
            // float cr = Mathf.Cos(roll);
            // float sr = Mathf.Sin(roll);

            // // Calculate quaternion components using ZYX rotation order
            // float w = cp * cy * cr + sp * sy * sr;
            // float x = sp * cy * cr - cp * sy * sr;
            // float y = cp * sy * cr + sp * cy * sr;
            // float z = cp * cy * sr - sp * sy * cr;



            float x = rotation.x;
            float y = rotation.y;
            float z = rotation.z;
            float w = rotation.w;

            
            loc_to_robot_pose.x = Mathf.Clamp(loc_to_robot_pose.x, min.x, max.x);
            loc_to_robot_pose.y = Mathf.Clamp(loc_to_robot_pose.y, min.y, max.y);
            loc_to_robot_pose.z = Mathf.Clamp(loc_to_robot_pose.z, min.z, max.z);


            poseMsg.Pose.Position.X = loc_to_robot_pose.x;
            poseMsg.Pose.Position.Y = loc_to_robot_pose.y;
            poseMsg.Pose.Position.Z = loc_to_robot_pose.z;
            poseMsg.Pose.Orientation.X = x;
            poseMsg.Pose.Orientation.Y = y;
            poseMsg.Pose.Orientation.Z = z;
            poseMsg.Pose.Orientation.W = w;
            pose_pub.Publish(poseMsg);
        }
    }
    void PublishTwist(Vector3 linear, Vector3 angular)
    {
        if (ros2Unity.Ok())
        {
            if (flip_x) linear.x = -linear.x;
            if (flip_y) linear.y = -linear.y;
            if (flip_z) linear.z = -linear.z;
            Vector3 loc_to_robot_lin = R_inverse.MultiplyVector(linear);

            ///rotation
            // if (flip_x) angular.x = -angular.x;
            // if (flip_y) angular.y = -angular.y;
            // if (flip_z) angular.z = -angular.z;
            // Vector3 loc_to_robot_ang = R_inverse.MultiplyVector(angular);
            // loc_to_robot_ang.x = -loc_to_robot_ang.x;
            // loc_to_robot_ang.y = -loc_to_robot_ang.y;
            // loc_to_robot_ang.z = -loc_to_robot_ang.z;


            // twistMsg.Twist.Linear.X = loc_to_robot_lin.x;
            // twistMsg.Twist.Linear.Y = loc_to_robot_lin.y;
            // twistMsg.Twist.Linear.Z = loc_to_robot_lin.z;

            // twistMsg.Twist.Angular.X = loc_to_robot_ang.x;
            // twistMsg.Twist.Angular.Y = loc_to_robot_ang.y;
            // twistMsg.Twist.Angular.Z = loc_to_robot_ang.z;


            // Clamp linear velocities (m/s)
            twistMsg.Twist.Linear.X = Mathf.Clamp(loc_to_robot_lin.x, minLinear.x, maxLinear.x);
            twistMsg.Twist.Linear.Y = Mathf.Clamp(loc_to_robot_lin.y, minLinear.y, maxLinear.y);
            twistMsg.Twist.Linear.Z = Mathf.Clamp(loc_to_robot_lin.z, minLinear.z, maxLinear.z);
            // Clamp angular velocities (rad/s)
            // twistMsg.Twist.Angular.X = Mathf.Clamp(loc_to_robot_ang.x, minAngular.x, maxAngular.x);
            // twistMsg.Twist.Angular.Y = Mathf.Clamp(loc_to_robot_ang.y, minAngular.y, maxAngular.y);
            // twistMsg.Twist.Angular.Z = Mathf.Clamp(loc_to_robot_ang.z, minAngular.z, maxAngular.z);


            ///clamping ignored , returning back the robot own angular speed 
            twistMsg.Twist.Angular.X = angular.x;
            twistMsg.Twist.Angular.Y = angular.y;
            twistMsg.Twist.Angular.Z = angular.z;


            twist_pub.Publish(twistMsg);
        }
        
    }

    void OnDestroy()
    {
        isRunning = false;
        if (publishingThread != null && publishingThread.IsAlive)
        {
            publishingThread.Join(1000);
        }
    }

}

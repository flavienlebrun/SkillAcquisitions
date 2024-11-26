using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomDeviceInfo : MonoBehaviour
{
    /// <summary>
    /// Device configuration name
    /// </summary>
    public string Name;

    /// <summary>
    /// Device handler
    /// </summary>
    public uint hHdAPI;

    /// <summary>
    /// Device position
    /// </summary>
    public Vector3 position;

    /// <summary>
    /// Device rotation
    /// </summary>
    public Quaternion rotation;

    public Quaternion correctionRotation;

    public bool inside;

    public Vector3 correctionPosition;

    /// <summary>
    /// Force to be applied to the attached device
    /// </summary>
    public Vector3 force;

    /// <summary>
    /// Scene object attached to this device's position and rotation
    /// </summary>
    public GameObject tool;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshRenderer))]
public class XRCalibration : MonoBehaviour
{
    [SerializeField]
    private InputAction calibrationButton;
    [SerializeField]
    private InputAction activationButton;

    private Unity.XR.CoreUtils.XROrigin XROrigin;

    [SerializeField]
    private Transform XRControllerVisual;

    public Transform Reference;

    [SerializeField] private bool UseY;

    private GameObject cube;

    private bool isCalibrationButtonPressed = false;
    private bool isActivationButtonPressed = false;

    private void OnEnable()
    {
        // Assignez des callbacks pour les actions
        calibrationButton.performed += OnCalibrationButtonPressed;
        calibrationButton.canceled += OnCalibrationButtonReleased;
        activationButton.performed += OnActivationButtonPressed;
        activationButton.canceled += OnActivationButtonReleased;

        // Activez les actions
        calibrationButton.Enable();
        activationButton.Enable();
    }

    private void Start()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        XROrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (XROrigin == null)
        {
            Debug.LogError("XROrigin is missing in the scene!");
        }

        cube.GetComponent<Renderer>().material.color = Color.yellow;
    }

    private void OnDisable()
    {
        // Désactivez les actions et retirez les callbacks
        calibrationButton.Disable();
        activationButton.Disable();

        calibrationButton.performed -= OnCalibrationButtonPressed;
        calibrationButton.canceled -= OnCalibrationButtonReleased;
        activationButton.performed -= OnActivationButtonPressed;
        activationButton.canceled -= OnActivationButtonReleased;
    }

    void OnCalibrationButtonPressed(InputAction.CallbackContext context)
    {
        isCalibrationButtonPressed = true;
        Debug.Log("calibrationButton Pressed");
    }

    void OnCalibrationButtonReleased(InputAction.CallbackContext context)
    {
        isCalibrationButtonPressed = false;
        Debug.Log("calibrationButton Released");
    }

    void OnActivationButtonPressed(InputAction.CallbackContext context)
    {
        isActivationButtonPressed = true;
        Debug.Log("activationButton Pressed");
    }

    void OnActivationButtonReleased(InputAction.CallbackContext context)
    {
        isActivationButtonPressed = false;
        Debug.Log("activationButton Released");
    }

    private void Update()
    {
        if (isActivationButtonPressed)
        {
            cube.GetComponent<Renderer>().enabled = true;

            transform.position = XRControllerVisual.position;
            transform.localRotation = Quaternion.Euler(0f, XRControllerVisual.rotation.eulerAngles.y, 0f);
            cube.transform.position = transform.position;
            cube.transform.localRotation = transform.localRotation;

            if(isCalibrationButtonPressed)
            {

                float diffX = Reference.position.x - transform.position.x;
                float diffY = 0;
                if(UseY)
                {
                    diffY = Reference.position.y - transform.position.y;                    
                }

                float diffZ = Reference.position.z - transform.position.z;   



                Quaternion diffRotation = Reference.rotation * Quaternion.Inverse(transform.rotation);
                XROrigin.transform.position += new Vector3(diffX,diffY,diffZ);
                XROrigin.transform.rotation *= diffRotation;

                if (Mathf.Abs(transform.position.x - Reference.position.x) < 0.0001f &&
                    Mathf.Abs(transform.position.y - Reference.position.y) < 0.0001f &&
                    Mathf.Abs(transform.position.z - Reference.position.z) < 0.0001f &&
                    Mathf.Abs(transform.rotation.y - Reference.rotation.y) < 1f)
                {
                    cube.GetComponent<Renderer>().material.color = Color.green;
                }
                else
                {
                    cube.GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
        }
        else
        {
            cube.GetComponent<Renderer>().enabled = false;            
        }
    }
}

/* Possibilité d'ajouter plus de controleurs */
public enum XRInputController
{
    LTouch,
    RTouch
}

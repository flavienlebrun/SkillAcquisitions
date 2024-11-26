using System.Collections.Generic;
using UnityEngine;

public class ZoomScreen : MonoBehaviour
{
    static public ZoomScreen Instance { get; private set; }

    [SerializeField]
    private List<Camera> CrossCamList = new List<Camera>();

    private int maxZoom = 10;
    private int minZoom = 1;

    public bool ZoomEnabled;
    public int currentZoom = 5;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {

        if(CrossCamList.Count == 0)
        {
            Debug.LogWarning("CrossCamList in ZoomScreen is empty !");
        }

        if(GameManager.Instance.EnabledHaptic)
        {
            TwoHapticsProbeNeedle.instance.ZoomDown += ZoomDown;
            TwoHapticsProbeNeedle.instance.ZoomUp += ZoomUp;
        }

        SwitchZoom();
    }

    public void ZoomUp()
    {

        if(ZoomEnabled)
        {
            if (currentZoom == maxZoom)
            {
                return;
            }

            currentZoom++;
            SwitchZoom();
        }  
    }

    public void ZoomDown()
    {
        if(ZoomEnabled)
        {
            if (currentZoom == minZoom)
            {
                return;
            }

            currentZoom--;
            SwitchZoom();
        }
    }

    private void SwitchZoom()
    {
        switch(maxZoom + 1 - currentZoom)
        {
            case 1:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -1f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.01f;
                }
                break;

            case 2:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -2f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.02f;
                }
                break;

            case 3:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -3f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.03f;
                }
                break;

            case 4:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -4f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.04f;
                }
                break;

            case 5:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -5f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.05f;
                }
                break;

            case 6:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -6f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.06f;
                }
                break;

            case 7:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -7f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.07f;
                }
                break;

            case 8:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -8f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.08f;
                }
                break;

            case 9:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -9f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.09f;
                }
                break;

            case 10:
                foreach (Camera cam in CrossCamList)
                {
                    var newPos = cam.transform.localPosition;
                    newPos.y = -10f;
                    cam.transform.localPosition = newPos;

                    cam.orthographicSize = 0.10f;
                }
                break;
        }

        CanvasEchographe.Instance.UpdateUIZoom(currentZoom);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance.EnabledHaptic)
        {
            TwoHapticsProbeNeedle.instance.ZoomDown -= ZoomDown;
            TwoHapticsProbeNeedle.instance.ZoomUp -= ZoomUp;
        }
    }
}

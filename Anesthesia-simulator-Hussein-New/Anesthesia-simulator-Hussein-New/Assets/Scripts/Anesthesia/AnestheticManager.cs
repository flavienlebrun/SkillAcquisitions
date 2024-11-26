using UnityEngine;
using System;
using Phidget22;
using Phidget22.Events;

public class AnestheticManager : MonoBehaviour
{
    public static AnestheticManager Instance { get; private set; }

    private bool NeedleInsideArea = false;
    private int StateUp = 0;
    private int StateDown = 0;
    public bool SuccessfulAnesthesia = false;
    private Vector3 lastNeedlePosition;

    [SerializeField]
    private Transform Needle = null;

    [SerializeField]
    private Transform AnesthesiaFeedback = null;

    public float minScale;
    public float maxScale;
    private TimeSpan TimeMiddleAnesthesia = TimeSpan.Zero;

    private int lastPotentiometerValue = 0;
    private const int maxPotentiometerValue = 200;
    private bool isInSecondPhase = false;

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
        if (GameManager.Instance.EnabledHaptic)
        {
            TwoHapticsProbeNeedle.instance.InsertAnesthesic += ApplyAnesthesic;
        }

        // Initialize the lastNeedlePosition
        if (Needle != null)
        {
            lastNeedlePosition = Needle.position;
        }
    }

    private void Update()
    {
        // Update the anesthetic state based on needle position
        if (Needle != null)
        {
            UpdateAnestheticState();
            lastNeedlePosition = Needle.position;
        }
    }

    private void UpdateAnestheticState()
    {
        // Detect when the needle crosses the threshold
        if (lastNeedlePosition.y > transform.position.y && Needle.position.y <= transform.position.y)
        {
            // Needle moved from above to below the threshold
            CanvasEchographe.Instance.UpdateUIDownAnesthesia(StateDown);
        }
        else if (lastNeedlePosition.y <= transform.position.y && Needle.position.y > transform.position.y)
        {
            // Needle moved from below to above the threshold
            CanvasEchographe.Instance.UpdateUIUpAnesthesia(StateUp);
        }
    }

    private void ApplyAnesthesic(int amount)
    {
        if (NeedleInsideArea && !NerveCollision.Instance.NerveIsTouch && !SuccessfulAnesthesia)
        {
            int increment = Mathf.Clamp(amount - lastPotentiometerValue, 0, maxPotentiometerValue);

            if (!isInSecondPhase)
            {
                if (Needle.position.y > transform.position.y)
                {
                    StateUp = Mathf.Clamp(StateUp + increment, 0, 100);
                    CanvasEchographe.Instance.UpdateUIUpAnesthesia(StateUp);
                }
                else
                {
                    StateDown = Mathf.Clamp(StateDown + increment, 0, 100);
                    CanvasEchographe.Instance.UpdateUIDownAnesthesia(StateDown);
                }

                if (StateUp == 100 || StateDown == 100)
                {
                    isInSecondPhase = true;
                    lastPotentiometerValue = 0; // Reset the potentiometer value
                }
            }
            else
            {
                // In the second phase, keep adding to the existing value
                if (Needle.position.y > transform.position.y)
                {
                    StateUp = Mathf.Clamp(StateUp + increment, 0, 100);
                    CanvasEchographe.Instance.UpdateUIUpAnesthesia(StateUp);
                }
                else
                {
                    StateDown = Mathf.Clamp(StateDown + increment, 0, 100);
                    CanvasEchographe.Instance.UpdateUIDownAnesthesia(StateDown);
                }
            }

            lastPotentiometerValue = amount; // Update last potentiometer value
            UpdateAnesthesiaFeedback();
            CheckForSuccessfulAnesthesia();
        }
    }

    public void RemoveAnesthesic(int amount)
    {
        if (!SuccessfulAnesthesia)
        {
            int decrement = Mathf.Clamp(lastPotentiometerValue - amount, 0, maxPotentiometerValue);

            if (Needle.position.y > transform.position.y)
            {
                StateUp = Mathf.Clamp(StateUp - decrement, 0, 100);
                CanvasEchographe.Instance.UpdateUIUpAnesthesia(StateUp);
            }
            else
            {
                StateDown = Mathf.Clamp(StateDown - decrement, 0, 100);
                CanvasEchographe.Instance.UpdateUIDownAnesthesia(StateDown);
            }

            lastPotentiometerValue = amount; // Update last potentiometer value
            UpdateAnesthesiaFeedback();
        }
    }

    private void UpdateAnesthesiaFeedback()
    {
        Vector3 temp = transform.localScale;
        var temp2 = Map(StateDown + StateUp, 0, 200, minScale, maxScale);
        temp.x = temp2;
        temp.y = temp2;
        AnesthesiaFeedback.transform.localScale = temp;
    }

    private void CheckForSuccessfulAnesthesia()
    {
        if (StateUp + StateDown >= 100 && TimeMiddleAnesthesia == TimeSpan.Zero)
        {
            TimeMiddleAnesthesia = TimeSpan.FromSeconds(CanvasEchographe.Instance.elapsedTime);
        }

        if (StateUp + StateDown == 200)
        {
            SuccessfulAnesthesia = true;
            CanvasEchographe.Instance.StopTimer();

            if (GameManager.Instance.Mode == Mode.Reality)
            {
                DataRecorder.Instance.SaveData(
                    CanvasEchographe.Instance.timePlaying,
                    NeedleCollision.Instance.firstInsertion,
                    TimeMiddleAnesthesia,
                    NeedleCollision.Instance.NbInsertion,
                    CanvasEchographe.Instance.NbNerveTouch,
                    CanvasEchographe.Instance.NbVeinTouch,
                    CanvasEchographe.Instance.NbArteryTouch
                );
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NeedleInsideArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NeedleInsideArea = false;
        }
    }

    private float Map(float value, float FromLow, float ToLow, float FromHigh, float ToHigh)
    {
        return (ToHigh - FromHigh) * ((value - FromLow) / (ToLow - FromLow)) + FromHigh;
    }
}













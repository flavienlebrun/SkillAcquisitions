using UnityEngine;

public class VeineDeformation : MonoBehaviour
{
    [SerializeField]
    private Transform veine = null;

    private float NormalPositionY;
    private float NormalScaleY;

    public float NewPositionY = -5f;
    public float NewScaleY; 

    [SerializeField]
    private float MinForce = 0;

    [SerializeField]
    private float MaxForce = 0;

    private void Start()
    {
        if(veine != null)
        {
            NormalPositionY = veine.localPosition.y;
            NormalScaleY = veine.localScale.y;
        }

        if(GameManager.Instance.EnabledHaptic)
        {
            TwoHapticsProbeNeedle.instance.ForceProbeApply += ApplyDeformation;
        }
    }

    private void ApplyDeformation(float force)
    {
        if(force >= MinForce)
        {
            if (force >= MaxForce)
            {
                veine.gameObject.SetActive(false);
            }
            else
            {
                veine.gameObject.SetActive(true);
            }

            float ResizePosY = Map(force, MinForce, MaxForce, NormalPositionY, NewPositionY);
            float ResizeScaleY = Map(force, MinForce, MaxForce, NormalScaleY, NewScaleY);

            var tempPos = veine.localPosition;
            tempPos.y = ResizePosY;
            veine.localPosition = tempPos;

            var tempScale = veine.localScale;
            tempScale.y = ResizeScaleY;
            veine.localScale = tempScale;
        }
    }

    private float Map(float value, float FromLow, float ToLow, float FromHigh, float ToHigh)
    {
        return (ToHigh - FromHigh) * ((value - FromLow) / (ToLow - FromLow)) + FromHigh;
    }
}

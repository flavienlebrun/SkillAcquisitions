using UnityEngine;

[CreateAssetMenu(fileName = "HapticConfig", menuName = "Haptic/HapticConfig", order = 1)]
public class HapticConfig : ScriptableObject
{
    public float FirstPlanePosition = 0;
    public float SecondPlanePosition = -1.5f;

    public float FirstPlaneStiffness = 0.25f;
    public float SecondPlaneStiffness = 0.33f;

    public float SkinLayerStiffness = 31.5f;

    public Vector3 TISSUE_DIMENSIONS = new Vector3(27, 0, 20.25f);

    public float FIRST_LAYER_TOP = 0.10f;

    public float DEVICE_FORCE_SCALE = 0.4f;

    public float FirstLayerDamping = 4f;

    public float SkinLayerCutting = 1.22f;

    public float UnitLength = 0.001f;
}
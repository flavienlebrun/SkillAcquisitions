using UnityEngine;

[RequireComponent(typeof(Animation))]
public class TouchNerveEffect : MonoBehaviour
{
    private Animation anim;

    private void Start()
    {
        anim = GetComponent<Animation>();
    }

    private void PllayerEffect()
    {
        if(anim.isPlaying)
        {
            return;
        }

        anim.Play();
    }
}

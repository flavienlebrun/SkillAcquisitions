using UnityEngine;

public class VeinCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            CanvasEchographe.Instance.UpdateTouchVein();
        }
    }
}

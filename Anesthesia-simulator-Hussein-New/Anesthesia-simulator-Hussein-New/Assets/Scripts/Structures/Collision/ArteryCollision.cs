using UnityEngine;

public class ArteryCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            CanvasEchographe.Instance.UpdateTouchArtery();
        }
    }
}

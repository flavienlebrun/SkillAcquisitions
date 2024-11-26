using UnityEngine;

public class DisplayScreenProbe : MonoBehaviour
{
    static public DisplayScreenProbe Instance { get; private set; }

    [SerializeField]
    private string SkinTag = "Skin";
    [SerializeField]
    private GameObject CrossCamsHandler = null;
    [SerializeField]
    private Material composite = null;
    [SerializeField]
    private Material ScreenPlaceHolder = null;
    [SerializeField]
    private GameObject Screenrenderer = null;

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
        CrossCamsHandler.SetActive(false);
        Screenrenderer.GetComponent<Renderer>().material = ScreenPlaceHolder;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == SkinTag)
        {
            CrossCamsHandler.SetActive(true);
            Screenrenderer.GetComponent<Renderer>().material = composite;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.tag == SkinTag)
        {
            Screenrenderer.GetComponent<Renderer>().material = ScreenPlaceHolder;
            CrossCamsHandler.SetActive(false);
        }
    }
}

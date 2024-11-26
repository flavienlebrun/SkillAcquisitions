using UnityEngine;

public class NerveCollision : MonoBehaviour
{
    static public NerveCollision Instance { get; private set; }

    public bool Penalty = false;

    [SerializeField]
    private int PenaltyValue = 10;

    [HideInInspector]
    public bool NerveIsTouch = false;

    private AudioBox audioBox;

    [SerializeField]
    private Animator CameraVRAnim = null;

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
        audioBox = GetComponent<AudioBox>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NerveIsTouch = true;
            CanvasEchographe.Instance.UpdateTouchNerve();

            if (GameManager.Instance.Mode == Mode.Reality)
            {
                audioBox.StopAll();
                audioBox.PlayOneShot(SoundOneShot.CryPain);
                CameraVRAnim.SetBool("PlayEffect", true);

                if(Penalty)
                {
                    AnestheticManager.Instance.RemoveAnesthesic(PenaltyValue);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Needle")
        {
            NerveIsTouch = false;
            if (GameManager.Instance.Mode == Mode.Reality)
            {
                CameraVRAnim.SetBool("PlayEffect", false);           
            }
        }
    }
}

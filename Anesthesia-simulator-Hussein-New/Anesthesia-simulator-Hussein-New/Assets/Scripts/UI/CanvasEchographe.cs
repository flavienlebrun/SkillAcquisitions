using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CanvasEchographe : MonoBehaviour
{
    public static CanvasEchographe Instance { get; private set; }

    [SerializeField]
    private TMPro.TMP_Text DateAndHourTxt = null;

    [SerializeField]
    private TMPro.TMP_Text ZoomTxt = null;

    [SerializeField]
    private TMPro.TMP_Text TimeTxt = null;

    [SerializeField]
    private TMPro.TMP_Text UpPartAnsthesiaTxt = null;
    [SerializeField]
    private TMPro.TMP_Text DownPartAnsthesiaTxt = null;

    [SerializeField]
    public Image BarUpPart = null;

    [SerializeField]
    public Image BarDownPart = null;

    [SerializeField]
    private TMPro.TMP_Text TouchNerveTxt = null;

    [SerializeField]
    private TMPro.TMP_Text TouchVeinTxt = null;

    [SerializeField]
    private TMPro.TMP_Text TouchArteryTxt = null;

    [HideInInspector]
    public bool TimerOngoing = false;

    public float elapsedTime;

    [HideInInspector]  
    public TimeSpan timePlaying;

    [HideInInspector]
    public int NbNerveTouch;

    [HideInInspector]
    public int NbVeinTouch;

    [HideInInspector]
    public int NbArteryTouch;

    public Coroutine Timer;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        DateAndHourTxt.text = System.DateTime.Now.ToString();
    }

    public void StartTimer()
    {
        if(GameManager.Instance.Mode == Mode.Reality)
        {
            TimerOngoing = true;
            Timer = StartCoroutine(UpdateTimer());            
        }
    }

    public void StopTimer()
    {
        TimerOngoing = false;
        StopCoroutine(Timer);
    }

    public void UpdateUIZoom(int zoom)
    {
        ZoomTxt.text = "  Zoom : " + zoom;
    }

    public void UpdateUIUpAnesthesia(int percentage)
    {
        UpPartAnsthesiaTxt.text = percentage + " %";
        BarUpPart.fillAmount = percentage / 100f;
    }

    public void UpdateUIDownAnesthesia(int percentage)
    {
        DownPartAnsthesiaTxt.text = percentage + " %";
        BarDownPart.fillAmount = percentage / 100f;
    }

    public void UpdateTouchNerve()
    {
        NbNerveTouch++;
        TouchNerveTxt.color = Color.red;
        TouchNerveTxt.text = NbNerveTouch.ToString();
    }

    public void UpdateTouchVein()
    {
        NbVeinTouch++;
        TouchVeinTxt.color = new Color(1f, 0.647f, 0f);
        TouchVeinTxt.text = NbVeinTouch.ToString();
    }

    public void UpdateTouchArtery()
    {
        NbArteryTouch++;
        TouchArteryTxt.color = new Color(1f, 0.647f, 0f);
        TouchArteryTxt.text = NbArteryTouch.ToString();
    }

    public IEnumerator UpdateTimer()
    {
        while(TimerOngoing)
        {
            elapsedTime += Time.deltaTime;
            timePlaying = TimeSpan.FromSeconds(elapsedTime);

            string minute = string.Format("{0:D2}", timePlaying.Minutes);
            string seconds = string.Format("{0:D2}", timePlaying.Seconds);

            TimeTxt.text = "  Time : " + minute + " : " + seconds;

            yield return null;
        }
    }
}

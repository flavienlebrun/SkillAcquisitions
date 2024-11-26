using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodFlowManager : MonoBehaviour
{
    private List<Vector3> PathNodes = new List<Vector3>();
    public GameObject ParentPath;
    public GameObject BloodPathVisual;
    public GameObject BloodCell;
    public GameObject DrawHere;
    public float AnimationPause;
    public Slider ThickSlider;
    int CurrentNode;
    private Vector2 startPosition;
    private bool DrawPath;
    public bool PlayNow;
    public int CurrentNodeIndex;

    // Start is called before the first frame update
    void Start()
    {
        ResetPath();
        AnimationPause = 0.5f;
    }

    // Transforms mouse screen position to world position and creates node for blood path based on mouse position 
    private void CreateNode()
    {
        Vector3 p = Input.mousePosition;
        p.z = 20;
        Vector3 pos = Camera.main.ScreenToWorldPoint(p);
        PathNodes.Add(pos);
        var newBlood = Instantiate(BloodPathVisual, pos, Quaternion.identity);
        newBlood.transform.parent = ParentPath.transform;
    }

    // Gets called from ThickSlider when slider changes, scales blood particle and changes sp
    public void ChangeBloodThinkness()
    {
        BloodCell.transform.localScale = new Vector3(1 + ThickSlider.value, 1 + ThickSlider.value, 1 + ThickSlider.value);
        AnimationPause = 0.5f + ThickSlider.value;
    }

    //Erases existing path and resets indexs
    public void ResetPath()
    {
        StopAllCoroutines();
        foreach (Transform child in ParentPath.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        PathNodes.Clear();
        CurrentNodeIndex = 0;
    }


    // Animates blood particle based on Movement speed which is set in ChangeBloodThinkess function;
    public IEnumerator Flow()
    {
        Debug.Log("inside coroutine");

        while (CurrentNodeIndex < PathNodes.Count - 1)
        {
            BloodCell.transform.position = PathNodes[CurrentNodeIndex + 1];
            CurrentNodeIndex++;
            //loop back if blood reaches end position;
            if (CurrentNodeIndex == PathNodes.Count - 1)
            {
                BloodCell.transform.position = PathNodes[0];
                CurrentNodeIndex = 0;
            }
            yield return new WaitForSeconds(AnimationPause / 10);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DrawPath = true;
            DrawHere.SetActive(false);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            DrawPath = false;
            Debug.Log("trying to start corutine");
            StartCoroutine(Flow());
        }
        if (DrawPath)
        {
            CreateNode();
        }

    }
}

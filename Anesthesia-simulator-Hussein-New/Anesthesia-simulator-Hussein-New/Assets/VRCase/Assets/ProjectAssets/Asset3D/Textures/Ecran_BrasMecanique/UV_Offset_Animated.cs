using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UV_Offset_Animated : MonoBehaviour {

    public float scrollSpeed = 0.3F;
    public Renderer rend;
    private float offset;
   // public Material MatScreenScope;
    void Start()
    {

    }
    void Update()
    {
        offset = Time.time * scrollSpeed;
        rend.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }

    public void BlackScreen()
    {
        rend.material.color = Color.black;
    }

    public void MovingScreen()
    {
        rend.material.color = Color.white;
    }

    public void OnEnable()
    {
        MovingScreen();
    }

    public void OnDisable()
    {
        BlackScreen();
    }

}

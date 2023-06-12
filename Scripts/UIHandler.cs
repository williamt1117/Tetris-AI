using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public GameObject mapBuilderGameObject;
    public TMP_Text scoreKeepingText;
    private MapHandler mh;

    void Start() {
        mh = GetComponentInParent<MapHandler>();
    }

    void Update()
    {
        scoreKeepingText.text = "Lines Cleared: " + mh.score;
    }
}

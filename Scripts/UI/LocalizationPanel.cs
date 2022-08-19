using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationPanel : MonoBehaviour
{
    public Text text1;
    public Text text2;
    public Text text3;
    public Text text4;
    void Start()
    {
        text1.text = LocalizationMgr.Instance.LoadText("145251006");
        text2.text = LocalizationMgr.Instance.LoadText("703840237");
        text3.text = LocalizationMgr.Instance.LoadText("846097046");
        text4.text = LocalizationMgr.Instance.LoadText("-1318004839");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

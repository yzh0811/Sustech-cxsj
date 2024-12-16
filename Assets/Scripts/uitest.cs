using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    //ªÒ»°uiµƒ∏∏‘™Àÿ
    private Transform UITest;
    //√Ê∞Â Ù–‘
    private int PanelValue = 0;
    void Start()
    {
        UITest = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeUIPanel();
    }

    //«–ªªui√Ê∞Â
    void ChangeUIPanel()
    {
        //Õ˘◊Û«–ªª Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            switch (PanelValue)
            {
                case 0:
                    UITest.GetChild(0).gameObject.SetActive(false);
                    UITest.GetChild(4).gameObject.SetActive(true);
                    PanelValue = 4;
                    break;
                case 1:
                    UITest.GetChild(1).gameObject.SetActive(false);
                    UITest.GetChild(0).gameObject.SetActive(true);
                    PanelValue = 0;
                    break;
                case 2:
                    UITest.GetChild(2).gameObject.SetActive(false);
                    UITest.GetChild(1).gameObject.SetActive(true);
                    PanelValue = 1;
                    break;
                case 3:
                    UITest.GetChild(3).gameObject.SetActive(false);
                    UITest.GetChild(2).gameObject.SetActive(true);
                    PanelValue = 2;
                    break;
                case 4:
                    UITest.GetChild(4).gameObject.SetActive(false);
                    UITest.GetChild(3).gameObject.SetActive(true);
                    PanelValue = 3;
                    break;
            }
        }
        //Õ˘”“«–ªª E
        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (PanelValue)
            {
                case 0:
                    UITest.GetChild(0).gameObject.SetActive(false);
                    UITest.GetChild(1).gameObject.SetActive(true);
                    PanelValue = 1;
                    break;
                case 1:
                    UITest.GetChild(1).gameObject.SetActive(false);
                    UITest.GetChild(2).gameObject.SetActive(true);
                    PanelValue = 2;
                    break;
                case 2:
                    UITest.GetChild(2).gameObject.SetActive(false);
                    UITest.GetChild(3).gameObject.SetActive(true);
                    PanelValue = 3;
                    break;
                case 3:
                    UITest.GetChild(3).gameObject.SetActive(false);
                    UITest.GetChild(4).gameObject.SetActive(true);
                    PanelValue = 4;
                    break;
                case 4:
                    UITest.GetChild(4).gameObject.SetActive(false);
                    UITest.GetChild(0).gameObject.SetActive(true);
                    PanelValue = 0;
                    break;
            }
        }
    }
}

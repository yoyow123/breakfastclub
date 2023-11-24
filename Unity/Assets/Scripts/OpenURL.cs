using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OpenURL : MonoBehaviour
{
    public Button btn;
    public string url;
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(() => OnBrowse(url));
    }

    public void OnBrowse(string url) {
        Application.OpenURL(url);
    }
}

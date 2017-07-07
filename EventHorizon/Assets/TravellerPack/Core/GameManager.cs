using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public bool m_useFps = false;
    public Text m_fps;

    void Awake()
    {
        if(m_useFps && !Debug.isDebugBuild)
        {
            m_useFps = false;
        }
        else
        {
            m_fps.gameObject.SetActive(m_useFps);
        }
    }

	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (m_useFps)
        {
            m_fps.text = (1.0f / Time.deltaTime).ToString("F2");
        }
    }
}

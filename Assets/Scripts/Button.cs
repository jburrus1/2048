using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public Util.ButtonType type;
    private void OnMouseDown()
    {
        switch (type)
        {

            case Util.ButtonType.Reset:
                 SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                //PlayerPrefs.SetInt("highscore", 0);     
                //PlayerPrefs.Save();
                break;
            case Util.ButtonType.Quit:
                Application.Quit();
                break;
        }
    }
}

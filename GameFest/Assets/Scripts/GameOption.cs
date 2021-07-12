using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOption : MonoBehaviour
{
    public GameObject Selection;

    public Scene SceneIndex;

    public void Selected()
    {
        Selection.SetActive(true);
        // TODO: start coroutine to show video
    }

    public void Deselected()
    {
        Selection.SetActive(false);
    }

}

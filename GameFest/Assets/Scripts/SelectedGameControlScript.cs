using UnityEngine;
using UnityEngine.UI;

public class SelectedGameControlScript : MonoBehaviour
{
    const int OFFSET = 2;

    public Image RendererLogo;
    public Image RendererBackground;
    public Sprite[] GameLogos;
    public Sprite[] GameBackgrounds;
    public GameObject DeleteIcon;

    public void SetImage(Scene game)
    {
        var index = (int)game - OFFSET;
        RendererLogo.sprite = GameLogos[index];
        RendererBackground.sprite = GameBackgrounds[index];

        gameObject.SetActive(true);
    }

    public void CanDelete(bool state)
    {
        DeleteIcon.SetActive(state);
    }
}

using System;
using UnityEngine;

public class ReceiptScript : MonoBehaviour
{
    // display components
    public TextMesh[] ReceiptTexts;
    public TextMesh TotalText;
    public TextMesh TotalHeader;
    public TextMesh[] ReceiptTextsScores;

    // configuration
    private const int STRING_LENGTH = 25;

    /// <summary>
    /// Sets the content of one of the text fields
    /// </summary>
    /// <param name="index">The index of the text to update</param>
    /// <param name="content">The text to display</param>
    public void SetText(int index, string content, int value)
    {
        // if there is data, display it, otherwise it is blank
        if (!string.IsNullOrEmpty(content))
        {
            ReceiptTexts[index].text = content;
            ReceiptTextsScores[index].text = value.ToString();
        }
        else
        {
            ReceiptTexts[index].text = "";
            ReceiptTextsScores[index].text = "";
        }
    }

    /// <summary>
    /// Puts all texts back to blank
    /// </summary>
    public void ResetTexts()
    {
        // reset all lines
        foreach (var txt in ReceiptTexts)
            txt.text = "";

        foreach (var txt in ReceiptTextsScores)
            txt.text = "";

        // update the total text
        TotalHeader.text = "";
        TotalText.text = "";
    }
}

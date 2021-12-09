using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelfieDisplayScript : MonoBehaviour
{
    public Text TxtUsername;
    public Text TxtCaption;
    public Text TxtFollowers;
    public Image ImgBackground;
    public Image ImgTaker;
    public Image ImgSubject;
    public RectTransform PointsLabel;

    public Sprite[] TakerSprites;
    public Sprite[] SubjectSprites;
    public Sprite[] BackgroundSprites;

    FollowBackInputHandler _owner;

    string[] HashtagOptions = new string[] { "friends", "bffs", "fun", "family", "happiness", "joy", "summervibez",
                                            "poppop", "fasterthanthehogwartsexpress", "ladsladslads", "tbt", "lol",
                                            "wayhayyy", "anawasliketha", "ohright", "wellwellwell", "goodtimes", "vibin" };

    string[] CaptionOptions = new string[] { "Chillin' with my homie", "Spot the", "Who's that hiding back there?", "Too school for cool", "Don't be shy",
                                            "Got you", "Following", "My fave", "Love ya" };

    /// <summary>
    /// Sets the content of the selfie
    /// </summary>
    /// <param id="players">The players in the selfie</param>
    public void Setup(Tuple<FollowBackInputHandler, FollowBackInputHandler> players)
    {
        _owner = players.Item1;

        // set background image
        var randomBackground = UnityEngine.Random.Range(0, BackgroundSprites.Length);
        ImgBackground.sprite = BackgroundSprites[randomBackground];

        // set player images
        ImgTaker.sprite = TakerSprites[_owner.GetCharacterIndex()];
        ImgSubject.sprite = SubjectSprites[players.Item2.GetCharacterIndex()];

        // set text diplays
        TxtUsername.text = "@" + _owner.GetPlayerName();
        TxtCaption.text = GetCaption_() + " <i>@" + players.Item2.GetPlayerName() + " </i><color=#4062B7>" + GetHashtags_() + "</color>";

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Generates a string of captions to use for the post
    /// </summary>
    /// <returns>String of the caption</returns>
    string GetCaption_()
    {
        string output = "";

        var cIndex = UnityEngine.Random.Range(0, CaptionOptions.Length);
        output = CaptionOptions[cIndex];

        return output;
    }

    /// <summary>
    /// Generates a string of hashtags to add to the end of the post
    /// </summary>
    /// <returns>String containing hashtags</returns>
    string GetHashtags_()
    {
        string output = "\n";

        var numHashtags = UnityEngine.Random.Range(1, 3);
        for (int i = 0; i < numHashtags; i++)
        {
            var htIndex = UnityEngine.Random.Range(0, HashtagOptions.Length);
            output += "#" + HashtagOptions[htIndex] + " ";

            // do not go longer than 45 characters
            if (output.Length > 45)
                break;
        }

        return output;
    }

    /// <summary>
    /// Allocates a random number of followers to the taker of the selfie
    /// </summary>
    public void AllocatePoints()
    {
        Debug.Log(_owner);
        if (_owner == null) return;

        // generate number of followers and add to the player
        var r = UnityEngine.Random.Range(15, 25);
        _owner.AddFollower(false, r);

        TxtFollowers.text = "+" + r;

        StartCoroutine(ZoomUp_());
    }

    /// <summary>
    /// Increases the size of the points label
    /// </summary>
    private IEnumerator ZoomUp_()
    {
        PointsLabel.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-25, 25));

        while (PointsLabel.localScale.x < 0.8f)
        {
            PointsLabel.localScale += new Vector3(0.075f, 0.075f, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }
}

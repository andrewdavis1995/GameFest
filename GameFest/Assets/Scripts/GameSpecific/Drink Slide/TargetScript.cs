using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    public TextMesh TxtCountdown;
    TimeLimit _limit;

    const int ZONE_TIMEOUT = 30;
    const float MAXIMUM_DISTANCE = 3.5f;

    public void Activate()
    {
        gameObject.SetActive(true);
        _limit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _limit.Initialise(ZONE_TIMEOUT, OnTimeLimitTick, OnTimeUp);
        _limit.StartTimer();
    }

    /// <summary>
    /// Called each second
    /// </summary>
    /// <param name="seconds">How many seconds are left</param>
    void OnTimeLimitTick(int seconds)
    {
        // display countdown from 10 to 0
        TxtCountdown.text = seconds.ToString();
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        var drinks = FindObjectsOfType<DrinkObjectScript>();
        if (drinks.Count() > 0)
        {
            var ordered = drinks.OrderBy(d => Vector3.Distance(d.transform.position, transform.position)).ToList();
            var winner = ordered.First().GetPlayerIndex();
            var index = 0;

            List<DrinkObjectScript> drinksInZone = new List<DrinkObjectScript>();

            while (index < ordered.Count() && ordered[index].GetPlayerIndex() == winner)
            {
                var distance = Vector3.Distance(ordered[index].transform.position, transform.position);

                if (distance < MAXIMUM_DISTANCE)
                {
                    ordered[index].WinnerIndicator.SetActive(true);
                    ordered[index].WinnerIndicator.GetComponent<SpriteRenderer>().color = ColourFetcher.GetColour(winner);
                    DrinkSlideController.Instance.AddPointsWinningStone(winner);
                    ordered[index].Texts[0].text = DrinkSlideController.WINNING_STONE_POINTS.ToString();
                    drinksInZone.Add(ordered[index]);
                }

                index++;
            }

            // points for being in zone
            foreach (var d in drinks)
            {
                if (d.InZone() == this)
                {
                    d.InZoneIndicator.SetActive(true);
                    DrinkSlideController.Instance.AddPointsInZone(winner);
                    d.Texts[1].text = DrinkSlideController.IN_ZONE_POINTS.ToString();
                    if (!drinksInZone.Contains(d))
                        drinksInZone.Add(d);
                }
            }

            DrinkSlideController.Instance.RefreshScores();

            StartCoroutine(DestroyDrinks_(drinksInZone));
        }
    }

    private IEnumerator DestroyDrinks_(List<DrinkObjectScript> drinksInZone)
    {
        yield return new WaitForSeconds(1f);

        Debug.Log("fdfd");
        foreach (var v in drinksInZone)
        {
            StartCoroutine(v.FadeTexts());
        }

        yield return new WaitForSeconds(3f);

        foreach (var v in drinksInZone)
        {
            if (v != null && v.gameObject.activeInHierarchy)
                Destroy(v.gameObject);
        }

        gameObject.SetActive(false);
    }
}

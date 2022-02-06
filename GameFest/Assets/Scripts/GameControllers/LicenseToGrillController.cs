using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the LicenseToGrill game
/// </summary>
public class LicenseToGrillController : GenericController
{
    // constants
    const int POINTS_PER_BURGER = 100;
    const int TIP_VALUE = 10;
    const int GAME_TIMEOUT = 300;

    // static instance
    public static LicenseToGrillController Instance;

    // components
    public ChefScript[] Chefs;
    public BurgerResultScript[] BurgerResults;
    public Transform FoodPlateItemPrefab;
    public Transform FoodPlateBurgerPrefab;
    public Camera BenchCamera;
    public Camera BackgroundCamera;
    public ResultsPageScreen ResultsScreen;
    public TransitionFader EndFader;
    TimeLimit _countdownTimer;

    // players
    List<LicenseToGrillInputHandler> _players = new List<LicenseToGrillInputHandler>();

    // sprites
    public Sprite[] BreadBottoms;
    public Sprite[] BreadTop;
    public Sprite[] Burgers;
    public Sprite[] BurgerBottoms;
    public Sprite[] Sauces;
    public Sprite LettuceSlice;
    public Sprite TomatoSlices;
    public Sprite PickleSlices;
    public Text TxtCountdown;
    public GameObject CountdownDisplay;
    public Sprite[] StarImages;
    public Sprite[] SauceImages;

    // status variables
    bool _ended = false;

    /// <summary>
    /// Called once on startup
    /// </summary>
    private void Start()
    {
        Instance = this;

        SpawnPlayers_();

        // initialise timer
        _countdownTimer = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _countdownTimer.Initialise(GAME_TIMEOUT, CountdownTick_, CountdownDone_, 1f);

        // make full screen if one player
        if (_players.Count == 1)
        {
            Chefs[0].ChefCamera.rect = new Rect(new Vector2(0, 0), new Vector2(1, 1));
            Chefs[0].ControlsDisplayRect.localScale = new Vector3(1, 1, 1);
            Chefs[0].ErrorDisplayRect.localScale = new Vector3(1, 1, 1);
            Chefs[0].ConfirmDisplayRect.localScale = new Vector3(1, 1, 1);
            Chefs[0].OrdersDisplayRect.localScale = new Vector3(1, 1, 1);
        }

        // fade in
        EndFader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        EndFader.StartFade(1, 0, () => { PauseGameHandler.Instance.Pause(true, StartGame_); });

        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers, QuitGame_);
    }

    /// <summary>
    /// Callback for when the player quits
    /// </summary>
    private void QuitGame_()
    {
        _ended = true;
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Can't pause once we get to the results section
    /// </summary>
    /// <returns>Whether the game can be paused at the current stage</returns>
    public override bool CanPause()
    {
        return !_ended;
    }

    /// <summary>
    /// Callback for each second of the countdown
    /// </summary>
    /// <param name="time">How much time is left</param>
    private void CountdownTick_(int time)
    {
        CountdownDisplay.SetActive(time <= 10);
        TxtCountdown.text = time <= 10 ? time.ToString() : "";
    }

    /// <summary>
    /// Callback for when countdown expires
    /// </summary>
    private void CountdownDone_()
    {
        EndGame_();
    }

    /// <summary>
    /// End the game and show results
    /// </summary>
    private void EndGame_()
    {
        _ended = true;
        CountdownDisplay.SetActive(false);

        // stop all players
        foreach (var player in _players)
        {
            player.Finished();
            player.DisableCamera();

            // add points to player
            var totalPoints = 0;
            totalPoints += player.GetWastePoints();

            int index = 0;

            // get customers for this player
            var customers = player.GetCustomers();
            foreach (var customer in customers)
            {
                // stop once we reach a customer who has not been served
                if (customer.GetActual() == null) break;

                // get complaints from customer and the number of points lost
                var complaints = BurgerValidation.ValidateBurger(customer);
                var pointsLost = complaints.Sum(e => e.PointsLost());

                // add points if there are any remaining
                var points = POINTS_PER_BURGER - pointsLost;
                if (points > 0)
                    totalPoints += points;

                var tip = 0;
                // if burger was perfect, add a tip
                if (complaints.Count == 0)
                    tip += TIP_VALUE * 2;

                // give small tip if nearly right
                if (points > 80)
                    tip += TIP_VALUE;

                totalPoints += tip;

                // show result
                BurgerResults[index].Initialise(complaints, tip, points, player.GetPlayerIndex(), customer.GetCustomerName());
                BurgerResults[index].gameObject.SetActive(true);
                index++;
            }

            // hide unused elements
            for(; index < BurgerResults.Count(); index++)
            {
                BurgerResults[index].gameObject.SetActive(false);
            }

            // don't allow points under 0
            if (totalPoints < 0) totalPoints = 0;

            player.AddPoints(totalPoints);
        }

        StartCoroutine(LoopThroughResults_());
    }

    /// <summary>
    /// Move through each burger
    /// </summary>
    private IEnumerator LoopThroughResults_()
    {
        var index = 0;

        BenchCamera.gameObject.SetActive(true);
        BackgroundCamera.gameObject.SetActive(false);

        // continue until found an unused burger
        while (index < BurgerResults.Length && BurgerResults[index].isActiveAndEnabled)
        {
            var targetPosition = BurgerResults[index].transform.position.x;

            // move into position
            while (BenchCamera.transform.position.x < targetPosition)
            {
                BenchCamera.transform.Translate(new Vector3(8f * Time.deltaTime, 0, 0));
                yield return new WaitForSeconds(0.025f);
            }

            yield return new WaitForSeconds(2.5f);
            index++;
        }

        yield return new WaitForSeconds(2f);
        StartCoroutine(Complete_());
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(LicenseToGrillInputHandler));
            _players.Add(player.GetComponent<LicenseToGrillInputHandler>());

            // create the "visual" player at the start point
            player.Spawn(null, Vector3.zero);
        }
    }

    /// <summary>
    /// Start the game
    /// </summary>
    void StartGame_()
    {
        // start all players
        foreach (var player in _players)
        {
            player.Activate();
        }

        // start timer
        _countdownTimer.StartTimer();
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.Where(p => p.GetPoints() > 0).OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 180, 70, 25 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            if (ordered[i].GetPoints() > 0)
            {
                ordered[i].AddPoints(winnerPoints[i]);
                ordered[i].SetBonusPoints(winnerPoints[i]);
            }
        }

        // set the winner
        ordered.FirstOrDefault()?.Winner();
    }

    /// <summary>
    /// Completes the game and return to menu
    /// </summary>
    IEnumerator Complete_()
    {
        AssignBonusPoints_();

        yield return new WaitForSeconds(3f);

        ResultsScreen.Setup();

        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        ResultsScreen.SetPlayers(genericPlayers);

        ScoreStoreHandler.StoreResults(Scene.LicenseToGrill, genericPlayers);

        yield return new WaitForSeconds(4 + genericPlayers.Length);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        PlayerManagerScript.Instance.CentralScene();
    }
}

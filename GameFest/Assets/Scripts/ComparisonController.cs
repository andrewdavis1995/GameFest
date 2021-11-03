using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Class for displaying player stats
/// </summary>
public class ComparisonController : MonoBehaviour
{
    bool _playerComparison = false;

    public HighScoreElement[] HighScores;

    public GameObject ComparisonWindow;
    public GameObject PlayerWindow;

    public PlayerStatsDisplayScript[] PlayerDisplays;
    List<PlayerStatsDisplayScript> _playerDisplaysInUse = new List<PlayerStatsDisplayScript>();

    // individual
    public PieChart WinPercentage;
    public Text TxtMaxScore;
    public Text TxtAverageScore;
    public GameObject NoDataLabelAtAll;
    public GameObject NoDataLabelPlayer;
    public GameObject HighScoreObject;
    public PreviousResultPlayerScript[] PreviousResults;

    // comparison
    public ComparisonGraphScript WinBreakdown;


    public Sprite[] GameBackgrounds;
    public Sprite[] GameLogos;

    public Image BackgroundImage;
    public Image LogoImage;

    Scene[] _availableScenes = { Scene.PunchlineBling, Scene.ShopDrop, Scene.XTinguish, Scene.MarshLand, Scene.BeachBowles, Scene.MineGames };

    int _gameIndex = 0;
    int _playerIndex = 0;
    List<StatContent> _stats = new List<StatContent>();

    public static ComparisonController Instance;

    /// <summary>
    /// Called once at startup
    /// </summary>
    private void Start()
    {
        Instance = this;
        _stats = ScoreStoreHandler.LoadScores();
        SpawnPlayers_();
        ShowActiveGame_();
    }

    /// <summary>
    /// Spawn players in menu
    /// </summary>
    private void SpawnPlayers_()
    {
        int index = 0;

        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(StatsInputHandler));

            // needed to set up player index
            player.Spawn(null, Vector3.zero);

            PlayerDisplays[index].SetData(player);
            _playerDisplaysInUse.Add(PlayerDisplays[index]);

            index++;
        }

        // hide unused controls
        for (; index < 4; index++)
            PlayerDisplays[index].gameObject.SetActive(false);

        // player comparison only allowed if there are multiple players
        if (PlayerManagerScript.Instance.GetPlayerCount() > 1)
            _playerDisplaysInUse.Add(PlayerDisplays.Last());
        else
            PlayerDisplays.Last().gameObject.SetActive(false);
    }

    /// <summary>
    /// Move to the next game to the right
    /// </summary>
    public void GameRight()
    {
        _gameIndex++;

        // loop around
        if (_gameIndex >= _availableScenes.Length)
        {
            _gameIndex = 0;
        }

        ShowActiveGame_();
    }

    /// <summary>
    /// Move to the next game to the left
    /// </summary>
    public void GameLeft()
    {
        _gameIndex--;

        // loop around
        if (_gameIndex < 0)
        {
            _gameIndex = _availableScenes.Length - 1;
        }

        ShowActiveGame_();
    }

    /// <summary>
    /// Move down to the next player
    /// </summary>
    public void PlayerDown()
    {
        // don't go beyond bottom
        if (_playerIndex < _playerDisplaysInUse.Count - 1)
        {
            ResetPlayerDisplays_();

            _playerIndex++;

            _playerComparison = _playerIndex == _playerDisplaysInUse.Count - 1;

            // update UI display
            ShowPlayer_();
        }
    }

    /// <summary>
    /// Move up to the next player
    /// </summary>
    public void PlayerUp()
    {
        // don't go beyond bottom
        if (_playerIndex > 0)
        {
            ResetPlayerDisplays_();

            _playerIndex--;

            _playerComparison = false;

            // update UI display
            ShowPlayer_();
        }
    }

    /// <summary>
    /// Updates the current player status
    /// </summary>
    void ShowPlayer_()
    {
        _playerDisplaysInUse[_playerIndex].Selected();
        ShowActiveGame_();
    }

    /// <summary>
    /// Clears all player displays (deselect)
    /// </summary>
    void ResetPlayerDisplays_()
    {
        foreach (var display in _playerDisplaysInUse)
        {
            display.Deselected();
        }
    }

    /// <summary>
    /// Returns to the menu
    /// </summary>
    public void ReturnToMenu()
    {
        PlayerManagerScript.Instance.CentralScene();
    }

    /// <summary>
    /// Loads the data fo the current game
    /// </summary>
    void ShowActiveGame_()
    {
        // update game images
        BackgroundImage.sprite = GameBackgrounds[_gameIndex];
        LogoImage.sprite = GameLogos[_gameIndex];

        // get stats for this game
        var stats = _stats.Where(t => t.GetScene() == _availableScenes[_gameIndex]).ToList();

        // group stats by session
        var statsGrouped = stats.GroupBy(s => s.GetDateTime()).ToList();

        // update displays
        HighScoreObject.SetActive(stats.Count > 0);
        NoDataLabelAtAll.SetActive(stats.Count == 0);
        NoDataLabelPlayer.SetActive(false);
        ShowHighscores_(stats.OrderByDescending(s => s.GetScore()).ToList());

        // check which mode we are on
        if (!_playerComparison)
        {
            ComparisonWindow.SetActive(false);

            // check there are stats to show
            if (stats.Count > 0)
            {
                var playerContent = stats.Any(t => t.GetPlayerId() == _playerDisplaysInUse[_playerIndex].PlayerID());
                NoDataLabelPlayer.SetActive(!playerContent);

                // check that there is data to to display
                if (playerContent)
                {
                    PlayerWindow.SetActive(true);

                    var playerSessions = statsGrouped.Where(g => g.Any(p => p.GetPlayerId() == _playerDisplaysInUse[_playerIndex].PlayerID()));
                    DisplayWinRate_(playerSessions.ToList());
                    DisplayScoreData_(stats);
                    DisplayPreviousMatches_(playerSessions.ToList());
                }
                else
                {
                    PlayerWindow.SetActive(false);
                }
            }
            else
            {
                PlayerWindow.SetActive(false);
            }
        }
        else
        {
            var matchesForPlayers = GetMatchesForPlayers_(statsGrouped);
            
            Debug.Log(matchesForPlayers.Count + " matches found for these players");
        
            // TODO: Comparison data
            ComparisonWindow.SetActive(true);
            PlayerWindow.SetActive(false);

            // TODO: TEMP
            // update charts
            WinBreakdown.SetValues(new float[] { 5, 10, 2, 1 }, "");
        }
    }
    
    /// <summary>
    /// Works out which matches include all the players
    /// </summary>
    private List<IGrouping<DateTime, StatContent>> GetMatchesForPlayers_(List<IGrouping<DateTime, StatContent>> allMatches)
    {
        var matches = new List<IGrouping<DateTime, StatContent>>();
    
        foreach(var match in allMatches)
        {
            bool correctPlayerCount = (match.Count == _playerDisplaysInUse.Count - 1);
            
            // check that the correct number of players 
            if(correctPlayerCount)
            {
                var allPlayersMatch = true;
                foreach(var player in match)
                {
                    // check each player was involved in the previous game
                    if(!_playerDisplaysInUse.Any(d => d.PlayerID() == player)
                    {
                        allPlayersMatch = false;
                        break;
                    }
                }
            
                // add if the correct players are involved
                if(allPlayersMatch)
                {
                    matches.Add(match);
                }
            }
        }
    
        return matches;
    }

    /// <summary>
    /// Displays scores from previous matches
    /// </summary>
    /// <param name="stats">Score information</param>
    private void DisplayPreviousMatches_(List<IGrouping<DateTime, StatContent>> stats)
    {
        var ph = new ProfileHandler();
        ph.Initialise();

        // order by date
        var sorted = stats.OrderByDescending(s => s.Key).ToList();

        var index = 0;

        // loop though controls - up to 5
        for(; index < PreviousResults.Count() && index < sorted.Count(); index++)
        {
            // find active player
            var activePlayerId = _playerDisplaysInUse[_playerIndex].PlayerID();
            var player = sorted[index].Where(s => s.GetPlayerId() == activePlayerId).FirstOrDefault();

            // get other players who played
            var otherPlayers = sorted[index].Where(s => s.GetPlayerId() != activePlayerId);

            // work out the scores
            var plScore = player?.GetScore();
            var maxScore = sorted[index].Max(s => s.GetScore());

            var otherCharacters = new List<int>();

            // get a list of the characters that were used by other players
            foreach(var p in otherPlayers)
            {
                var profile = ph.GetProfile(p.GetPlayerId());
                if (profile != null)
                    otherCharacters.Add(profile.GetCharacterIndex());
            }

            // initialise controls
            PreviousResults[index].gameObject.SetActive(true);
            PreviousResults[index].SetData(sorted[index].Key, (int)plScore, otherCharacters, plScore == maxScore);
        }

        // hind unused controls
        for (; index < PreviousResults.Count(); index++)
        {
            PreviousResults[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Displays score info
    /// </summary>
    /// <param name="stats">Score information</param>
    private void DisplayScoreData_(List<StatContent> stats)
    {
        var playerScores = stats.Where(p => p.GetPlayerId() == _playerDisplaysInUse[_playerIndex].PlayerID());

        // calculate maximum and average score
        var topScore = playerScores.Max(p => p.GetScore());
        var avScore = (int)(playerScores.Average(p => p.GetScore()));

        TxtMaxScore.text = topScore.ToString();
        TxtAverageScore.text = avScore.ToString();
    }

    /// <summary>
    /// Displays the win rate that the player has
    /// </summary>
    /// <param name="playerSessions"></param>
    private void DisplayWinRate_(List<IGrouping<DateTime, StatContent>> playerSessions)
    {
        var multiplePlayers = playerSessions.Where(p => p.Count() > 1).ToList();

        var numberOfGames = multiplePlayers.Count;
        var wins = 0;

        foreach (var game in multiplePlayers)
        {
            var playerScore = game.Where(p => p.GetPlayerId() == _playerDisplaysInUse[_playerIndex].PlayerID()).FirstOrDefault()?.GetScore();
            if ((playerScore > 0) && (game.Max(m => m.GetScore()) == playerScore))
                wins++;
        }

        // display win %
        var winPercent = (float)wins / numberOfGames;
        WinPercentage.SetValues(new float[] { winPercent }, (int)(winPercent * 100) + "%");
    }

    /// <summary>
    /// Displays high score data
    /// </summary>
    /// <param name="stats">The data to show</param>
    private void ShowHighscores_(List<StatContent> stats)
    {
        var ph = new ProfileHandler();
        ph.Initialise();

        var index = 0;
        for (; (index < stats.Count && index < HighScores.Length); index++)
        {
            HighScores[index].gameObject.SetActive(true);
            var profile = ph.GetProfile(stats[index].GetPlayerId());
            if (profile != null)
                HighScores[index].SetPlayerData(profile.GetProfileName(), stats[index].GetScore(), profile.GetCharacterIndex());
        }

        for (; index < HighScores.Length; index++)
        {
            HighScores[index].gameObject.SetActive(false);
        }
    }
}

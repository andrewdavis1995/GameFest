using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

/// <summary>
/// Class for storing score information
/// </summary>
public class StatContent
{
    Scene _scene;
    Guid _playerId;
    int _score;
    DateTime _date;
    string _additional;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="content">Content to parse</param>
    public StatContent(string content)
    {
        var split = content.Split('@');
        _scene = (Scene)int.Parse(split[0]);
        _playerId = Guid.Parse(split[1]);
        _score = int.Parse(split[2]);
        _date = DateTime.ParseExact(split[3], "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        if (split.Length > 4)
            _additional = split[4];
    }

    /// <summary>
    /// Returns the game that the score relates to
    /// </summary>
    /// <returns>The game</returns>
    public Scene GetScene()
    {
        return _scene;
    }

    /// <summary>
    /// Returns the player ID that the score relates to
    /// </summary>
    /// <returns>The player ID</returns>
    public Guid GetPlayerId()
    {
        return _playerId;
    }

    /// <summary>
    /// Returns the points scored
    /// </summary>
    /// <returns>The score</returns>
    public int GetScore()
    {
        return _score;
    }

    /// <summary>
    /// Returns the date and time of the score
    /// </summary>
    /// <returns>The date</returns>
    public DateTime GetDateTime()
    {
        return _date;
    }
}

/// <summary>
/// Class for storing player information
/// </summary>
public class ScoreStoreHandler
{
    const string SCORES_FILE_NAME = "ScoreHistory.txt";

    /// <summary>
    /// Stores the results for each player
    /// </summary>
    /// <param name="scene">The game that was played</param>
    /// <param name="players">The player data</param>
    public static void StoreResults(Scene scene, GenericInputHandler[] players)
    {
        // open the file for writing
        var sw = new StreamWriter(SCORES_FILE_NAME, true);

        // write each profile to file
        foreach (var pl in players)
        {
            var str = (int)scene + "@" + pl.GetProfileID() + "@" + pl.GetPoints() + "@" + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            sw.WriteLine(str);
        }

        // make sure to close the file
        sw.Close();
    }

    /// <summary>
    /// Load the content of the scores file
    /// </summary>
    /// <returns></returns>
    public static List<StatContent> LoadScores()
    {
        var list = new List<StatContent>();

        var lines = File.ReadAllLines(SCORES_FILE_NAME);

        // process each line
        foreach(var line in lines)
        {
            list.Add(new StatContent(line));
        }

        return list;
    }
}
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Class for storing player information
/// </summary>
public class ScoreStoreHandler
{
    const string SCORES_FILE_NAME = "ScoreHistory.txt";

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
}
using System.Collections.Generic;
using UnityEngine;

public class NameHandler
{

    private List<string> _firstNames = new List<string>();
    private List<string> _surnames = new List<string>();
    public NameHandler()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load("TextFiles/FirstNames");
        string txt = mytxtData.text;
        var split = txt.Split('\n');
        foreach (var line in split)
        {
            _firstNames.Add(line);
        }

        mytxtData = (TextAsset)Resources.Load("TextFiles/SecondNames");
        txt = mytxtData.text;
        split = txt.Split('\n');
        foreach (var line in split)
        {
            _surnames.Add(line);
        }
    }

    string GetFirstName()
    {
        var possibilities = new List<string>();
        possibilities = _firstNames;

        var index = Random.Range(0, possibilities.Count);

        return possibilities[index];
    }

    string GetSurname()
    {
        var index = Random.Range(0, _surnames.Count);
        return _surnames[index];
    }

    public string GetName()
    {
        return GetFirstName() + " " + GetSurname();
    }
}


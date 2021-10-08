using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class for storing player information
/// </summary>
public class PlayerProfile : IEquatable<PlayerProfile>
{
    const uint INDEX_GUID = 0;
    const uint INDEX_NAME = 1;
    const uint INDEX_CHARACTER = 2;

    Guid _guid;
    string _playerName;
    int _characterIndex;

    /// <summary>
    /// Constructor for making a new player
    /// </summary>
    public PlayerProfile()
    {
        _guid = Guid.NewGuid();
    }

    /// <summary>
    /// Constructor for loading existing player details
    /// </summary>
    /// <param name="guid">ID of the profile</param>
    /// <param name="plName">Name of the profile</param>
    /// <param name="charIndex">Index of the character index in use</param>
    public PlayerProfile(string[] details)
    {
        try { _guid = Guid.Parse(details[INDEX_GUID]); } catch (Exception) { Debug.Log("Could not parse Guid"); }
        _playerName = details[INDEX_NAME];
        try { _characterIndex = int.Parse(details[INDEX_CHARACTER]); } catch (Exception) { Debug.Log("Could not parse character index"); }
    }

    /// <summary>
    /// Update existing player details
    /// </summary>
    /// <param name="plName">Name of the profile</param>
    /// <param name="charIndex">Index of the character index in use</param>
    public void UpdateDetails(string plName, int charIndex)
    {
        _playerName = plName;
        _characterIndex = charIndex;
    }

    /// <summary>
    /// Returns the ID of the profile
    /// </summary>
    /// <returns>Profile ID</returns>
    public Guid GetGuid()
    {
        return _guid;
    }

    /// <summary>
    /// Returns the name of the profile
    /// </summary>
    /// <returns>Profile name</returns>
    public string GetProfileName()
    {
        return _playerName;
    }

    /// <summary>
    /// Returns the index of the character in use
    /// </summary>
    /// <returns>Character index</returns>
    public int GetCharacterIndex()
    {
        return _characterIndex;
    }

    /// <summary>
    /// Overrides the ToString() behaviour of this object
    /// </summary>
    /// <returns>The object represented as a string</returns>
    public override string ToString()
    {
        return _guid.ToString() + "@" + _playerName + "@" + _characterIndex;
    }

    /// <summary>
    /// Logic for comparing this object with other objects - just based on GUID
    /// </summary>
    /// <param name="other">The item to compare against</param>
    /// <returns>Whether it is a match</returns>
    public bool Equals(PlayerProfile other)
    {
        return other.GetGuid().Equals(_guid);
    }
}

/// <summary>
/// Class for reading and writing profile information to/from file
/// </summary>
internal class ProfileHandler
{
    const string PROFILE_FILE_NAME = "Profiles.txt";
    const uint CORRECT_NUMBER_OF_FIELDS = 3;

    List<PlayerProfile> _profiles = new List<PlayerProfile>();

    /// <summary>
    /// Gets a list of all stored profiles
    /// </summary>
    /// <returns>List of all profiles</returns>
    public List<PlayerProfile> GetProfileList()
    {
        return _profiles;
    }

    /// <summary>
    /// Gets a list of all stored profiles
    /// </summary>
    /// <returns>List of all profiles</returns>
    public void Initialise()
    {
        _profiles = new List<PlayerProfile>();
        _profiles.Add(null);

        // check that file exists
        if (File.Exists(PROFILE_FILE_NAME))
        {
            // read each line
            var lines = File.ReadAllLines(PROFILE_FILE_NAME);
            foreach (var line in lines)
            {
                var split = line.Split('@');

                // check that line has correct number of components
                if (split.Length == CORRECT_NUMBER_OF_FIELDS)
                {
                    var profile = new PlayerProfile(split);
                    _profiles.Add(profile);
                }
                else
                {
                    Debug.Log("Invalid profile found");
                }
            }
        }
    }

    /// <summary>
    /// Updates a profile with the latest details
    /// </summary>
    /// <param name="updated">The updated details</param>
    public void UpdateProfile(PlayerProfile updated)
    {
        // loop through all stored profiles to find the one we are looking for
        foreach (var profile in _profiles)
        {
            if (profile == updated)
            {
                // update the stored details
                profile.UpdateDetails(updated.GetProfileName(), updated.GetCharacterIndex());
                break;
            }
        }

        // save list
        SaveProfileList();
    }

    /// <summary>
    /// Adds a new profile to the list
    /// </summary>
    /// <param name="profile">The profile to add</param>
    public void AddProfile(PlayerProfile profile)
    {
        // add the profile
        _profiles.Add(profile);

        // save list
        SaveProfileList();
    }

    /// <summary>
    /// Removes a new profile from the list
    /// </summary>
    /// <param name="guid">The profile to remove</param>
    public void RemoveProfile(Guid guid)
    {
        for(int i = 0; i < _profiles.Count; i++)
        {
            // find match, and delete it
            if(_profiles[i] != null && _profiles[i].GetGuid() == guid)
            {
                _profiles.RemoveAt(i);
                break;
            }
        }

        // save list
        SaveProfileList();
    }

    /// <summary>
    /// Save all profiles to file
    /// </summary>
    public void SaveProfileList()
    {
        // open the file for writing
        var sw = new StreamWriter(PROFILE_FILE_NAME, false);

        // write each profile to file
        foreach (var profile in _profiles)
        {
            if (profile != null)
                sw.WriteLine(profile.ToString());
        }

        // make sure to close the file
        sw.Close();
    }
}
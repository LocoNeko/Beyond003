using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public enum Hemisphere : int { North, South }
    public class Place
    {
        public string Name { get; protected set; }
        public Hemisphere hemisphere { get; protected set; }
        public Gametime gametime { get; protected set; }
        public int Length { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int MinLevel { get; protected set; }
        public List<BeyondGroup> beyondGroups { get; protected set; }

        public Place(string s = "A test place", int l = 1000, int w = 1000, int h = 200, int ml = -20)
        {
            Name = s;
            hemisphere = Hemisphere.North;
            gametime = new Gametime();
            Length = l;
            Width = w;
            Height = h;
            MinLevel = ml;
            beyondGroups = new List<BeyondGroup>();
            Debug.Log("New place created");
        }

        public void update(float deltatime)
        {
            gametime.Update(deltatime);
        }

        public Season GetSeason()
        {
            return gametime.GetSeason(hemisphere);
        }

    }
}
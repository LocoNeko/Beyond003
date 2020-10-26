using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public enum Hemisphere : int { North, South }
    public class Place
    {
        public string name { get; protected set; }
        public Hemisphere hemisphere { get; protected set; }
        public Gametime gametime { get; protected set; }
        public int Length { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int LowestY { get; protected set; }
        public List<BeyondGroup> beyondGroups { get; protected set; }


        public Place(string s = "A test place", int l = 1000, int w = 1000, int h = 200, int ly = -20)
        {
            name = s;
            hemisphere = Hemisphere.North;
            gametime = new Gametime();
            Length = l;
            Width = w;
            Height = h;
            LowestY = ly;
            beyondGroups = new List<BeyondGroup>();
            Debug.Log(string.Format("New place '{0}' created" , name));
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
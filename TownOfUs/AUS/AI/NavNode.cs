using UnityEngine;

namespace AUPathfinder
{
    [Serializable]
    public class NavNode
    {
        public int ID;
        public float X;
        public float Y;

        public List<int> Connections = new();

        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
        }

        public NavNode(int id, float x, float y)
        {
            ID = id;
            X = x;
            Y = y;
        }
    }
}
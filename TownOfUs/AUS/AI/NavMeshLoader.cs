using UnityEngine;

namespace AUPathfinder
{
    public class NavMeshLoader
    {
        public static NavMeshLoader GlobalNavMesh;
        public Dictionary<int, NavNode> Nodes = new();

        public void Load()
        {
            //string path = Application.persistentDataPath + "/SkeldNodes.txt";
            string path = PathRecorder.filePath;
            if (!File.Exists(path))
            {
                AUSPlugin.DebugLogMessage("No navigation file found!", AUSPlugin.MsgType.Error);
                return;
            }

            string[] lines = File.ReadAllLines(path);

            // First create nodes
            foreach (string line in lines)
            {
                string[] data = line.Split('|');
                int id = int.Parse(data[0]);
                float x = float.Parse(data[1]);
                float y = float.Parse(data[2]);

                Nodes[id] = new NavNode(id, x, y);
            }

            // Then add connections
            foreach (string line in lines)
            {
                string[] data = line.Split('|');
                int id = int.Parse(data[0]);

                if (data.Length < 4)
                    continue;

                string[] connections = data[3].Split(',');
                foreach (string connection in connections)
                {
                    if (int.TryParse(connection, out int nodeID))
                    {
                        Nodes[id].Connections.Add(nodeID);
                    }
                }
            }

            AUSPlugin.DebugLogMessage($"Loaded {Nodes.Count} nodes");
        }

        public void ConnectNearbyNodes(float maxDistance)
        {
            foreach (NavNode nodeA in Nodes.Values)
            {
                foreach (NavNode nodeB in Nodes.Values)
                {
                    if (nodeA.ID == nodeB.ID)
                        continue;


                    float distance = Vector2.Distance(
                        nodeA.Position,
                        nodeB.Position
                    );


                    if (distance <= maxDistance)
                    {
                        if (!nodeA.Connections.Contains(nodeB.ID))
                        {
                            nodeA.Connections.Add(nodeB.ID);
                        }
                    }
                }
            }


            AUSPlugin.DebugLogMessage(
                "Connected nearby nodes"
            );
        }
    }
}
using System.Collections;
using UnityEngine;

namespace AUPathfinder
{
    public static class PathRecorder
    {

        public static bool Recording;
        private static List<NavNode> Nodes = new();
        private static int nextID = 0;

        // Distance before creating a new node
        public static float NodeDistance = 0.5f;

        public static IEnumerator Update()
        {
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    StartRecording();
                }

                if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    StopRecording();
                    break;
                }

                if (Recording)
                {
                    RecordPosition();
                }

                yield return null;
            }
        }

        public static void StartRecording()
        {
            AUSPlugin.DebugLogMessage("Started recording map");

            Nodes.Clear();
            nextID = 0;
            Recording = true;
        }

        public static void RecordPosition()
        {
            Vector2 currentPosition = PlayerControl.LocalPlayer.transform.position;

            // First node
            if (Nodes.Count == 0)
            {
                AddNode(currentPosition);
                return;
            }

            NavNode last = Nodes[^1];

            // Only create node if moved far enough
            if (Vector2.Distance(last.Position, currentPosition) >= NodeDistance)
            {
                AddNode(currentPosition);
            }
        }

        public static void AddNode(Vector2 position)
        {
            NavNode node = new NavNode(nextID, position.x, position.y);

            // Connect to previous node
            if (Nodes.Count > 0)
            {
                NavNode previous = Nodes[^1];
                previous.Connections.Add(node.ID);
                node.Connections.Add( previous.ID);
            }

            Nodes.Add(node);
            nextID++;

            AUSPlugin.DebugLogMessage($"Created node {node.ID}: {position}");
        }

        public static void StopRecording()
        {
            Recording = false;
            SaveNodes();
            AUSPlugin.DebugLogMessage($"Saved {Nodes.Count} nodes");
        }

        public static string filePath = "SkeldNodes.txt";
        public static void SaveNodes()
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (NavNode node in Nodes)
                {
                    string connections = string.Join(",", node.Connections);

                    writer.WriteLine(
                        $"{node.ID}|{node.X}|{node.Y}|{connections}"
                    );
                }
            }

            AUSPlugin.DebugLogMessage(
                "Saved navigation file: " + filePath
            );
        }
    }
}
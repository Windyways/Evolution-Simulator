using Reactor.Utilities.Attributes;
using System.Collections;
using UnityEngine;
using static AmongUsSalem.AI.Bot;

namespace AUPathfinder
{
    [RegisterInIl2Cpp]
    public class BotNavigator(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public PlayerControl player;
        private NavMeshLoader loader;
        public Bot bot;

        void Start()
        {
            loader = NavMeshLoader.GlobalNavMesh;// new NavMeshLoader();
           // loader.Load();
            //loader.ConnectNearbyNodes(1.25f);
        }

        public void MoveTo(Vector2 target, ActionType2 type)
        {
            int start = GetClosestNode(player.transform.position);
            int end = GetClosestNode(target);
            List<NavNode> path = CreateSimplePath(start, end);

            Coroutines.Start(FollowPath(path, type));
        }

        public int GetClosestNode(Vector2 position)
        {
            float closest = Mathf.Infinity;
            int result = -1;

            foreach (NavNode node in loader.Nodes.Values)
            {
                float distance = Vector2.Distance(position, node.Position);
                if (distance < closest)
                {
                    closest = distance;
                    result = node.ID;
                }
            }

            return result;
        }

        public List<NavNode> CreateSimplePath(int start, int end)
        {
            Queue<int> queue = new();
            Dictionary<int, int> previous = new();
            HashSet<int> visited = new();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                if (current == end)
                    break;

                NavNode node = loader.Nodes[current];
                foreach (int neighbor in node.Connections)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);

                        previous[neighbor] = current;

                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Reconstruct path
            List<NavNode> path = new();
            int currentNode = end;

            while (currentNode != start)
            {
                path.Add(loader.Nodes[currentNode]);
                if (!previous.ContainsKey(currentNode))
                {
                    AUSPlugin.DebugLogMessage("No path found!");
                    return new List<NavNode>();
                }

                currentNode = previous[currentNode];
            }

            path.Add(loader.Nodes[start]);
            path.Reverse();
            return path;
        }

        public IEnumerator FollowPath(List<NavNode> path, ActionType2 type)
        {
            bool continueMove = true;
            AUSPlugin.DebugLogMessage($"Starting FollowPath with {path.Count} nodes");
            bot.nodes = path.Count;
            bot.isMoving = true;
            bot.currentAction = type;

            foreach (NavNode node in path)
            {
                if ((type == ActionType2.Move && bot.Actions()) || bot.endTurn || MeetingHud.Instance)
                {
                    continueMove = false;
                    yield break;
                }

                yield return MoveSmoothly(node.Position);
            }

            if (!continueMove)
                yield break;

            bot.currentAction = ActionType2.None;
            bot.isMoving = false;

            AUSPlugin.DebugLogMessage("Finished FollowPath");

            if (type == ActionType2.Move) bot.PerformAction(false);
            if (type == ActionType2.Ability) bot.PerformAction(true);
            //if (type == ActionType2.Report) player.CmdReportDeadBody(bot.target.Data);
        }

        public static float speed = 7;
        public IEnumerator MoveSmoothly(Vector2 target)
        {
            Vector2 start = player.transform.position;

            float distance = Vector2.Distance(start, target);

            float duration = distance / speed;

            float timer = 0f;


            while (timer < duration)
            {
                timer += Time.deltaTime;

                float percent = timer / duration;

                player.transform.position =
                    Vector2.Lerp(start, target, percent);

                yield return null;
            }


            player.transform.position = target;
        }
    }
}
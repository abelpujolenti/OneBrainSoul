using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Entities.AI.Combat;

namespace ECS.Systems.AI.Navigation
{
    public class UpdateAgentDestinationSystem
    {
        public void UpdateAgentDestination(AStarPath aStarPath)
        {
            List<Node> newPath = AStarPathFindingAlgorithm.FindPath(aStarPath.GetNavMeshGraph(), aStarPath.origin, 
                aStarPath.destination);
            
            aStarPath.GetNavMeshGraph().ResetNodesImportantInfo();

            if (newPath.Count == 0)
            {
                return;
            }

            aStarPath.path = newPath;
            
            aStarPath.path.RemoveAt(0);

            //aStarPath.path = SmoothPath(newPath);

            if (aStarPath.path.Count < 2)
            {
                return;
            }
            aStarPath.path.RemoveAt(aStarPath.path.Count - 2);

            if (aStarPath.path.Count < 2)
            {
                return;
            }
            aStarPath.path.RemoveAt(aStarPath.path.Count - 2);
        }

        private List<Node> SmoothPath(List<Node> originalPath)
        {
            //TODO
            List<Node> smoothedPath = new List<Node>();

            return smoothedPath;
        }
    }
}

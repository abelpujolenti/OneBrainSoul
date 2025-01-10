using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using ECS.Components.AI.Navigation;
using ECS.Entities.AI.Navigation;
using Managers;
using UnityEngine;

namespace DEBUG
{
    public class DEBUG_NavigationAgent : NavMeshAgentEntity
    {
        [SerializeField] private Transform _destination;

        public List<Node> originalPath;
        public List<Node> optimizedPath;
        public List<List<Node>> allClosestNodes = new List<List<Node>>();
        private List<(Vector3, Vector3, float, float)> cubes = new List<(Vector3, Vector3, float, float)>();

        public void SetOriginalPath(List<Node> path)
        {
            originalPath = path;
            allClosestNodes.Clear();
        }

        public void SetClosestNodes(List<Node> closestNodes)
        {
            allClosestNodes.Add(closestNodes);
        }

        public void SetCubes(Vector3 start, Vector3 direction, float length, float width)
        {
            cubes.Add((start, direction, length, width));
        }

        void Start()
        {
            Setup();
            
            ECSNavigationManager.Instance.AddNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent(), 
                GetComponent<CapsuleCollider>().radius);
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), new TransformComponent(_destination));
        }
        
        private void OnDrawGizmos()
        {
            if (ECSNavigationManager.Instance == null)
            {
                return;
            }
            
            /*Vector3 position = transform.position;

            Gizmos.color = Color.green;

            foreach (DirectionWeights directionAndWeight in _raysDirectionAndWeights)
            {
                Gizmos.DrawRay(position, directionAndWeight.direction * _raysDistance);
            }*/

            Vector3[] corners = GetCorners(originalPath);

            if (corners.Length == 0)
            {
                return;
            }

            int height = 0;
            
            Gizmos.color = Color.blue;

            height++;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawSphere(Up(corners[i], height), 0.2f);
                
                Gizmos.DrawLine(Up(corners[i], height), Up(corners[i + 1], 1));
            }

            corners = GetCorners(optimizedPath);
            
            Gizmos.color = Color.magenta;

            height++;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawSphere(Up(corners[i], height), 0.2f);
                
                Gizmos.DrawLine(Up(corners[i], height), Up(corners[i + 1], 2));
            }
            
            Gizmos.color = Color.green;

            int counter = 0;

            foreach (List<Node> closestNodes in allClosestNodes)
            {
                height++;
                Gizmos.color = new Color(0, (float)counter / allClosestNodes.Count, 0);
                //Gizmos.Draw
                foreach (Node node in closestNodes)
                {
                    Gizmos.DrawSphere(Up(node.position, height), 0.2f);       
                }

                counter++;
            }
            
            Gizmos.DrawSphere(Up(corners[^1], 2), 0.2f);
        }

        private Vector3[] GetCorners(List<Node> path)
        {
            List<Vector3> corners = new List<Vector3>();

            foreach (Node node in path)
            {
                corners.Add(node.position);
            }

            return corners.ToArray();
        }

        private Vector3 Up(Vector3 position, float height)
        {
            return new Vector3(position.x, position.y + height, position.z);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Valkyrie;

namespace Hilaly.Services
{
    public interface IPathFinder : IService
    {
        float NavMeshQueryRange { get; set; }
        
        bool ComputePath(Vector3 position, Vector3 target, List<Vector3> points);
    }
    
    class PathFinderService : IPathFinder
    {
        public float NavMeshQueryRange { get; set; } = 1;

        public bool ComputePath(Vector3 position, Vector3 target, List<Vector3> points)
        {
            if (NavMesh.SamplePosition(target, out var hit, NavMeshQueryRange, NavMesh.AllAreas))
                target = hit.position;
            var path = new NavMeshPath();
            points.Clear();
            if (NavMesh.CalculatePath(position, target, NavMesh.AllAreas, path))
            {
                points.AddRange(path.corners);
                return true;
            }
            return false;
        }
    }
}
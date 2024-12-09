using System.Collections.Generic;
using UnityEngine;

namespace Interfaces.AI.UBS.BaseInterfaces.Get
{
    public interface IGetVectorsToEnemiesThatTargetsMe
    {
        public List<Vector3> GetVectorsToEnemiesThatTargetsMe();
    }
}
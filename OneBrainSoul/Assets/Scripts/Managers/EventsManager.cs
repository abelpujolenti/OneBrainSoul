using System;
using System.Collections.Generic;
using AI.Navigation;
using ECS.Entities;

namespace Managers
{
    public static class EventsManager
    {
        #region KeyEvents
        
        public static Action MoveForward;
        public static Action MoveLeft;
        public static Action MoveBackwards;
        public static Action MoveRight;
        public static Action PressJump;
        
        public static Action StopMovingForward;
        public static Action StopMovingLeft;
        public static Action StopMovingBackwards;
        public static Action StopMovingRight;
        public static Action ReleaseJump;
        public static Action ReleaseEscape;
        
        #endregion

        #region MouseButtonsEvents
        
        public static Action PressMouseButton0;
        public static Action PressMouseButton1;
        public static Action PressMouseButton2;
        
        public static Action ReleaseMouseButton0;
        public static Action ReleaseMouseButton1;
        public static Action ReleaseMouseButton2;        

        #endregion

        #region ScrollWheelEvents
    
        public static Action ScrollUp;
        public static Action ScrollDown;        

        #endregion

        public static Action OnDefeatEnemy;
        public static Action<EntityType, uint> OnAgentDefeated;

        #region Navigation

        public static Action<uint> UpdatePositionAndDestination;
        public static Action<uint> UpdateAgentPath;
        public static Func<List<DynamicObstacleThreadSafe>> OnUpdateDynamicObstacle;

        #endregion

    }
}

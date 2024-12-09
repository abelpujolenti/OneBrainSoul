using System;
using System.Collections.Generic;
using System.Diagnostics;
using Interfaces.AI.Navigation;
using UnityEngine;
using UnityEngine.Assertions;

namespace Threads
{
    public class MainThreadQueue
    {
        private Dictionary<CommandReturnType, Action<BaseCommand>> _commands;
        
        private Stack<ActionCommand> _actionPool;
        private Stack<GetPositionCommand> _getPositionPool;
        private Stack<GetPositionsCommand> _getPositionsPool;

        private Queue<BaseCommand> _commandQueue;

        private Stopwatch _executeLimitStopwatch;

        public MainThreadQueue()
        {
            _actionPool = new Stack<ActionCommand>(); 
            _getPositionPool = new Stack<GetPositionCommand>();
            _getPositionsPool = new Stack<GetPositionsCommand>();
            _commandQueue = new Queue<BaseCommand>();
            _executeLimitStopwatch = new Stopwatch();

            _commands = new Dictionary<CommandReturnType, Action<BaseCommand>>
            {
                { CommandReturnType.ACTION , ProcessAction },
                { CommandReturnType.POSITION , ProcessPosition },
                { CommandReturnType.POSITIONS , ProcessPositions }
            };
        }

        public void SetAction(Action action)
        {
            ActionCommand command = GetFromPool(_actionPool);
            command.action = action;
            QueueCommand(command);
        }

        private void ProcessAction(BaseCommand baseCommand)
        {
            ActionCommand actionCommand = (ActionCommand)baseCommand;
            Action action = actionCommand.action;
                        
            ReturnToPool(_actionPool, actionCommand);

            action();
        }

        public void GetPosition(IPosition iPosition, ThreadResult<Vector3> result)
        {
            result.Reset();
            GetPositionCommand command = GetFromPool(_getPositionPool);
            command.iPosition = iPosition;
            command.result = result;
            QueueCommand(command);
        }

        private void ProcessPosition(BaseCommand baseCommand)
        {
            GetPositionCommand getPositionCommand = (GetPositionCommand)baseCommand;
            IPosition iPosition = getPositionCommand.iPosition;
            ThreadResult<Vector3> result = getPositionCommand.result;
                        
            ReturnToPool(_getPositionPool, getPositionCommand);

            Vector3 position = iPosition.GetPosition();
                        
            result.Ready(position);
        }

        public void GetPositions(List<IPosition> iPositions, ThreadResult<List<Vector3>> result)
        {
            result.Reset();
            GetPositionsCommand command = GetFromPool(_getPositionsPool);
            command.iPositions = iPositions;
            command.result = result;
            QueueCommand(command);
        }

        private void ProcessPositions(BaseCommand baseCommand)
        {
            GetPositionsCommand getPositionsCommand = (GetPositionsCommand)baseCommand;
            List<IPosition> iPositions = getPositionsCommand.iPositions;
            ThreadResult<List<Vector3>> result = getPositionsCommand.result;
                        
            ReturnToPool(_getPositionsPool, getPositionsCommand);

            List<Vector3> positions = new List<Vector3>();

            foreach (IPosition iPosition in iPositions)
            {
                positions.Add(iPosition.GetPosition());
            }
                        
            result.Ready(positions);
        }

        public void Execute(int maximumMilliseconds = int.MaxValue)
        {
            Assert.IsTrue(maximumMilliseconds > 0);
            
            _executeLimitStopwatch.Reset();
            _executeLimitStopwatch.Start();

            while (_executeLimitStopwatch.ElapsedMilliseconds < maximumMilliseconds)
            {
                BaseCommand baseCommand;
                
                lock (_commandQueue)
                {
                    if (_commandQueue.Count == 0)
                    {
                        break;
                    }

                    baseCommand = _commandQueue.Dequeue();
                }

                _commands[baseCommand.type](baseCommand);
            }
        }

        private static T GetFromPool<T>(Stack<T> pool)
            where T : new()
        {
            lock (pool)
            {
                return pool.Count == 0 ? new T() : pool.Pop();
            }
        }

        private static void ReturnToPool<T>(Stack<T> pool, T obj)
        {
            lock (pool)
            {
                pool.Push(obj);
            }
        }

        private void QueueCommand(BaseCommand command)
        {
            lock (_commandQueue)
            {
                _commandQueue.Enqueue(command);
            }
        }
    }
}
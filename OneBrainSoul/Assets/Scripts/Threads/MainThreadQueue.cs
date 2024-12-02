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
        private Stack<ActionCommand> _actionPool;
        private Stack<GetPositionCommand> _getPositionPool;

        private Queue<BaseCommand> _commandQueue;

        private Stopwatch _executeLimitStopwatch;

        public MainThreadQueue()
        {
            _actionPool = new Stack<ActionCommand>(); 
            _getPositionPool = new Stack<GetPositionCommand>();
            _commandQueue = new Queue<BaseCommand>();
            _executeLimitStopwatch = new Stopwatch();
        }

        public void SetAction(Action action)
        {
            ActionCommand command = GetFromPool(_actionPool);
            command.action = action;
            QueueCommand(command);
        }

        public void GetPosition(IPosition iPosition, ThreadResult<Vector3> result)
        {
            result.Reset();
            GetPositionCommand command = GetFromPool(_getPositionPool);
            command.iPosition = iPosition;
            command.result = result;
            QueueCommand(command);
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

                switch (baseCommand.type)
                {
                    case CommandReturnType.POSITION:

                        GetPositionCommand getPositionCommand = (GetPositionCommand)baseCommand;
                        IPosition iPosition = getPositionCommand.iPosition;
                        ThreadResult<Vector3> result = getPositionCommand.result;
                        
                        ReturnToPool(_getPositionPool, getPositionCommand);

                        Vector3 position = iPosition.GetPosition();
                        
                        result.Ready(position);
                        break;
                    
                    case CommandReturnType.ACTION:

                        ActionCommand actionCommand = (ActionCommand)baseCommand;
                        Action action = actionCommand.action;
                        
                        ReturnToPool(_actionPool, actionCommand);

                        action();
                        break;
                }
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
﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Infrastructure.Grid;

namespace Infrastructure.Tick
{
    /// <summary>
    /// Manages executing tasks and ticks on a constant tick cycle. Runs on its own thread. Offers thread safe queues to input references and data.
    /// </summary>
    public class TickManager
    {
        #region Public Members
        public readonly float TickDelay;
        public bool IsRunning { get; private set; }
        public City TargetCity { get; private set; }
        public event SessionAction PreTick;
        public event SessionAction PostTick;
        public event SessionAction LowPriorityTick;
        public ConcurrentQueue<SessionAction> SessionActionsQueue;
        public ConcurrentQueue<GridSystem> NewGridSystems;
        public ConcurrentQueue<Tickable> IncomingTickableQueue;
        public ConcurrentQueue<Tickable> LowPriorityIncomingQueue;
        public delegate void SessionAction(Session s, float tickTime);
        #endregion

        #region Private Members
        /// <summary>
        /// The timer used to tick the Tick method.
        /// </summary>
        private System.Timers.Timer _timer;
        /// <summary>
        /// Used as a lock to prevent overlapping ticks.
        /// </summary>
        private static object _tickLock;
        private ElapsedEventHandler _timerHandle;
        private Session _session;
        /// <summary>
        /// The tick delay of which low priority tickables are ticked.
        /// </summary>
        private readonly float _lowPriorityRate = 4;
        private float _lowPriorityTicksPassed = 0;
        /// <summary>
        /// All the tickables that we tick.
        /// </summary>
        private List<Tickable> TickTargets;
        /// <summary>
        /// List of all LowPriority tickable objects. This List is ticked at a lower rate.
        /// </summary>
        private List<Tickable> LowPriorityTickTargets;
        /// <summary>
        /// List of referneces to the queues that all existing grid systems hold. These queues hold new tickables waiting to be added to the master TickTargets list.
        /// </summary>
        private List<ConcurrentQueue<Tickable>> GridNewTickableQueues;
        #endregion

        /// <summary>
        /// Creates a new tickmanager. The given tick delay is fixed and used throughout the lifetime of the tick manager.
        /// </summary>
        /// <param name="targetCity">The target city to manage</param>
        /// <param name="tickDelay">How long in ms to wait to tick again.</param>
        public TickManager(City targetCity, float tickDelay = 100, bool autoStart = false)
        {
            TickDelay = tickDelay;
            TargetCity = targetCity;
            _session = TargetCity.ParentSession;

            TickTargets = new List<Tickable>();
            GridNewTickableQueues = new List<ConcurrentQueue<Tickable>>();
            SessionActionsQueue = new ConcurrentQueue<SessionAction>();
            NewGridSystems = new ConcurrentQueue<GridSystem>();
            IncomingTickableQueue = new ConcurrentQueue<Tickable>();

            LowPriorityIncomingQueue = new ConcurrentQueue<Tickable>();
            LowPriorityTickTargets = new List<Tickable>();

            IsRunning = false;
            _tickLock = new object();

            if (autoStart) Start();
        }

        /// <summary>
        /// Sets up the timer and begins the ticking.
        /// </summary>
        public void Start()
        {
            //The thread is open and we begin setting up the tick manager to tick.
            _timer = new System.Timers.Timer(TickDelay);
            _timerHandle = Tick;
            _timer.Elapsed += _timerHandle;
            _timer.AutoReset = true;
            //_timer.SynchronizingObject = _syncronisingObject;
            _timer.Start();
        }

        /// <summary>
        /// The method that performs ticks. This is subscribed to the Timer Elapsed event.
        /// </summary>
        private void Tick(object sender, ElapsedEventArgs args)
        {
            //=====
            //We use a lock and use Monitor to check if we can aquire it. We dont use lock as we want to end the execution instead of pausing it.
            //=====
            bool haveLock = false;
            try
            {
                Monitor.TryEnter(_tickLock, ref haveLock);
                //If we were not able to get the lock, return.
                if (!haveLock) return;

                //*** TICK BODY ***

                //Convert the tick delay to a float in seconds.
                float ticktimeinseconds = TickDelay / 1000;

                //This is called when we tick.
                PreTick?.Invoke(_session, ticktimeinseconds);

                //Execute any delegates that are waiting.
                for (int x = 0; x < SessionActionsQueue.Count; x++)
                {
                    SessionAction sessionAction;
                    while (SessionActionsQueue.TryDequeue(out sessionAction)) sessionAction.Invoke(_session, TickDelay);
                }

                //Check if there are any new tickables in the queue.
                TryDequingNewTickables();

                //Check if there are any new grids
                TryDequeingNewGrids();

                //Test to see if there are any new tickable targets to get from grid systems.
                TryDequeingNewTargets();

                //Execute all ticks
                for (int i = 0; i < TickTargets.Count; i++)
                {
                    TickTargets[i].Tick(ticktimeinseconds);
                }

                #region Low Priority
                //Check if we should tick low priority targets.
                //Increase the timer and check its value.
                _lowPriorityTicksPassed++;
                if (_lowPriorityTicksPassed >= _lowPriorityRate)
                {
                    //Tick all low priority objects
                    for (int i = 0; i < LowPriorityTickTargets.Count; i++)
                    {
                        LowPriorityTickTargets[i].Tick(ticktimeinseconds * _lowPriorityRate);
                    }
                    //Invoke event
                    LowPriorityTick?.Invoke(_session, ticktimeinseconds * _lowPriorityRate);

                    //Reset counter
                    _lowPriorityTicksPassed = 0;
                }
                #endregion

                //Post tick event.
                PostTick?.Invoke(_session, ticktimeinseconds);
            }
            finally
            {
                if (haveLock) Monitor.Exit(_tickLock);
            }
        }

        public bool AddSessionAction(SessionAction action)
        {
            if (action == null) return false;
            SessionActionsQueue.Enqueue(action);
            return true;
        }

        private void TryDequeingNewGrids()
        {
            for (int i = 0; i < NewGridSystems.Count; i++)
            {
                GridSystem newGrid;
                while (NewGridSystems.TryDequeue(out newGrid)) GridNewTickableQueues.Add(newGrid.TickAddQueue);
            }
        }

        private void TryDequingNewTickables()
        {
            for (int i = 0; i < IncomingTickableQueue.Count; i++)
            {
                Tickable newTickable;
                while (IncomingTickableQueue.TryDequeue(out newTickable)) TickTargets.Add(newTickable);
            }
            //Dequeue Low Priority targets.
            for (int i = 0; i < LowPriorityIncomingQueue.Count; i++)
            {
                Tickable newTickable;
                while (LowPriorityIncomingQueue.TryDequeue(out newTickable)) LowPriorityTickTargets.Add(newTickable);
            }
        }

        /// <summary>
        /// Try to dequeue any new targets in the grid systems queues. If there are any we add them to the master list.
        /// </summary>
        private void TryDequeingNewTargets()
        {
            for (int i = 0; i < GridNewTickableQueues.Count; i++)
            {
                for (int j = 0; j < GridNewTickableQueues[i].Count; j++)
                {
                    Tickable newTickable;
                    while (GridNewTickableQueues[i].TryDequeue(out newTickable)) TickTargets.Add(newTickable);
                }
            }
        }
    }
}

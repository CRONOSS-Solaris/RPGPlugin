using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Timers;
using RPGPlugin.Utils;

namespace RPGPlugin.PointManagementSystem
{
    public abstract class ClassesBase
    {
        /// <summary>
        /// Contains the named object of your points and the value
        /// </summary>
        public abstract ObservableCollection<KeyValuePair<string, double>> ExpRatio { get; set; }

        /// <summary>
        /// Thread safe FIFO collection for storing points during server startup and between calculations.
        /// </summary>
        public ConcurrentQueue<ExperienceAction> _ProcessQueue { get; set; } =
            new ConcurrentQueue<ExperienceAction>();

        /// <summary>
        /// KVP lookups for larger collections are not the best for performance, Dictionary uses hashtables at
        /// the source and will keep the access rate constant regardless of how much data is inside.   
        /// </summary>
        protected virtual Dictionary<string, double> xpTable { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Controls the XP points calculation frequency.
        /// </summary>
        private Timer _queueTimer { get; set; } = new Timer();

        /// <summary>
        /// Helps prevent any long cue from being attempted a second simultaneous time.
        /// </summary>
        protected static bool _queueInProcess { get; set; }

        /// <summary>
        /// Time in seconds between attempting to process the XP for player points.
        /// </summary>
        protected abstract double _queueFrequency { get; set; }

        /// <summary>
        /// Process all current XP in the queue and begins the processing timer.
        /// </summary>
        public void Start()
        {
            _queueTimer.Interval = TimeSpan.FromSeconds(_queueFrequency).TotalMilliseconds;
            _queueTimer.Elapsed += ProcessXPCollected;
            UpdateLookupTable(null,null);
            ProcessXPCollected(null, null);
            Roles.Instance.minerConfig.ExpRatio.CollectionChanged += UpdateLookupTable;
            _queueTimer.Start();
        }

        /// <summary>
        /// Updates the private hashtable containing xp item and its value.
        /// </summary>
        private void UpdateLookupTable(object sender, NotifyCollectionChangedEventArgs e)
        {
            xpTable.Clear();
            foreach (KeyValuePair<string, double> item in ExpRatio)
                xpTable.Add(item.Key,item.Value);
        }

        /// <summary>
        /// Switch processing the XP queue to Async.
        /// </summary>
        private async void ProcessXPCollected(object state, ElapsedEventArgs elapsedEventArgs)
        {
            await ProcessXpCollectedAsync();
        }

        /// <summary>
        /// Processing of the actual queued XP
        /// </summary>
        protected abstract Task ProcessXpCollectedAsync();

        /// <summary>
        /// Returns the amount of xp points required to reach the next level.
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public abstract int ExpToLevelUp(long id);

        /// <summary>
        /// Create your own algorithm to reward points and assign the points to the correct
        /// class inside the PlayerManagers[ID]._PlayersData.YOURCLASSHERE.  This is awaitable!! 
        /// </summary>
        /// <param name="id">Player ID of the player to receive the XP</param>
        /// <param name="exp">Amount of XP</param>
        /// <returns>Task.CompletedTask</returns>
        protected abstract Task AddClassExp(long id, double exp);
    }
}
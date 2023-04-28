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
        
        //test skill point system
        public abstract ObservableCollection<KeyValuePair<int, int>> SkillPoints { get; set; }
        
        public virtual bool InitComplete { get; set; }

        public virtual void init()
        {
            InitComplete = true;
        }
        
        /// <summary>
        /// Thread safe FIFO collection for storing points during server startup and between calculations.
        /// </summary>
        public ConcurrentQueue<ExperienceAction> _ProcessQueue { get; set; } =
            new ConcurrentQueue<ExperienceAction>();

        /// <summary>
        /// KVP lookups for larger collections are not the best for performance, Dictionary uses hash tables at
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
        public virtual void QueueStart()
        {
            if (!InitComplete) init();
            _queueTimer.Interval = TimeSpan.FromSeconds(_queueFrequency).TotalMilliseconds;
            _queueTimer.Elapsed += ProcessXPCollected;
            UpdateLookupTable(null,null);
            ProcessXPCollected(null, null);
            ExpRatio.CollectionChanged += UpdateLookupTable;
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

        
        /// Switch processing the XP queue to Async.
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
        /// <param name="steamID">Player SteamID</param>
        /// <returns>int</returns>
        public abstract int ExpToLevelUp(ulong steamID);

        /// <summary>
        /// Create your own algorithm to reward points and assign the points to the correct
        /// class inside the PlayerManagers[ID]._PlayersData.YOURCLASSHERE.  This is awaitable!! 
        /// </summary>
        /// <param name="steamID">Player Steam ID</param>
        /// <param name="exp">Amount of XP</param>
        /// <returns>Task.CompletedTask</returns>
        protected abstract Task AddClassExp(ulong steamID, double exp);

        /// <summary>
        /// Called after the server is up and running.
        /// </summary>
        public abstract void Loaded();

        /// <summary>
        /// Called when the server is going offline.
        /// </summary>
        public abstract void Unloading();

    }
}
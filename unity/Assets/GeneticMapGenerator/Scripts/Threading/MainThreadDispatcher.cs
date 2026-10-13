using System;
using System.Collections.Generic;

namespace GeneticMapGenerator.Unity.Threading
{
    /// <summary>
    /// Hands work from the generator thread to Unity's main thread. Work posted with a key is
    /// coalesced — if the generator outpaces the frame rate, only the latest update for that key is
    /// rendered — while keyless work always runs. Execution order follows first posting order.
    /// </summary>
    public sealed class MainThreadDispatcher
    {
        private readonly object _gate = new object();
        private List<Entry> _pending = new List<Entry>();
        private List<Entry> _draining = new List<Entry>();

        public void Post(Action action) => Post(null, action);

        public void Post(string? coalescingKey, Action action)
        {
            lock (_gate)
            {
                if (coalescingKey != null)
                {
                    var existing = _pending.FindIndex(entry => entry.Key == coalescingKey);
                    if (existing >= 0)
                    {
                        _pending[existing] = new Entry(coalescingKey, action);
                        return;
                    }
                }

                _pending.Add(new Entry(coalescingKey, action));
            }
        }

        /// <summary>Runs everything posted so far. Call from the main thread only.</summary>
        public void Drain()
        {
            lock (_gate)
            {
                (_pending, _draining) = (_draining, _pending);
            }

            foreach (var entry in _draining)
            {
                entry.Action();
            }

            _draining.Clear();
        }

        public void Clear()
        {
            lock (_gate)
            {
                _pending.Clear();
            }
        }

        private readonly struct Entry
        {
            public Entry(string? key, Action action)
            {
                Key = key;
                Action = action;
            }

            public string? Key { get; }

            public Action Action { get; }
        }
    }
}

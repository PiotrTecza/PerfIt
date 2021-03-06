﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PerfIt.Handlers
{
    public class CurrentConcurrentCountHandler : CounterHandlerBase
    {

        private Lazy<PerformanceCounter> _counter;

        public CurrentConcurrentCountHandler
            (
            string categoryName,
            string instanceName)
            : base(categoryName, instanceName)
        {           
            BuildCounters();
        }

        public override string CounterType
        {
            get { return CounterTypes.CurrentConcurrentOperationsCount; }
        }

        protected override void OnRequestStarting(IDictionary<string, object> contextBag, PerfItContext context)
        {
            _counter.Value.Increment();
        }

        protected override void OnRequestEnding(IDictionary<string, object> contextBag, PerfItContext context)
        {
            _counter.Value.Decrement();
        }

        protected override void BuildCounters(bool newInstanceName = false)
        {         
            _counter = new Lazy<PerformanceCounter>(() =>
            {
                var counter = new PerformanceCounter()
                {
                    CategoryName = _categoryName,
                    CounterName = Name,
                    InstanceName = GetInstanceName(newInstanceName),
                    ReadOnly = false,
                    InstanceLifetime = PerformanceCounterInstanceLifetime.Process
                };

                counter.RawValue = 0;
                return counter;
            });
        }

        protected override CounterCreationData[] DoGetCreationData()
        {
            return new []
                       {
                           new CounterCreationData()
                               {
                                   CounterName = Name,
                                   CounterType = PerformanceCounterType.NumberOfItems32,
                                   CounterHelp = "# of requests running concurrently"
                               }
                       };
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_counter != null && _counter.IsValueCreated)
            {
                _counter.Value.RemoveInstance();
                _counter.Value.Dispose(); 
            }
        }

        
    }
}

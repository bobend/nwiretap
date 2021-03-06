﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NWiretap.Instruments.Meter;

namespace NWiretap.Instruments.Timer
{
    public class InvocationTimer : Meter.Meter, IInvocationTimer
    {
        private ConcurrentList<int> _invocationTimes = new ConcurrentList<int>();
        private readonly ConcurrentList<TimerSample> _samples = new ConcurrentList<TimerSample>();

        public InvocationTimer(Type owningType, string groupName, string instrumentIdent, int sampleLengthMs) : base(owningType, groupName, instrumentIdent, sampleLengthMs)
        {
        }

        protected override void CalculateValues()
        {
            base.CalculateValues();

            var invc = new List<int>(_invocationTimes);
            _invocationTimes = new ConcurrentList<int>();
            
            var sample = invc.Count <= 0 ? new TimerSample() : new TimerSample
                                                                 {
                                                                     AverageInvokationTimeMs = invc.Average(),
                                                                     MaxInvocationTimeMs = invc.Max(),
                                                                     MinInvocationTimeMs = invc.Min()
                                                                 };
            PushSample(sample);
        }

        public override string InstrumentType
        {
            get { return "InvocationTimer"; }
        }

        public override InstrumentMeasurementBase GetMeasurement()
        {
            var counterMeasurement = (MeterMeasurement)base.GetMeasurement();
            return new TimerMeasurement(counterMeasurement, _samples);
        }

        public TResult Time<TResult>(Func<TResult> func)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Start();
                return func();
            }
            finally
            {
                sw.Stop();
                _invocationTimes.Add((int)sw.ElapsedMilliseconds);
                Tick();
            }
        }

        public void Time(Action act)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Start();
                act();
            }
            finally
            {
                sw.Stop();
                _invocationTimes.Add((int)sw.ElapsedMilliseconds);
                Tick();
            }
        }

        private void PushSample(TimerSample timerSample)
        {
            if (_samples.Count >= ((1000*60)/SampleLengthMs))
            {
                _samples.RemoveAt(_samples.Count-1);
            }

            _samples.Insert(0, timerSample);
        }
    }
}
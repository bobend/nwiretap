﻿using System;
using System.Threading;
using NWiretap.Measurement;

namespace NWiretap.Instruments.Ticker
{
    public class Meter : IInstrument, IMeter, IDisposable
    {
        public string CounterName { get; private set; }
        public int SampleLengthMs { get; private set; }

        private int _ticks;
        public int Ticks { get { return _ticks; } }

        private int _lastTicks;
        private readonly System.Threading.Timer _sampleTimer;
        private float _currentFrequency;

        public Meter(string counterName, int sampleLengthMs)
        {
            CounterName = counterName;
            SampleLengthMs = sampleLengthMs;

            _sampleTimer = new System.Threading.Timer(state => CalculateValues(), null, SampleLengthMs, SampleLengthMs);
        }

        protected virtual void CalculateValues()
        {
            var ticksDelta = Ticks - _lastTicks;
            _lastTicks = Ticks;

            _currentFrequency = (ticksDelta*1000) / (SampleLengthMs*1f);
        }

        public void Tick()
        {
            Interlocked.Increment(ref _ticks);
        }

        public virtual string InstrumentIdent
        {
            get { return CounterName; }
        }

        public virtual InstrumentMeasurementBase GetMeasurement()
        {
            return new MeterMeasurement(_currentFrequency, _ticks);
        }

        public void Dispose()
        {
            _sampleTimer.Dispose();
            InstrumentTracker.RemoveInstrument(this);
        }
    }
}
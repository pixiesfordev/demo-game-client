using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class SmoothTimer {
    public static IObservable<float> ProgressTimer(TimeSpan duration) {
        return Observable.Create<float>(observer => {
            DateTime startTime = DateTime.Now;
            return Observable.EveryUpdate().Subscribe(_ => {
                TimeSpan elapsed = DateTime.Now - startTime;
                float progress = Mathf.Clamp01((float)elapsed.TotalSeconds / (float)duration.TotalSeconds);
                observer.OnNext(progress);
                if (progress >= 1)
                {
                    observer.OnCompleted();
                }
            });
        });
    }

    public static IObservable<float> ProgressTimer(float duration) {
        return ProgressTimer(TimeSpan.FromSeconds(duration));
    }

    public static IObservable<int> MilliSecTimer(int milliSec) {
        return Observable.Create<int>(observer => {
            DateTime startTime = DateTime.Now;
            return Observable.EveryUpdate().Subscribe(_ => {
                TimeSpan elapsed = DateTime.Now - startTime;
                int progress = (int)elapsed.TotalMilliseconds;
                observer.OnNext(progress);
                if (progress >= milliSec)
                {
                    observer.OnCompleted();
                }
            });
        });
    }
}

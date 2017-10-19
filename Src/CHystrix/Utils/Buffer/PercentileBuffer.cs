﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using CHystrix.Utils.Atomic;

namespace CHystrix.Utils.Buffer
{
    internal class PercentileBuffer<T> : CircularBuffer<PercentileBucket<T>>
    {
        protected readonly int BucketCapacity;

        public PercentileBuffer(int timeWindowInMilliseconds, int bucketCount, int bucketCapacity)
            : base(timeWindowInMilliseconds, bucketCount)
        {
            if (bucketCapacity < 1)
                throw new ArgumentException("Bucket capacity cannot be less than 1.");
            BucketCapacity = bucketCapacity;
        }

        protected override PercentileBucket<T> CreateEmptyBucket(long timeInMilliseconds)
        {
            return new PercentileBucket<T>(timeInMilliseconds, BucketCapacity);
        }

        public void Add(T data)
        {
            CurrentBucket.Add(data);
        }

        public void VisitData(Action<T> consume)
        {
            long currentBucketStartTime = GetCurrentBucketStartTimeInMilliseconds();
            for (int i = 0; i < Buckets.Length; i++)
            {
                PercentileBucket<T> bucket = Buckets[i];
                if (bucket.TimeInMilliseconds <= currentBucketStartTime && bucket.TimeInMilliseconds + TimeWindowInMilliseconds > currentBucketStartTime)
                {
                    int count = bucket.Count;
                    for (int j = 0; j < count; j++)
                    {
                        consume(bucket[j]);
                    }
                }
            }
        }

        public virtual List<T> GetSnapShot()
        {
            List<T> data = new List<T>();
            VisitData(item => data.Add(item));
            return data;
        }
    }
}

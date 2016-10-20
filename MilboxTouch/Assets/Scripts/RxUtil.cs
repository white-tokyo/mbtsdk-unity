using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace MilboxTouch
{
    public static class RxUtil{
        public static IObservable<Tuple<T, T>> Pre<T>(this IObservable<T> self)
        {
            return self.Scan(Tuple.Create(default(T), default(T)), (pre, current) =>
            {
                return Tuple.Create(pre.Item2, current);
            });
        }

        public static IObservable<List<T>> List<T>(this IObservable<T> self)
        {
            return self.Scan(new List<T>(), (list, current) =>
            {
                var next = new List<T>(list);
                next.Add(current);
                return next;
            });
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> self, int count)
        {
            var selfCount = self.Count();
            return self.Skip(selfCount >= count ? selfCount - count : 0);
        }
    }
}

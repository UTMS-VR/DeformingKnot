using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DrawCurve {
    public static class Extensions {
        public static IReadOnlyList<T> GetRange<T>(this IReadOnlyList<T> list, int index, int count) {
            return list.ToList().GetRange(index, count);
            // var rangeList = new List<T>();
            // for (int i = index; i < index + count; i++) {
            //     rangeList.Add(list[i]);
            // }
            // return rangeList;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RACFlightDataService {
  public static class IEnumerableExt {
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
      foreach (var item in source) {
        action(item);
      }
    }

    public static void Paginate<T>(this IEnumerable<T> source, int pageSize, Action<IEnumerable<T>> action) {
      if (pageSize <= 0) {
        throw new ArgumentException(nameof(pageSize), "pageSize must be grater than zero");
      }

      var pageCount = Math.Ceiling((double)source.Count() / pageSize);
      for (int p = 0; p < pageCount; p++) {
        var page = source.Skip(pageSize * p).Take(p);
        action(page);
      }
    }

    public static async Task Paginate<T>(this IEnumerable<T> source, int pageSize, Func<IEnumerable<T>, Task> action) {
      if (pageSize <= 0) {
        throw new ArgumentException(nameof(pageSize), "pageSize must be grater than zero");
      }

      var pageCount = Math.Ceiling((double)source.Count() / pageSize);
      for (int p = 0; p < pageCount; p++) {
        var page = source.Skip(pageSize * p).Take(pageSize);
        await action(page);
      }
    }
  }
}
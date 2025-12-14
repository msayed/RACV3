using System;

namespace RACFlightDataService {
  public static class NumberExt {
    public static TimeSpan Minutes(this int number) {
      return TimeSpan.FromMinutes(number);
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt.Common;

namespace RACFlightDataService.Extensions;

public static class AppResultExtensions
{
    public static T GetSuccess<T>(this Result<T> source)
    {
        var result = source.Match<T>(s => s, err => default);
        return result;
    }

    public static Exception GetException<T>(this Result<T> source)
    {
        var result = source.Match(s => default, err => err);
        return result;
    }

    public static bool CompareValues<TSource, TDest>(this TSource source, TDest destination)
        where TSource : class where TDest : class
    {
        if (source is null || destination is null)
            return false;

        var excludedProps = new List<string>()
        {
            "id",
            "uniquekey",
            "LastActionTime",
            "LastActionCode"
        }.Select(s => s.ToLower());
        foreach (var propertyInfo in source.GetType().GetProperties())
        {
            var gacaFlightValue = propertyInfo.GetValue(source);
            var historyPropValue = destination.GetType().GetProperty(propertyInfo.Name)
                ?.GetValue(destination);
            if (excludedProps.Contains(propertyInfo.Name.ToLower()))
            {
                continue;
            }

            if (historyPropValue is null && gacaFlightValue is null)
                continue;

            if ((historyPropValue is not null && gacaFlightValue is null))
            {
                return false;
            }

            if (historyPropValue is null)
            {
                return false;
            }

            if (historyPropValue.ToString() != gacaFlightValue.ToString())
            {
                return false;
            }
        }

        return true;
    }
}
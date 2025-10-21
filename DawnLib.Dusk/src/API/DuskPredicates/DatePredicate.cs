using System;
using Dawn;
using UnityEngine;

namespace Dusk;

[Flags]
public enum DateTimeFlags
{
    Year = 1 << 0,
    Month = 1 << 1,
    Day = 1 << 2,
    Hour = 1 << 3,
    Minute = 1 << 4,
    Second = 1 << 5
}

[CreateAssetMenu(menuName = $"{DuskModConstants.DuskPredicates}/Date Unlock Requirement", fileName = "New Date Predicate", order = DuskModConstants.PredicateOrder)]
public class DatePredicate : DuskPredicate
{
    [field: Header("Date Checks")]
    [field: SerializeField]
    public DateTimeFlags DateTimeFlags { get; private set; }

    [field: Header("Start Range Values")]
    [field: SerializeField]
    public DawnDateTime DawnStartDateTime { get; private set; }

    [field: Header("End Range Values")]
    [field: SerializeField]
    public DawnDateTime DawnEndDateTime { get; private set; }

    public override bool Evaluate()
    {
        int year = DateTime.Now.Year;
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day;
        int hour = DateTime.Now.Hour;
        int minute = DateTime.Now.Minute;
        int second = DateTime.Now.Second;
        DateTime startDateTime = new(DawnStartDateTime.Year, DawnStartDateTime.Month, DawnStartDateTime.Day, DawnStartDateTime.Hour, DawnStartDateTime.Minute, DawnStartDateTime.Second);
        DateTime endDateTime = new(DawnEndDateTime.Year, DawnEndDateTime.Month, DawnEndDateTime.Day, DawnEndDateTime.Hour, DawnEndDateTime.Minute, DawnEndDateTime.Second);

        if (!DateTimeFlags.HasFlag(DateTimeFlags.Year))
        {
            year = startDateTime.Year;
        }

        if (!DateTimeFlags.HasFlag(DateTimeFlags.Month))
        {
            month = startDateTime.Month;
        }

        if (!DateTimeFlags.HasFlag(DateTimeFlags.Day))
        {
            day = startDateTime.Day;
        }

        if (!DateTimeFlags.HasFlag(DateTimeFlags.Hour))
        {
            hour = startDateTime.Hour;
        }

        if (!DateTimeFlags.HasFlag(DateTimeFlags.Minute))
        {
            minute = startDateTime.Minute;
        }

        if (!DateTimeFlags.HasFlag(DateTimeFlags.Second))
        {
            second = startDateTime.Second;
        }

        DateTime currentDateTime = new(year, month, day, hour, minute, second);
        return currentDateTime >= startDateTime && currentDateTime <= endDateTime;
    }

    public override void Register(NamespacedKey namespacedKey)
    {
    }
}
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
        DateTime currentDateTime = DateTime.Now;
        if (DateTimeFlags.HasFlag(DateTimeFlags.Year))
        {
            int year = currentDateTime.Year;
            if (year < DawnStartDateTime.Year || year > DawnEndDateTime.Year)
                return false;
        }

        if (DateTimeFlags.HasFlag(DateTimeFlags.Month))
        {
            int month = currentDateTime.Month;
            if (month < DawnStartDateTime.Month || month > DawnEndDateTime.Month)
                return false;
        }

        if (DateTimeFlags.HasFlag(DateTimeFlags.Day))
        {
            int day = currentDateTime.Day;
            if (day < DawnStartDateTime.Day || day > DawnEndDateTime.Day)
                return false;
        }

        if (DateTimeFlags.HasFlag(DateTimeFlags.Hour))
        {
            int hour = currentDateTime.Hour;
            if (hour < DawnStartDateTime.Hour || hour > DawnEndDateTime.Hour)
                return false;
        }

        if (DateTimeFlags.HasFlag(DateTimeFlags.Minute))
        {
            int minute = currentDateTime.Minute;
            if (minute < DawnStartDateTime.Minute || minute > DawnEndDateTime.Minute)
                return false;
        }

        if (DateTimeFlags.HasFlag(DateTimeFlags.Second))
        {
            int second = currentDateTime.Second;
            if (second < DawnStartDateTime.Second || second > DawnEndDateTime.Second)
                return false;
        }
        return true;
    }

    public override void Register(NamespacedKey namespacedKey)
    {
    }
}
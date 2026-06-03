using System;
using UnityEngine;

public enum DayPhase
{
    Day,
    Dusk,
    Night
}

public sealed class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float dayLengthSeconds = 300f;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float elapsedInDay;

    public event Action Changed;

    public float DayLengthSeconds => dayLengthSeconds;
    public int CurrentDay => currentDay;
    public float NormalizedTime => dayLengthSeconds > 0f ? Mathf.Clamp01(elapsedInDay / dayLengthSeconds) : 0f;
    public DayPhase CurrentPhase => PhaseFromNormalizedTime(NormalizedTime);
    public bool IsNight => CurrentPhase == DayPhase.Night;

    private void Update()
    {
        AdvanceTime(Time.deltaTime);
    }

    public void Configure(float newDayLengthSeconds)
    {
        dayLengthSeconds = Mathf.Max(1f, newDayLengthSeconds);
    }

    public void AdvanceTime(float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        elapsedInDay += seconds;
        while (elapsedInDay >= dayLengthSeconds)
        {
            elapsedInDay -= dayLengthSeconds;
            currentDay++;
        }

        Changed?.Invoke();
    }

    public void SetState(int day, float normalizedTime)
    {
        currentDay = Mathf.Max(1, day);
        elapsedInDay = Mathf.Clamp01(normalizedTime) * dayLengthSeconds;
        Changed?.Invoke();
    }

    public static DayPhase PhaseFromNormalizedTime(float normalizedTime)
    {
        if (normalizedTime < 0.65f)
        {
            return DayPhase.Day;
        }

        if (normalizedTime < 0.8f)
        {
            return DayPhase.Dusk;
        }

        return DayPhase.Night;
    }
}

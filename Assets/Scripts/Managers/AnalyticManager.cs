using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class AnalyticManager : MonoSingleton<AnalyticManager>
{
    #region Data Structures
    private struct EventData
    {
        public string eventName;
        public List<EventParameterData> eventParameters;
    }

    private struct EventParameterData
    {
        public string parameterName;
        public object parameterValue;
    }
    
    [Serializable]
    private struct EventNameData
    {
        public string eventName;
        public string parameterName;
    }

    private enum EventType
    {
        SessionLength,
        FitMeAmount,
        BlockRandomization
    }
    #endregion
    
    [SerializeField, SerializedDictionary("Event Type", "Event Name")]
    private SerializedDictionary<EventType, EventNameData> eventNameDictionary;
    
    void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
    }
    
    public void OnSessionEnd(int fitMeAmount, float sessionTime)
    {
        var lengthData = CreateEventData(EventType.SessionLength, sessionTime);
        Debug.Log($"Fit Me Amount {fitMeAmount}");
        var fitMeData = CreateEventData(EventType.FitMeAmount, fitMeAmount); ;
        SendEvent(lengthData);
        SendEvent(fitMeData);
    }

    public void OnRandomBlock(BlockTypes blockTypes)
    {
        string blockType = blockTypes.ToString();
        var eventData = CreateEventData(EventType.BlockRandomization, blockType);
        SendEvent(eventData);
    }

    private EventData CreateEventData(EventType eventType, int parameterValue)
    {
        string eventName = eventNameDictionary[eventType].eventName;
        string parameterName = eventNameDictionary[eventType].parameterName;
        List<EventParameterData> eventParameters = new List<EventParameterData>
        {
            new()
            {
                parameterName = parameterName,
                parameterValue = (int)parameterValue
            }
        };
        return new EventData
        {
            eventName = eventName,
            eventParameters = eventParameters
        };
    }
    
    private EventData CreateEventData(EventType eventType, float parameterValue)
    {
        string eventName = eventNameDictionary[eventType].eventName;
        string parameterName = eventNameDictionary[eventType].parameterName;
        List<EventParameterData> eventParameters = new List<EventParameterData>
        {
            new()
            {
                parameterName = parameterName,
                parameterValue = (float)parameterValue
            }
        };
        return new EventData
        {
            eventName = eventName,
            eventParameters = eventParameters
        };
    }
    
    private EventData CreateEventData(EventType eventType, string parameterValue)
    {
        string eventName = eventNameDictionary[eventType].eventName;
        string parameterName = eventNameDictionary[eventType].parameterName;
        List<EventParameterData> eventParameters = new List<EventParameterData>
        {
            new()
            {
                parameterName = parameterName,
                parameterValue = (string)parameterValue
            }
        };
        return new EventData
        {
            eventName = eventName,
            eventParameters = eventParameters
        };
    }

    private void SendEvent(EventData eventData)
    {
        CustomEvent customEvent = new CustomEvent(eventData.eventName);
        eventData.eventParameters.ForEach(parameter =>
        {
            customEvent.Add(parameter.parameterName, parameter.parameterValue);
        });
        AnalyticsService.Instance.RecordEvent(customEvent);
        Debug.Log($"Event sent: {eventData.eventName}");
    }
}

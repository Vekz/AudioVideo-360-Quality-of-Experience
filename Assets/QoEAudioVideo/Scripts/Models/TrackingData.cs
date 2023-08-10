using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class TrackingData
{
    public JsonDateTime TimeStamp;
    public float W;
    public float X;
    public float Y;
    public float Z;
}

[Serializable]
public class TrackingsDataCollection
{
    public List<TrackingData> TrackingData = new();

    public void CopyTo(TrackingsDataCollection target)
        => target.TrackingData = TrackingData.ToList();

    public void Clear()
        => TrackingData.Clear();

    public void Add(TrackingData tracking)
        => TrackingData.Add(tracking);

    public TrackingData First()
        => TrackingData.First();

    public TrackingData Last()
        => TrackingData.Last();
}

[Serializable]
public struct JsonDateTime
{
    public long value;
    public static implicit operator DateTime(JsonDateTime jdt)
    {
        return DateTime.FromFileTimeUtc(jdt.value);
    }
    public static implicit operator JsonDateTime(DateTime dt)
    {
        JsonDateTime jdt = new JsonDateTime();
        jdt.value = dt.ToFileTimeUtc();
        return jdt;
    }
    public override string ToString()
        => ((DateTime)this).ToString("yyyyMMddHHmmssffff");
}


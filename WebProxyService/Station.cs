using System;
using System.Runtime.Serialization;

[DataContract]
public class Station
{
    [DataMember]
    public int number { get; set; }

    [DataMember]
    public string name { get; set; }

    [DataMember]
    public Position position { get; set; }

    [DataMember]
    public string status { get; set; }

    [DataMember]
    public Totalstands totalStands { get; set; }

    public string contractName { get; set; }
    public string address { get; set; }
    public bool banking { get; set; }
    public bool bonus { get; set; }
    public DateTime lastUpdate { get; set; }
    public bool connected { get; set; }
    public bool overflow { get; set; }
    public object shape { get; set; }
    public Mainstands mainStands { get; set; }
    public object overflowStands { get; set; }

}

public class Position
{
    [DataMember]
    public float latitude { get; set; }

    [DataMember]
    public float longitude { get; set; }
}

public class Totalstands
{
    [DataMember]
    public Availabilities availabilities { get; set; }

    [DataMember]
    public int capacity { get; set; }
}

public class Mainstands
{
    public Availabilities availabilities { get; set; }
    public int capacity { get; set; }
}


public class Availabilities
{
    [DataMember]
    public int bikes { get; set; }

    [DataMember]
    public int stands { get; set; }

    [DataMember]
    public int mechanicalBikes { get; set; }

    [DataMember]
    public int electricalBikes { get; set; }

    public int electricalInternalBatteryBikes { get; set; }
    public int electricalRemovableBatteryBikes { get; set; }
}
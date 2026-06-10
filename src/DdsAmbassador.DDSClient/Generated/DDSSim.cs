
/*
WARNING: THIS FILE IS AUTO-GENERATED. DO NOT MODIFY.

This file was generated from DDSSim.idl
using RTI Code Generator (rtiddsgen) version 4.3.1.
The rtiddsgen tool is part of the RTI Connext DDS distribution.
For more information, type 'rtiddsgen -help' at a command shell
or consult the Code Generator User's Manual.
*/

using System;
using System.Reflection;
using System.Collections.Generic;
using Rti.Types;
using System.Linq;
using Omg.Types;

namespace ENUM
{

    public enum ForceIdentifier
    {
        Other = 0,
        Friend = 1,
        Hostile = 2,
        Neutral = 3,
        SureFriend = 4,
        Unknown = 5,
        Suspect = 6,
        ExFriend = 7,
        Faker = 8,
        ExNeutral = 9,
        ExSureFriend = 10,
        ExUnknown = 11,
        Joker = 12
    }

    public enum JammingType
    {
        NoJamming = 0,
        ANJ = 1,
        RGPO = 2,
        VGPO = 3
    }

    public enum ATSStatus
    {
        NA = 0,
        Turning = 1,
        Evasive = 2,
        Intercept = 3,
        TBMStart = 11,
        TBMSeparation = 12,
        TBMDebris = 13,
        EndOfFlight = 21
    }

    public enum MunitionStatus
    {
        NA = 0,
        Launch = 1,
        Separation = 2,
        InterceptSuccess = 4,
        SelfDestruction = 5,
        EmergencyDestruction = 6,
        TerrainImpact = 7
    }

    public enum MunitionType
    {
        NA = 0,
        AAM = 1,
        ABM = 2,
        Booster = 3,
        Shroud = 4,
        HABM = 5
    }

    public enum ThreatType
    {
        Air = 0,
        TBM = 1
    }

    public enum SeekerPhase
    {
        NS = 0,
        SeekerOn = 1,
        LockOnTarget = 2
    }

    public enum NetworkID
    {
        DDS = 0,
        RTI = 1,
        LTE = 2,
        Link16 = 3,
        MDIL = 4,
        REG = 5,
        LCHR = 6,
        EMS = 7,
        SIU = 8,
        CIGP = 9
    }

    public enum MessageID
    {
        AirThreatInformation = 1,
        MunitionInformation = 2,
        UplinkInformation = 3,
        DownlinkInformation = 4,
        TimeTickInformation = 5,
        GroundVehicleInformation = 6,
        SimulatorStatus = 7,
        SetLog = 8,
        SetConfiguration = 9,
        SetScenario = 10,
        SetSimulation = 11,
        Collision = 12,
        MunitionDetonation = 13,
        WeaponFire = 14,
        TargetInformation = 15,
        TrackNumberInfo = 16
    }

    public enum SimulatorID
    {
        TCC = 1,
        TDS = 2,
        VGD = 3,
        DOLU = 4,
        ELU = 5,
        ATS_A = 11,
        ATS_B = 12,
        ATS_C = 13,
        ATS_D = 14,
        MSS_A = 21,
        MSS_B = 22
    }

    public enum SimulationStatus
    {
        NA = 0,
        On = 1,
        Off = 2,
        Scenario = 3,
        Start = 4,
        Stop = 5
    }

    public enum CollisionStatus
    {
        InElastic = 0,
        Elastic = 1
    }

    public enum DetonationType
    {
        NA = 0,
        ED = 29,
        SDLockOnFail = 30,
        SDTimeOut = 31,
        SDDetectFail = 32,
        SDHitFail = 33
    }
} // namespace ENUM
namespace STRUCT
{

    public class MessageHeader8 :  IEquatable<MessageHeader8>
    {
        public ulong TimeTick { get; set; }
        public ulong TimeStamp { get; set; }
        public global::ENUM.MessageID MsgID { get; set; }
        public global::ENUM.SimulatorID SrcSimID { get; set; }
        public global::ENUM.SimulatorID DstSimID { get; set; }

        public MessageHeader8()
        {
            MsgID = (global::ENUM.MessageID) (1);
            SrcSimID = (global::ENUM.SimulatorID) (1);
            DstSimID = (global::ENUM.SimulatorID) (1);
        }

        public MessageHeader8(ulong  TimeTick, ulong  TimeStamp, global::ENUM.MessageID  MsgID, global::ENUM.SimulatorID  SrcSimID, global::ENUM.SimulatorID  DstSimID)
        {
            this.TimeTick = TimeTick;
            this.TimeStamp = TimeStamp;
            this.MsgID = MsgID;
            this.SrcSimID = SrcSimID;
            this.DstSimID = DstSimID;
        }

        public MessageHeader8(MessageHeader8 other)
        {
            if (other == null)
            {
                return;
            }

            this.TimeTick = other.TimeTick;
            this.TimeStamp = other.TimeStamp;
            this.MsgID = other.MsgID;
            this.SrcSimID = other.SrcSimID;
            this.DstSimID = other.DstSimID;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.TimeTick);
            hash.Add(this.TimeStamp);
            hash.Add(this.MsgID);
            hash.Add(this.SrcSimID);
            hash.Add(this.DstSimID);

            return hash.ToHashCode();
        }

        public bool Equals(MessageHeader8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.TimeTick.Equals(other.TimeTick) && 
            this.TimeStamp.Equals(other.TimeStamp) && 
            this.MsgID.Equals(other.MsgID) && 
            this.SrcSimID.Equals(other.SrcSimID) && 
            this.DstSimID.Equals(other.DstSimID);
        }

        public override bool Equals(object obj) => this.Equals(obj as MessageHeader8);

        public override string ToString() => MessageHeader8Support.Instance.ToString(this);
    }

    public class StandardDeviation2 :  IEquatable<StandardDeviation2>
    {
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort Z { get; set; }

        public StandardDeviation2()
        {
        }

        public StandardDeviation2(ushort  X, ushort  Y, ushort  Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public StandardDeviation2(StandardDeviation2 other)
        {
            if (other == null)
            {
                return;
            }

            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.X);
            hash.Add(this.Y);
            hash.Add(this.Z);

            return hash.ToHashCode();
        }

        public bool Equals(StandardDeviation2 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.X.Equals(other.X) && 
            this.Y.Equals(other.Y) && 
            this.Z.Equals(other.Z);
        }

        public override bool Equals(object obj) => this.Equals(obj as StandardDeviation2);

        public override string ToString() => StandardDeviation2Support.Instance.ToString(this);
    }

    public class Position8 :  IEquatable<Position8>
    {
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double LocationZ { get; set; }

        public Position8()
        {
        }

        public Position8(double  LocationX, double  LocationY, double  LocationZ)
        {
            this.LocationX = LocationX;
            this.LocationY = LocationY;
            this.LocationZ = LocationZ;
        }

        public Position8(Position8 other)
        {
            if (other == null)
            {
                return;
            }

            this.LocationX = other.LocationX;
            this.LocationY = other.LocationY;
            this.LocationZ = other.LocationZ;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.LocationX);
            hash.Add(this.LocationY);
            hash.Add(this.LocationZ);

            return hash.ToHashCode();
        }

        public bool Equals(Position8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.LocationX.Equals(other.LocationX) && 
            this.LocationY.Equals(other.LocationY) && 
            this.LocationZ.Equals(other.LocationZ);
        }

        public override bool Equals(object obj) => this.Equals(obj as Position8);

        public override string ToString() => Position8Support.Instance.ToString(this);
    }

    public class RelativePosition8 :  IEquatable<RelativePosition8>
    {
        public double DistanceX { get; set; }
        public double DistanceY { get; set; }
        public double DistanceZ { get; set; }

        public RelativePosition8()
        {
        }

        public RelativePosition8(double  DistanceX, double  DistanceY, double  DistanceZ)
        {
            this.DistanceX = DistanceX;
            this.DistanceY = DistanceY;
            this.DistanceZ = DistanceZ;
        }

        public RelativePosition8(RelativePosition8 other)
        {
            if (other == null)
            {
                return;
            }

            this.DistanceX = other.DistanceX;
            this.DistanceY = other.DistanceY;
            this.DistanceZ = other.DistanceZ;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.DistanceX);
            hash.Add(this.DistanceY);
            hash.Add(this.DistanceZ);

            return hash.ToHashCode();
        }

        public bool Equals(RelativePosition8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.DistanceX.Equals(other.DistanceX) && 
            this.DistanceY.Equals(other.DistanceY) && 
            this.DistanceZ.Equals(other.DistanceZ);
        }

        public override bool Equals(object obj) => this.Equals(obj as RelativePosition8);

        public override string ToString() => RelativePosition8Support.Instance.ToString(this);
    }

    public class LLA8 :  IEquatable<LLA8>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public LLA8()
        {
        }

        public LLA8(double  Latitude, double  Longitude, double  Altitude)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Altitude = Altitude;
        }

        public LLA8(LLA8 other)
        {
            if (other == null)
            {
                return;
            }

            this.Latitude = other.Latitude;
            this.Longitude = other.Longitude;
            this.Altitude = other.Altitude;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Latitude);
            hash.Add(this.Longitude);
            hash.Add(this.Altitude);

            return hash.ToHashCode();
        }

        public bool Equals(LLA8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Latitude.Equals(other.Latitude) && 
            this.Longitude.Equals(other.Longitude) && 
            this.Altitude.Equals(other.Altitude);
        }

        public override bool Equals(object obj) => this.Equals(obj as LLA8);

        public override string ToString() => LLA8Support.Instance.ToString(this);
    }

    public class Velocity8 :  IEquatable<Velocity8>
    {
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double VelocityZ { get; set; }

        public Velocity8()
        {
        }

        public Velocity8(double  VelocityX, double  VelocityY, double  VelocityZ)
        {
            this.VelocityX = VelocityX;
            this.VelocityY = VelocityY;
            this.VelocityZ = VelocityZ;
        }

        public Velocity8(Velocity8 other)
        {
            if (other == null)
            {
                return;
            }

            this.VelocityX = other.VelocityX;
            this.VelocityY = other.VelocityY;
            this.VelocityZ = other.VelocityZ;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.VelocityX);
            hash.Add(this.VelocityY);
            hash.Add(this.VelocityZ);

            return hash.ToHashCode();
        }

        public bool Equals(Velocity8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.VelocityX.Equals(other.VelocityX) && 
            this.VelocityY.Equals(other.VelocityY) && 
            this.VelocityZ.Equals(other.VelocityZ);
        }

        public override bool Equals(object obj) => this.Equals(obj as Velocity8);

        public override string ToString() => Velocity8Support.Instance.ToString(this);
    }

    public class Acceleration8 :  IEquatable<Acceleration8>
    {
        public double AccelerationX { get; set; }
        public double AccelerationY { get; set; }
        public double AccelerationZ { get; set; }

        public Acceleration8()
        {
        }

        public Acceleration8(double  AccelerationX, double  AccelerationY, double  AccelerationZ)
        {
            this.AccelerationX = AccelerationX;
            this.AccelerationY = AccelerationY;
            this.AccelerationZ = AccelerationZ;
        }

        public Acceleration8(Acceleration8 other)
        {
            if (other == null)
            {
                return;
            }

            this.AccelerationX = other.AccelerationX;
            this.AccelerationY = other.AccelerationY;
            this.AccelerationZ = other.AccelerationZ;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.AccelerationX);
            hash.Add(this.AccelerationY);
            hash.Add(this.AccelerationZ);

            return hash.ToHashCode();
        }

        public bool Equals(Acceleration8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.AccelerationX.Equals(other.AccelerationX) && 
            this.AccelerationY.Equals(other.AccelerationY) && 
            this.AccelerationZ.Equals(other.AccelerationZ);
        }

        public override bool Equals(object obj) => this.Equals(obj as Acceleration8);

        public override string ToString() => Acceleration8Support.Instance.ToString(this);
    }

    public class Orientation8 :  IEquatable<Orientation8>
    {
        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }

        public Orientation8()
        {
        }

        public Orientation8(double  Roll, double  Pitch, double  Yaw)
        {
            this.Roll = Roll;
            this.Pitch = Pitch;
            this.Yaw = Yaw;
        }

        public Orientation8(Orientation8 other)
        {
            if (other == null)
            {
                return;
            }

            this.Roll = other.Roll;
            this.Pitch = other.Pitch;
            this.Yaw = other.Yaw;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Roll);
            hash.Add(this.Pitch);
            hash.Add(this.Yaw);

            return hash.ToHashCode();
        }

        public bool Equals(Orientation8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Roll.Equals(other.Roll) && 
            this.Pitch.Equals(other.Pitch) && 
            this.Yaw.Equals(other.Yaw);
        }

        public override bool Equals(object obj) => this.Equals(obj as Orientation8);

        public override string ToString() => Orientation8Support.Instance.ToString(this);
    }

    public class AER8 :  IEquatable<AER8>
    {
        public double Azimuth { get; set; }
        public double Elevation { get; set; }
        public double Roll { get; set; }

        public AER8()
        {
        }

        public AER8(double  Azimuth, double  Elevation, double  Roll)
        {
            this.Azimuth = Azimuth;
            this.Elevation = Elevation;
            this.Roll = Roll;
        }

        public AER8(AER8 other)
        {
            if (other == null)
            {
                return;
            }

            this.Azimuth = other.Azimuth;
            this.Elevation = other.Elevation;
            this.Roll = other.Roll;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Azimuth);
            hash.Add(this.Elevation);
            hash.Add(this.Roll);

            return hash.ToHashCode();
        }

        public bool Equals(AER8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Azimuth.Equals(other.Azimuth) && 
            this.Elevation.Equals(other.Elevation) && 
            this.Roll.Equals(other.Roll);
        }

        public override bool Equals(object obj) => this.Equals(obj as AER8);

        public override string ToString() => AER8Support.Instance.ToString(this);
    }

    public class ObjectInfo4 :  IEquatable<ObjectInfo4>
    {
        public global::ENUM.ForceIdentifier IFF { get; set; }
        public ushort ModelKind { get; set; }
        public ushort ObjectNumber { get; set; }
        public bool IsFrozen { get; set; }

        public ObjectInfo4()
        {
            IFF = (global::ENUM.ForceIdentifier) (0);
        }

        public ObjectInfo4(global::ENUM.ForceIdentifier  IFF, ushort  ModelKind, ushort  ObjectNumber, bool  IsFrozen)
        {
            this.IFF = IFF;
            this.ModelKind = ModelKind;
            this.ObjectNumber = ObjectNumber;
            this.IsFrozen = IsFrozen;
        }

        public ObjectInfo4(ObjectInfo4 other)
        {
            if (other == null)
            {
                return;
            }

            this.IFF = other.IFF;
            this.ModelKind = other.ModelKind;
            this.ObjectNumber = other.ObjectNumber;
            this.IsFrozen = other.IsFrozen;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.IFF);
            hash.Add(this.ModelKind);
            hash.Add(this.ObjectNumber);
            hash.Add(this.IsFrozen);

            return hash.ToHashCode();
        }

        public bool Equals(ObjectInfo4 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.IFF.Equals(other.IFF) && 
            this.ModelKind.Equals(other.ModelKind) && 
            this.ObjectNumber.Equals(other.ObjectNumber) && 
            this.IsFrozen.Equals(other.IsFrozen);
        }

        public override bool Equals(object obj) => this.Equals(obj as ObjectInfo4);

        public override string ToString() => ObjectInfo4Support.Instance.ToString(this);
    }

    public class Jamming4 :  IEquatable<Jamming4>
    {
        public global::ENUM.JammingType Type { get; set; }
        public ushort JammingPower { get; set; }
        public ushort JammingPullOff { get; set; }

        public Jamming4()
        {
            Type = (global::ENUM.JammingType) (0);
        }

        public Jamming4(global::ENUM.JammingType  Type, ushort  JammingPower, ushort  JammingPullOff)
        {
            this.Type = Type;
            this.JammingPower = JammingPower;
            this.JammingPullOff = JammingPullOff;
        }

        public Jamming4(Jamming4 other)
        {
            if (other == null)
            {
                return;
            }

            this.Type = other.Type;
            this.JammingPower = other.JammingPower;
            this.JammingPullOff = other.JammingPullOff;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Type);
            hash.Add(this.JammingPower);
            hash.Add(this.JammingPullOff);

            return hash.ToHashCode();
        }

        public bool Equals(Jamming4 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Type.Equals(other.Type) && 
            this.JammingPower.Equals(other.JammingPower) && 
            this.JammingPullOff.Equals(other.JammingPullOff);
        }

        public override bool Equals(object obj) => this.Equals(obj as Jamming4);

        public override string ToString() => Jamming4Support.Instance.ToString(this);
    }

    public class LCHRStatus2 :  IEquatable<LCHRStatus2>
    {
        public ushort LaunchReadyStatus { get; set; }
        public ushort LCUStatus { get; set; }
        public ushort HotInventory { get; set; }
        public ushort ColdInventory { get; set; }
        public byte LSSNumber { get; set; }
        public byte ConnectionStatus { get; set; }

        public LCHRStatus2()
        {
        }

        public LCHRStatus2(ushort  LaunchReadyStatus, ushort  LCUStatus, ushort  HotInventory, ushort  ColdInventory, byte  LSSNumber, byte  ConnectionStatus)
        {
            this.LaunchReadyStatus = LaunchReadyStatus;
            this.LCUStatus = LCUStatus;
            this.HotInventory = HotInventory;
            this.ColdInventory = ColdInventory;
            this.LSSNumber = LSSNumber;
            this.ConnectionStatus = ConnectionStatus;
        }

        public LCHRStatus2(LCHRStatus2 other)
        {
            if (other == null)
            {
                return;
            }

            this.LaunchReadyStatus = other.LaunchReadyStatus;
            this.LCUStatus = other.LCUStatus;
            this.HotInventory = other.HotInventory;
            this.ColdInventory = other.ColdInventory;
            this.LSSNumber = other.LSSNumber;
            this.ConnectionStatus = other.ConnectionStatus;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.LaunchReadyStatus);
            hash.Add(this.LCUStatus);
            hash.Add(this.HotInventory);
            hash.Add(this.ColdInventory);
            hash.Add(this.LSSNumber);
            hash.Add(this.ConnectionStatus);

            return hash.ToHashCode();
        }

        public bool Equals(LCHRStatus2 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.LaunchReadyStatus.Equals(other.LaunchReadyStatus) && 
            this.LCUStatus.Equals(other.LCUStatus) && 
            this.HotInventory.Equals(other.HotInventory) && 
            this.ColdInventory.Equals(other.ColdInventory) && 
            this.LSSNumber.Equals(other.LSSNumber) && 
            this.ConnectionStatus.Equals(other.ConnectionStatus);
        }

        public override bool Equals(object obj) => this.Equals(obj as LCHRStatus2);

        public override string ToString() => LCHRStatus2Support.Instance.ToString(this);
    }

    public class MFRStatus8 :  IEquatable<MFRStatus8>
    {
        public double BeamAzimuth { get; set; }
        public double BeamAzimuthOffset { get; set; }
        public double BeamElevation { get; set; }
        public double BeamElevationOffset { get; set; }
        public double BeamHorizontalArea { get; set; }
        public double BeamVerticalArea { get; set; }
        public uint BeamRange { get; set; }
        public byte BeamOnOffState { get; set; }

        public MFRStatus8()
        {
        }

        public MFRStatus8(double  BeamAzimuth, double  BeamAzimuthOffset, double  BeamElevation, double  BeamElevationOffset, double  BeamHorizontalArea, double  BeamVerticalArea, uint  BeamRange, byte  BeamOnOffState)
        {
            this.BeamAzimuth = BeamAzimuth;
            this.BeamAzimuthOffset = BeamAzimuthOffset;
            this.BeamElevation = BeamElevation;
            this.BeamElevationOffset = BeamElevationOffset;
            this.BeamHorizontalArea = BeamHorizontalArea;
            this.BeamVerticalArea = BeamVerticalArea;
            this.BeamRange = BeamRange;
            this.BeamOnOffState = BeamOnOffState;
        }

        public MFRStatus8(MFRStatus8 other)
        {
            if (other == null)
            {
                return;
            }

            this.BeamAzimuth = other.BeamAzimuth;
            this.BeamAzimuthOffset = other.BeamAzimuthOffset;
            this.BeamElevation = other.BeamElevation;
            this.BeamElevationOffset = other.BeamElevationOffset;
            this.BeamHorizontalArea = other.BeamHorizontalArea;
            this.BeamVerticalArea = other.BeamVerticalArea;
            this.BeamRange = other.BeamRange;
            this.BeamOnOffState = other.BeamOnOffState;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.BeamAzimuth);
            hash.Add(this.BeamAzimuthOffset);
            hash.Add(this.BeamElevation);
            hash.Add(this.BeamElevationOffset);
            hash.Add(this.BeamHorizontalArea);
            hash.Add(this.BeamVerticalArea);
            hash.Add(this.BeamRange);
            hash.Add(this.BeamOnOffState);

            return hash.ToHashCode();
        }

        public bool Equals(MFRStatus8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.BeamAzimuth.Equals(other.BeamAzimuth) && 
            this.BeamAzimuthOffset.Equals(other.BeamAzimuthOffset) && 
            this.BeamElevation.Equals(other.BeamElevation) && 
            this.BeamElevationOffset.Equals(other.BeamElevationOffset) && 
            this.BeamHorizontalArea.Equals(other.BeamHorizontalArea) && 
            this.BeamVerticalArea.Equals(other.BeamVerticalArea) && 
            this.BeamRange.Equals(other.BeamRange) && 
            this.BeamOnOffState.Equals(other.BeamOnOffState);
        }

        public override bool Equals(object obj) => this.Equals(obj as MFRStatus8);

        public override string ToString() => MFRStatus8Support.Instance.ToString(this);
    }

    public class Config8 :  IEquatable<Config8>
    {
        [Bound(255)]
        public string ConfigName { get; set; } = string.Empty;
        [Bound(255)]
        public string ConfigVersion { get; set; } = string.Empty;
        public global::ENUM.SimulatorID SimID { get; set; }

        public Config8()
        {
            SimID = (global::ENUM.SimulatorID) (1);
        }

        public Config8(string  ConfigName, string  ConfigVersion, global::ENUM.SimulatorID  SimID)
        {
            this.ConfigName = ConfigName;
            this.ConfigVersion = ConfigVersion;
            this.SimID = SimID;
        }

        public Config8(Config8 other)
        {
            if (other == null)
            {
                return;
            }

            this.ConfigName = other.ConfigName;
            this.ConfigVersion = other.ConfigVersion;
            this.SimID = other.SimID;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.ConfigName);
            hash.Add(this.ConfigVersion);
            hash.Add(this.SimID);

            return hash.ToHashCode();
        }

        public bool Equals(Config8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.ConfigName.Equals(other.ConfigName) && 
            this.ConfigVersion.Equals(other.ConfigVersion) && 
            this.SimID.Equals(other.SimID);
        }

        public override bool Equals(object obj) => this.Equals(obj as Config8);

        public override string ToString() => Config8Support.Instance.ToString(this);
    }

    public class AirThreat8 :  IEquatable<AirThreat8>
    {
        public global::STRUCT.Position8 Position { get; set; }
        public global::STRUCT.Velocity8 Velocity { get; set; }
        public global::STRUCT.Acceleration8 Acceleration { get; set; }
        public global::STRUCT.Orientation8 Orientation { get; set; }
        public global::STRUCT.ObjectInfo4 Information { get; set; }
        public global::STRUCT.Jamming4 JammingStatus { get; set; }
        public global::ENUM.ATSStatus Status { get; set; }
        public ushort MeanRCS { get; set; }

        public AirThreat8()
        {
            Position = new global::STRUCT.Position8();
            Velocity = new global::STRUCT.Velocity8();
            Acceleration = new global::STRUCT.Acceleration8();
            Orientation = new global::STRUCT.Orientation8();
            Information = new global::STRUCT.ObjectInfo4();
            JammingStatus = new global::STRUCT.Jamming4();
            Status = (global::ENUM.ATSStatus) (0);
        }

        public AirThreat8(global::STRUCT.Position8  Position, global::STRUCT.Velocity8  Velocity, global::STRUCT.Acceleration8  Acceleration, global::STRUCT.Orientation8  Orientation, global::STRUCT.ObjectInfo4  Information, global::STRUCT.Jamming4  JammingStatus, global::ENUM.ATSStatus  Status, ushort  MeanRCS)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Acceleration = Acceleration;
            this.Orientation = Orientation;
            this.Information = Information;
            this.JammingStatus = JammingStatus;
            this.Status = Status;
            this.MeanRCS = MeanRCS;
        }

        public AirThreat8(AirThreat8 other)
        {
            if (other == null)
            {
                return;
            }

            this.Position = new global::STRUCT.Position8(other.Position);
            this.Velocity = new global::STRUCT.Velocity8(other.Velocity);
            this.Acceleration = new global::STRUCT.Acceleration8(other.Acceleration);
            this.Orientation = new global::STRUCT.Orientation8(other.Orientation);
            this.Information = new global::STRUCT.ObjectInfo4(other.Information);
            this.JammingStatus = new global::STRUCT.Jamming4(other.JammingStatus);
            this.Status = other.Status;
            this.MeanRCS = other.MeanRCS;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Position);
            hash.Add(this.Velocity);
            hash.Add(this.Acceleration);
            hash.Add(this.Orientation);
            hash.Add(this.Information);
            hash.Add(this.JammingStatus);
            hash.Add(this.Status);
            hash.Add(this.MeanRCS);

            return hash.ToHashCode();
        }

        public bool Equals(AirThreat8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Position.Equals(other.Position) && 
            this.Velocity.Equals(other.Velocity) && 
            this.Acceleration.Equals(other.Acceleration) && 
            this.Orientation.Equals(other.Orientation) && 
            this.Information.Equals(other.Information) && 
            this.JammingStatus.Equals(other.JammingStatus) && 
            this.Status.Equals(other.Status) && 
            this.MeanRCS.Equals(other.MeanRCS);
        }

        public override bool Equals(object obj) => this.Equals(obj as AirThreat8);

        public override string ToString() => AirThreat8Support.Instance.ToString(this);
    }

    public class Munition8 :  IEquatable<Munition8>
    {
        public global::STRUCT.Position8 Position { get; set; }
        public global::STRUCT.Velocity8 Velocity { get; set; }
        public global::STRUCT.Acceleration8 Acceleration { get; set; }
        public global::STRUCT.Orientation8 Orientation { get; set; }
        public global::STRUCT.ObjectInfo4 Information { get; set; }
        public global::ENUM.MunitionStatus Status { get; set; }
        public global::ENUM.SeekerPhase SeekerStatus { get; set; }
        public global::ENUM.MunitionType MissileType { get; set; }
        public ushort LockOnThreatObjectNumber { get; set; }
        public ushort MeanRCS { get; set; }
        public ushort SignalPower { get; set; }
        public ushort AddressKey { get; set; }

        public Munition8()
        {
            Position = new global::STRUCT.Position8();
            Velocity = new global::STRUCT.Velocity8();
            Acceleration = new global::STRUCT.Acceleration8();
            Orientation = new global::STRUCT.Orientation8();
            Information = new global::STRUCT.ObjectInfo4();
            Status = (global::ENUM.MunitionStatus) (0);
            SeekerStatus = (global::ENUM.SeekerPhase) (0);
            MissileType = (global::ENUM.MunitionType) (0);
        }

        public Munition8(global::STRUCT.Position8  Position, global::STRUCT.Velocity8  Velocity, global::STRUCT.Acceleration8  Acceleration, global::STRUCT.Orientation8  Orientation, global::STRUCT.ObjectInfo4  Information, global::ENUM.MunitionStatus  Status, global::ENUM.SeekerPhase  SeekerStatus, global::ENUM.MunitionType  MissileType, ushort  LockOnThreatObjectNumber, ushort  MeanRCS, ushort  SignalPower, ushort  AddressKey)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Acceleration = Acceleration;
            this.Orientation = Orientation;
            this.Information = Information;
            this.Status = Status;
            this.SeekerStatus = SeekerStatus;
            this.MissileType = MissileType;
            this.LockOnThreatObjectNumber = LockOnThreatObjectNumber;
            this.MeanRCS = MeanRCS;
            this.SignalPower = SignalPower;
            this.AddressKey = AddressKey;
        }

        public Munition8(Munition8 other)
        {
            if (other == null)
            {
                return;
            }

            this.Position = new global::STRUCT.Position8(other.Position);
            this.Velocity = new global::STRUCT.Velocity8(other.Velocity);
            this.Acceleration = new global::STRUCT.Acceleration8(other.Acceleration);
            this.Orientation = new global::STRUCT.Orientation8(other.Orientation);
            this.Information = new global::STRUCT.ObjectInfo4(other.Information);
            this.Status = other.Status;
            this.SeekerStatus = other.SeekerStatus;
            this.MissileType = other.MissileType;
            this.LockOnThreatObjectNumber = other.LockOnThreatObjectNumber;
            this.MeanRCS = other.MeanRCS;
            this.SignalPower = other.SignalPower;
            this.AddressKey = other.AddressKey;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Position);
            hash.Add(this.Velocity);
            hash.Add(this.Acceleration);
            hash.Add(this.Orientation);
            hash.Add(this.Information);
            hash.Add(this.Status);
            hash.Add(this.SeekerStatus);
            hash.Add(this.MissileType);
            hash.Add(this.LockOnThreatObjectNumber);
            hash.Add(this.MeanRCS);
            hash.Add(this.SignalPower);
            hash.Add(this.AddressKey);

            return hash.ToHashCode();
        }

        public bool Equals(Munition8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Position.Equals(other.Position) && 
            this.Velocity.Equals(other.Velocity) && 
            this.Acceleration.Equals(other.Acceleration) && 
            this.Orientation.Equals(other.Orientation) && 
            this.Information.Equals(other.Information) && 
            this.Status.Equals(other.Status) && 
            this.SeekerStatus.Equals(other.SeekerStatus) && 
            this.MissileType.Equals(other.MissileType) && 
            this.LockOnThreatObjectNumber.Equals(other.LockOnThreatObjectNumber) && 
            this.MeanRCS.Equals(other.MeanRCS) && 
            this.SignalPower.Equals(other.SignalPower) && 
            this.AddressKey.Equals(other.AddressKey);
        }

        public override bool Equals(object obj) => this.Equals(obj as Munition8);

        public override string ToString() => Munition8Support.Instance.ToString(this);
    }

    public class GroundVehicle8 :  IEquatable<GroundVehicle8>
    {
        public global::STRUCT.Position8 Position { get; set; }
        public global::STRUCT.Velocity8 Velocity { get; set; }
        public global::STRUCT.Acceleration8 Acceleration { get; set; }
        public global::STRUCT.Orientation8 Orientation { get; set; }
        public global::STRUCT.MFRStatus8 MFRStatus { get; set; }
        public global::STRUCT.ObjectInfo4 Information { get; set; }
        public global::STRUCT.LCHRStatus2 LauncherStatus { get; set; }

        public GroundVehicle8()
        {
            Position = new global::STRUCT.Position8();
            Velocity = new global::STRUCT.Velocity8();
            Acceleration = new global::STRUCT.Acceleration8();
            Orientation = new global::STRUCT.Orientation8();
            MFRStatus = new global::STRUCT.MFRStatus8();
            Information = new global::STRUCT.ObjectInfo4();
            LauncherStatus = new global::STRUCT.LCHRStatus2();
        }

        public GroundVehicle8(global::STRUCT.Position8  Position, global::STRUCT.Velocity8  Velocity, global::STRUCT.Acceleration8  Acceleration, global::STRUCT.Orientation8  Orientation, global::STRUCT.MFRStatus8  MFRStatus, global::STRUCT.ObjectInfo4  Information, global::STRUCT.LCHRStatus2  LauncherStatus)
        {
            this.Position = Position;
            this.Velocity = Velocity;
            this.Acceleration = Acceleration;
            this.Orientation = Orientation;
            this.MFRStatus = MFRStatus;
            this.Information = Information;
            this.LauncherStatus = LauncherStatus;
        }

        public GroundVehicle8(GroundVehicle8 other)
        {
            if (other == null)
            {
                return;
            }

            this.Position = new global::STRUCT.Position8(other.Position);
            this.Velocity = new global::STRUCT.Velocity8(other.Velocity);
            this.Acceleration = new global::STRUCT.Acceleration8(other.Acceleration);
            this.Orientation = new global::STRUCT.Orientation8(other.Orientation);
            this.MFRStatus = new global::STRUCT.MFRStatus8(other.MFRStatus);
            this.Information = new global::STRUCT.ObjectInfo4(other.Information);
            this.LauncherStatus = new global::STRUCT.LCHRStatus2(other.LauncherStatus);

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Position);
            hash.Add(this.Velocity);
            hash.Add(this.Acceleration);
            hash.Add(this.Orientation);
            hash.Add(this.MFRStatus);
            hash.Add(this.Information);
            hash.Add(this.LauncherStatus);

            return hash.ToHashCode();
        }

        public bool Equals(GroundVehicle8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Position.Equals(other.Position) && 
            this.Velocity.Equals(other.Velocity) && 
            this.Acceleration.Equals(other.Acceleration) && 
            this.Orientation.Equals(other.Orientation) && 
            this.MFRStatus.Equals(other.MFRStatus) && 
            this.Information.Equals(other.Information) && 
            this.LauncherStatus.Equals(other.LauncherStatus);
        }

        public override bool Equals(object obj) => this.Equals(obj as GroundVehicle8);

        public override string ToString() => GroundVehicle8Support.Instance.ToString(this);
    }

    public class Uplink2 :  IEquatable<Uplink2>
    {
        public ushort AddressKey { get; set; }
        public ushort MissileType { get; set; }
        public ushort[] Data { get; set; }

        public Uplink2()
        {
            Data = new ushort[26];
            for( int i1 = 0; i1 < 26; i1++)
            {
                Data[i1] = (0);
            }
            ;
        }

        public Uplink2(ushort  AddressKey, ushort  MissileType, ushort [] Data)
        {
            this.AddressKey = AddressKey;
            this.MissileType = MissileType;
            this.Data = Data;
        }

        public Uplink2(Uplink2 other)
        {
            if (other == null)
            {
                return;
            }

            this.AddressKey = other.AddressKey;
            this.MissileType = other.MissileType;
            this.Data = new ushort[26];
            for( int i1 = 0; i1 < 26; i1++)
            {
                Data[i1] = other.Data[i1];
            }

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.AddressKey);
            hash.Add(this.MissileType);
            hash.Add(this.Data[0]);

            return hash.ToHashCode();
        }

        public bool Equals(Uplink2 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.AddressKey.Equals(other.AddressKey) && 
            this.MissileType.Equals(other.MissileType) && 
            this.Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object obj) => this.Equals(obj as Uplink2);

        public override string ToString() => Uplink2Support.Instance.ToString(this);
    }

    public class Downlink2 :  IEquatable<Downlink2>
    {
        public ushort AddressKey { get; set; }
        public ushort CRC { get; set; }
        public ushort[] Data { get; set; }

        public Downlink2()
        {
            Data = new ushort[24];
            for( int i1 = 0; i1 < 24; i1++)
            {
                Data[i1] = (0);
            }
            ;
        }

        public Downlink2(ushort  AddressKey, ushort  CRC, ushort [] Data)
        {
            this.AddressKey = AddressKey;
            this.CRC = CRC;
            this.Data = Data;
        }

        public Downlink2(Downlink2 other)
        {
            if (other == null)
            {
                return;
            }

            this.AddressKey = other.AddressKey;
            this.CRC = other.CRC;
            this.Data = new ushort[24];
            for( int i1 = 0; i1 < 24; i1++)
            {
                Data[i1] = other.Data[i1];
            }

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.AddressKey);
            hash.Add(this.CRC);
            hash.Add(this.Data[0]);

            return hash.ToHashCode();
        }

        public bool Equals(Downlink2 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.AddressKey.Equals(other.AddressKey) && 
            this.CRC.Equals(other.CRC) && 
            this.Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object obj) => this.Equals(obj as Downlink2);

        public override string ToString() => Downlink2Support.Instance.ToString(this);
    }

    public class TimeTick8 :  IEquatable<TimeTick8>
    {
        public ulong TimeTick { get; set; }
        public uint SyncCycle { get; set; }

        public TimeTick8()
        {
        }

        public TimeTick8(ulong  TimeTick, uint  SyncCycle)
        {
            this.TimeTick = TimeTick;
            this.SyncCycle = SyncCycle;
        }

        public TimeTick8(TimeTick8 other)
        {
            if (other == null)
            {
                return;
            }

            this.TimeTick = other.TimeTick;
            this.SyncCycle = other.SyncCycle;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.TimeTick);
            hash.Add(this.SyncCycle);

            return hash.ToHashCode();
        }

        public bool Equals(TimeTick8 other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.TimeTick.Equals(other.TimeTick) && 
            this.SyncCycle.Equals(other.SyncCycle);
        }

        public override bool Equals(object obj) => this.Equals(obj as TimeTick8);

        public override string ToString() => TimeTick8Support.Instance.ToString(this);
    }

} // namespace STRUCT
namespace MSG
{

    public class AirThreatInformation :  IEquatable<AirThreatInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public ushort AirCount { get; set; }
        [Bound(900)]
        public ISequence<global::STRUCT.AirThreat8> AirObjects { get; }

        public AirThreatInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            AirObjects = new Rti.Types.Sequence<global::STRUCT.AirThreat8>();
        }

        public AirThreatInformation(global::STRUCT.MessageHeader8  Header, ushort  AirCount, ISequence<global::STRUCT.AirThreat8>AirObjects)
        {
            this.Header = Header;
            this.AirCount = AirCount;
            this.AirObjects = AirObjects;
        }

        public AirThreatInformation(AirThreatInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.AirCount = other.AirCount;
            this.AirObjects = new Rti.Types.Sequence<global::STRUCT.AirThreat8>(other.AirObjects.Select(element => new global::STRUCT.AirThreat8(element)));

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.AirCount);
            hash.Add(this.AirObjects.Count);

            return hash.ToHashCode();
        }

        public bool Equals(AirThreatInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.AirCount.Equals(other.AirCount) && 
            this.AirObjects.SequenceEqual(other.AirObjects);
        }

        public override bool Equals(object obj) => this.Equals(obj as AirThreatInformation);

        public override string ToString() => AirThreatInformationSupport.Instance.ToString(this);
    }

    public class MunitionInformation :  IEquatable<MunitionInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public ushort MunitionCount { get; set; }
        [Bound(900)]
        public ISequence<global::STRUCT.Munition8> MunitionObjects { get; }

        public MunitionInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            MunitionObjects = new Rti.Types.Sequence<global::STRUCT.Munition8>();
        }

        public MunitionInformation(global::STRUCT.MessageHeader8  Header, ushort  MunitionCount, ISequence<global::STRUCT.Munition8>MunitionObjects)
        {
            this.Header = Header;
            this.MunitionCount = MunitionCount;
            this.MunitionObjects = MunitionObjects;
        }

        public MunitionInformation(MunitionInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.MunitionCount = other.MunitionCount;
            this.MunitionObjects = new Rti.Types.Sequence<global::STRUCT.Munition8>(other.MunitionObjects.Select(element => new global::STRUCT.Munition8(element)));

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.MunitionCount);
            hash.Add(this.MunitionObjects.Count);

            return hash.ToHashCode();
        }

        public bool Equals(MunitionInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.MunitionCount.Equals(other.MunitionCount) && 
            this.MunitionObjects.SequenceEqual(other.MunitionObjects);
        }

        public override bool Equals(object obj) => this.Equals(obj as MunitionInformation);

        public override string ToString() => MunitionInformationSupport.Instance.ToString(this);
    }

    public class GroundVehicleInformation :  IEquatable<GroundVehicleInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public ushort GroundVehicleCount { get; set; }
        [Bound(100)]
        public ISequence<global::STRUCT.GroundVehicle8> GroundVehicleObjects { get; }

        public GroundVehicleInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            GroundVehicleObjects = new Rti.Types.Sequence<global::STRUCT.GroundVehicle8>();
        }

        public GroundVehicleInformation(global::STRUCT.MessageHeader8  Header, ushort  GroundVehicleCount, ISequence<global::STRUCT.GroundVehicle8>GroundVehicleObjects)
        {
            this.Header = Header;
            this.GroundVehicleCount = GroundVehicleCount;
            this.GroundVehicleObjects = GroundVehicleObjects;
        }

        public GroundVehicleInformation(GroundVehicleInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.GroundVehicleCount = other.GroundVehicleCount;
            this.GroundVehicleObjects = new Rti.Types.Sequence<global::STRUCT.GroundVehicle8>(other.GroundVehicleObjects.Select(element => new global::STRUCT.GroundVehicle8(element)));

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.GroundVehicleCount);
            hash.Add(this.GroundVehicleObjects.Count);

            return hash.ToHashCode();
        }

        public bool Equals(GroundVehicleInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.GroundVehicleCount.Equals(other.GroundVehicleCount) && 
            this.GroundVehicleObjects.SequenceEqual(other.GroundVehicleObjects);
        }

        public override bool Equals(object obj) => this.Equals(obj as GroundVehicleInformation);

        public override string ToString() => GroundVehicleInformationSupport.Instance.ToString(this);
    }

    public class UplinkInformation :  IEquatable<UplinkInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public ushort UplinkCount { get; set; }
        [Bound(56)]
        public ISequence<global::STRUCT.Uplink2> UplinkMessages { get; }

        public UplinkInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            UplinkMessages = new Rti.Types.Sequence<global::STRUCT.Uplink2>();
        }

        public UplinkInformation(global::STRUCT.MessageHeader8  Header, ushort  UplinkCount, ISequence<global::STRUCT.Uplink2>UplinkMessages)
        {
            this.Header = Header;
            this.UplinkCount = UplinkCount;
            this.UplinkMessages = UplinkMessages;
        }

        public UplinkInformation(UplinkInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.UplinkCount = other.UplinkCount;
            this.UplinkMessages = new Rti.Types.Sequence<global::STRUCT.Uplink2>(other.UplinkMessages.Select(element => new global::STRUCT.Uplink2(element)));

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.UplinkCount);
            hash.Add(this.UplinkMessages.Count);

            return hash.ToHashCode();
        }

        public bool Equals(UplinkInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.UplinkCount.Equals(other.UplinkCount) && 
            this.UplinkMessages.SequenceEqual(other.UplinkMessages);
        }

        public override bool Equals(object obj) => this.Equals(obj as UplinkInformation);

        public override string ToString() => UplinkInformationSupport.Instance.ToString(this);
    }

    public class DownlinkInformation :  IEquatable<DownlinkInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public ushort DownlinkCount { get; set; }
        [Bound(56)]
        public ISequence<global::STRUCT.Downlink2> DownlinkMessage { get; }

        public DownlinkInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            DownlinkMessage = new Rti.Types.Sequence<global::STRUCT.Downlink2>();
        }

        public DownlinkInformation(global::STRUCT.MessageHeader8  Header, ushort  DownlinkCount, ISequence<global::STRUCT.Downlink2>DownlinkMessage)
        {
            this.Header = Header;
            this.DownlinkCount = DownlinkCount;
            this.DownlinkMessage = DownlinkMessage;
        }

        public DownlinkInformation(DownlinkInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.DownlinkCount = other.DownlinkCount;
            this.DownlinkMessage = new Rti.Types.Sequence<global::STRUCT.Downlink2>(other.DownlinkMessage.Select(element => new global::STRUCT.Downlink2(element)));

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.DownlinkCount);
            hash.Add(this.DownlinkMessage.Count);

            return hash.ToHashCode();
        }

        public bool Equals(DownlinkInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.DownlinkCount.Equals(other.DownlinkCount) && 
            this.DownlinkMessage.SequenceEqual(other.DownlinkMessage);
        }

        public override bool Equals(object obj) => this.Equals(obj as DownlinkInformation);

        public override string ToString() => DownlinkInformationSupport.Instance.ToString(this);
    }

    public class TimeTickInformation :  IEquatable<TimeTickInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public global::STRUCT.TimeTick8 TimeTickMessage { get; set; }

        public TimeTickInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            TimeTickMessage = new global::STRUCT.TimeTick8();
        }

        public TimeTickInformation(global::STRUCT.MessageHeader8  Header, global::STRUCT.TimeTick8  TimeTickMessage)
        {
            this.Header = Header;
            this.TimeTickMessage = TimeTickMessage;
        }

        public TimeTickInformation(TimeTickInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.TimeTickMessage = new global::STRUCT.TimeTick8(other.TimeTickMessage);

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.TimeTickMessage);

            return hash.ToHashCode();
        }

        public bool Equals(TimeTickInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.TimeTickMessage.Equals(other.TimeTickMessage);
        }

        public override bool Equals(object obj) => this.Equals(obj as TimeTickInformation);

        public override string ToString() => TimeTickInformationSupport.Instance.ToString(this);
    }

    public class SimulatorStatus :  IEquatable<SimulatorStatus>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }

        public SimulatorStatus()
        {
            Header = new global::STRUCT.MessageHeader8();
        }

        public SimulatorStatus(global::STRUCT.MessageHeader8  Header)
        {
            this.Header = Header;
        }

        public SimulatorStatus(SimulatorStatus other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);

            return hash.ToHashCode();
        }

        public bool Equals(SimulatorStatus other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header);
        }

        public override bool Equals(object obj) => this.Equals(obj as SimulatorStatus);

        public override string ToString() => SimulatorStatusSupport.Instance.ToString(this);
    }

    public class WeaponFire :  IEquatable<WeaponFire>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public global::STRUCT.Position8 MFRPosition { get; set; }
        public global::STRUCT.AER8 MFRAntennaAER { get; set; }
        public global::ENUM.MunitionType MunitionType { get; set; }
        public ushort MunitionObjectNumber { get; set; }
        public ushort MunitionAddressKey { get; set; }
        public ushort EDCode { get; set; }
        public ushort IPGroundType { get; set; }
        public ushort IsForcedFire { get; set; }
        public ushort[] EncodingKeyTable { get; set; }
        public byte LSSNumber { get; set; }
        public bool EncodingKeyFlag { get; set; }

        public WeaponFire()
        {
            Header = new global::STRUCT.MessageHeader8();
            MFRPosition = new global::STRUCT.Position8();
            MFRAntennaAER = new global::STRUCT.AER8();
            MunitionType = (global::ENUM.MunitionType) (0);
            EncodingKeyTable = new ushort[55];
            for( int i1 = 0; i1 < 55; i1++)
            {
                EncodingKeyTable[i1] = (0);
            }
            ;
        }

        public WeaponFire(global::STRUCT.MessageHeader8  Header, global::STRUCT.Position8  MFRPosition, global::STRUCT.AER8  MFRAntennaAER, global::ENUM.MunitionType  MunitionType, ushort  MunitionObjectNumber, ushort  MunitionAddressKey, ushort  EDCode, ushort  IPGroundType, ushort  IsForcedFire, ushort [] EncodingKeyTable, byte  LSSNumber, bool  EncodingKeyFlag)
        {
            this.Header = Header;
            this.MFRPosition = MFRPosition;
            this.MFRAntennaAER = MFRAntennaAER;
            this.MunitionType = MunitionType;
            this.MunitionObjectNumber = MunitionObjectNumber;
            this.MunitionAddressKey = MunitionAddressKey;
            this.EDCode = EDCode;
            this.IPGroundType = IPGroundType;
            this.IsForcedFire = IsForcedFire;
            this.EncodingKeyTable = EncodingKeyTable;
            this.LSSNumber = LSSNumber;
            this.EncodingKeyFlag = EncodingKeyFlag;
        }

        public WeaponFire(WeaponFire other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.MFRPosition = new global::STRUCT.Position8(other.MFRPosition);
            this.MFRAntennaAER = new global::STRUCT.AER8(other.MFRAntennaAER);
            this.MunitionType = other.MunitionType;
            this.MunitionObjectNumber = other.MunitionObjectNumber;
            this.MunitionAddressKey = other.MunitionAddressKey;
            this.EDCode = other.EDCode;
            this.IPGroundType = other.IPGroundType;
            this.IsForcedFire = other.IsForcedFire;
            this.EncodingKeyTable = new ushort[55];
            for( int i1 = 0; i1 < 55; i1++)
            {
                EncodingKeyTable[i1] = other.EncodingKeyTable[i1];
            }
            this.LSSNumber = other.LSSNumber;
            this.EncodingKeyFlag = other.EncodingKeyFlag;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.MFRPosition);
            hash.Add(this.MFRAntennaAER);
            hash.Add(this.MunitionType);
            hash.Add(this.MunitionObjectNumber);
            hash.Add(this.MunitionAddressKey);
            hash.Add(this.EDCode);
            hash.Add(this.IPGroundType);
            hash.Add(this.IsForcedFire);
            hash.Add(this.EncodingKeyTable[0]);
            hash.Add(this.LSSNumber);
            hash.Add(this.EncodingKeyFlag);

            return hash.ToHashCode();
        }

        public bool Equals(WeaponFire other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.MFRPosition.Equals(other.MFRPosition) && 
            this.MFRAntennaAER.Equals(other.MFRAntennaAER) && 
            this.MunitionType.Equals(other.MunitionType) && 
            this.MunitionObjectNumber.Equals(other.MunitionObjectNumber) && 
            this.MunitionAddressKey.Equals(other.MunitionAddressKey) && 
            this.EDCode.Equals(other.EDCode) && 
            this.IPGroundType.Equals(other.IPGroundType) && 
            this.IsForcedFire.Equals(other.IsForcedFire) && 
            this.EncodingKeyTable.SequenceEqual(other.EncodingKeyTable) && 
            this.LSSNumber.Equals(other.LSSNumber) && 
            this.EncodingKeyFlag.Equals(other.EncodingKeyFlag);
        }

        public override bool Equals(object obj) => this.Equals(obj as WeaponFire);

        public override string ToString() => WeaponFireSupport.Instance.ToString(this);
    }

    public class TargetInformation :  IEquatable<TargetInformation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public global::STRUCT.Position8 TargetPosition { get; set; }
        public global::STRUCT.Velocity8 TargetVelocity { get; set; }
        public global::STRUCT.Position8 IPPosition { get; set; }
        public global::STRUCT.Velocity8 IPVelocity { get; set; }
        public global::ENUM.ThreatType TargetType { get; set; }
        public global::STRUCT.StandardDeviation2 TargetPositionSTD { get; set; }
        public global::STRUCT.StandardDeviation2 TargetVelocitySTD { get; set; }
        public ushort MunitionObjectNumber { get; set; }
        public ushort DelayTime { get; set; }
        public byte LSSNumber { get; set; }

        public TargetInformation()
        {
            Header = new global::STRUCT.MessageHeader8();
            TargetPosition = new global::STRUCT.Position8();
            TargetVelocity = new global::STRUCT.Velocity8();
            IPPosition = new global::STRUCT.Position8();
            IPVelocity = new global::STRUCT.Velocity8();
            TargetType = (global::ENUM.ThreatType) (0);
            TargetPositionSTD = new global::STRUCT.StandardDeviation2();
            TargetVelocitySTD = new global::STRUCT.StandardDeviation2();
        }

        public TargetInformation(global::STRUCT.MessageHeader8  Header, global::STRUCT.Position8  TargetPosition, global::STRUCT.Velocity8  TargetVelocity, global::STRUCT.Position8  IPPosition, global::STRUCT.Velocity8  IPVelocity, global::ENUM.ThreatType  TargetType, global::STRUCT.StandardDeviation2  TargetPositionSTD, global::STRUCT.StandardDeviation2  TargetVelocitySTD, ushort  MunitionObjectNumber, ushort  DelayTime, byte  LSSNumber)
        {
            this.Header = Header;
            this.TargetPosition = TargetPosition;
            this.TargetVelocity = TargetVelocity;
            this.IPPosition = IPPosition;
            this.IPVelocity = IPVelocity;
            this.TargetType = TargetType;
            this.TargetPositionSTD = TargetPositionSTD;
            this.TargetVelocitySTD = TargetVelocitySTD;
            this.MunitionObjectNumber = MunitionObjectNumber;
            this.DelayTime = DelayTime;
            this.LSSNumber = LSSNumber;
        }

        public TargetInformation(TargetInformation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.TargetPosition = new global::STRUCT.Position8(other.TargetPosition);
            this.TargetVelocity = new global::STRUCT.Velocity8(other.TargetVelocity);
            this.IPPosition = new global::STRUCT.Position8(other.IPPosition);
            this.IPVelocity = new global::STRUCT.Velocity8(other.IPVelocity);
            this.TargetType = other.TargetType;
            this.TargetPositionSTD = new global::STRUCT.StandardDeviation2(other.TargetPositionSTD);
            this.TargetVelocitySTD = new global::STRUCT.StandardDeviation2(other.TargetVelocitySTD);
            this.MunitionObjectNumber = other.MunitionObjectNumber;
            this.DelayTime = other.DelayTime;
            this.LSSNumber = other.LSSNumber;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.TargetPosition);
            hash.Add(this.TargetVelocity);
            hash.Add(this.IPPosition);
            hash.Add(this.IPVelocity);
            hash.Add(this.TargetType);
            hash.Add(this.TargetPositionSTD);
            hash.Add(this.TargetVelocitySTD);
            hash.Add(this.MunitionObjectNumber);
            hash.Add(this.DelayTime);
            hash.Add(this.LSSNumber);

            return hash.ToHashCode();
        }

        public bool Equals(TargetInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.TargetPosition.Equals(other.TargetPosition) && 
            this.TargetVelocity.Equals(other.TargetVelocity) && 
            this.IPPosition.Equals(other.IPPosition) && 
            this.IPVelocity.Equals(other.IPVelocity) && 
            this.TargetType.Equals(other.TargetType) && 
            this.TargetPositionSTD.Equals(other.TargetPositionSTD) && 
            this.TargetVelocitySTD.Equals(other.TargetVelocitySTD) && 
            this.MunitionObjectNumber.Equals(other.MunitionObjectNumber) && 
            this.DelayTime.Equals(other.DelayTime) && 
            this.LSSNumber.Equals(other.LSSNumber);
        }

        public override bool Equals(object obj) => this.Equals(obj as TargetInformation);

        public override string ToString() => TargetInformationSupport.Instance.ToString(this);
    }

    public class TrackNumberInfo :  IEquatable<TrackNumberInfo>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public ushort ObjectNumber { get; set; }
        public ushort RDRTrackNumber { get; set; }
        public ushort ECSTrackNumber { get; set; }
        public ushort Link16TrackNumber { get; set; }
        public ushort MDILTrackNumber { get; set; }

        public TrackNumberInfo()
        {
            Header = new global::STRUCT.MessageHeader8();
        }

        public TrackNumberInfo(global::STRUCT.MessageHeader8  Header, ushort  ObjectNumber, ushort  RDRTrackNumber, ushort  ECSTrackNumber, ushort  Link16TrackNumber, ushort  MDILTrackNumber)
        {
            this.Header = Header;
            this.ObjectNumber = ObjectNumber;
            this.RDRTrackNumber = RDRTrackNumber;
            this.ECSTrackNumber = ECSTrackNumber;
            this.Link16TrackNumber = Link16TrackNumber;
            this.MDILTrackNumber = MDILTrackNumber;
        }

        public TrackNumberInfo(TrackNumberInfo other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.ObjectNumber = other.ObjectNumber;
            this.RDRTrackNumber = other.RDRTrackNumber;
            this.ECSTrackNumber = other.ECSTrackNumber;
            this.Link16TrackNumber = other.Link16TrackNumber;
            this.MDILTrackNumber = other.MDILTrackNumber;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.ObjectNumber);
            hash.Add(this.RDRTrackNumber);
            hash.Add(this.ECSTrackNumber);
            hash.Add(this.Link16TrackNumber);
            hash.Add(this.MDILTrackNumber);

            return hash.ToHashCode();
        }

        public bool Equals(TrackNumberInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.ObjectNumber.Equals(other.ObjectNumber) && 
            this.RDRTrackNumber.Equals(other.RDRTrackNumber) && 
            this.ECSTrackNumber.Equals(other.ECSTrackNumber) && 
            this.Link16TrackNumber.Equals(other.Link16TrackNumber) && 
            this.MDILTrackNumber.Equals(other.MDILTrackNumber);
        }

        public override bool Equals(object obj) => this.Equals(obj as TrackNumberInfo);

        public override string ToString() => TrackNumberInfoSupport.Instance.ToString(this);
    }

    public class SetLog :  IEquatable<SetLog>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        [Bound(20)]
        public ISequence<global::ENUM.NetworkID> LogSaveNetworkList { get; }

        public SetLog()
        {
            Header = new global::STRUCT.MessageHeader8();
            LogSaveNetworkList = new Rti.Types.Sequence<global::ENUM.NetworkID>();
        }

        public SetLog(global::STRUCT.MessageHeader8  Header, ISequence<global::ENUM.NetworkID>LogSaveNetworkList)
        {
            this.Header = Header;
            this.LogSaveNetworkList = LogSaveNetworkList;
        }

        public SetLog(SetLog other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.LogSaveNetworkList = new Rti.Types.Sequence<global::ENUM.NetworkID>(other.LogSaveNetworkList);

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.LogSaveNetworkList.Count);

            return hash.ToHashCode();
        }

        public bool Equals(SetLog other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.LogSaveNetworkList.SequenceEqual(other.LogSaveNetworkList);
        }

        public override bool Equals(object obj) => this.Equals(obj as SetLog);

        public override string ToString() => SetLogSupport.Instance.ToString(this);
    }

    public class SetConfiguration :  IEquatable<SetConfiguration>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        [Bound(50)]
        public ISequence<global::STRUCT.Config8> Config { get; }

        public SetConfiguration()
        {
            Header = new global::STRUCT.MessageHeader8();
            Config = new Rti.Types.Sequence<global::STRUCT.Config8>();
        }

        public SetConfiguration(global::STRUCT.MessageHeader8  Header, ISequence<global::STRUCT.Config8>Config)
        {
            this.Header = Header;
            this.Config = Config;
        }

        public SetConfiguration(SetConfiguration other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.Config = new Rti.Types.Sequence<global::STRUCT.Config8>(other.Config.Select(element => new global::STRUCT.Config8(element)));

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.Config.Count);

            return hash.ToHashCode();
        }

        public bool Equals(SetConfiguration other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.Config.SequenceEqual(other.Config);
        }

        public override bool Equals(object obj) => this.Equals(obj as SetConfiguration);

        public override string ToString() => SetConfigurationSupport.Instance.ToString(this);
    }

    public class SetScenario :  IEquatable<SetScenario>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        [Bound(255)]
        public string ScenarioName { get; set; } = string.Empty;
        [Bound(255)]
        public string ScenarioVersion { get; set; } = string.Empty;

        public SetScenario()
        {
            Header = new global::STRUCT.MessageHeader8();
        }

        public SetScenario(global::STRUCT.MessageHeader8  Header, string  ScenarioName, string  ScenarioVersion)
        {
            this.Header = Header;
            this.ScenarioName = ScenarioName;
            this.ScenarioVersion = ScenarioVersion;
        }

        public SetScenario(SetScenario other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.ScenarioName = other.ScenarioName;
            this.ScenarioVersion = other.ScenarioVersion;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.ScenarioName);
            hash.Add(this.ScenarioVersion);

            return hash.ToHashCode();
        }

        public bool Equals(SetScenario other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.ScenarioName.Equals(other.ScenarioName) && 
            this.ScenarioVersion.Equals(other.ScenarioVersion);
        }

        public override bool Equals(object obj) => this.Equals(obj as SetScenario);

        public override string ToString() => SetScenarioSupport.Instance.ToString(this);
    }

    public class SetSimulation :  IEquatable<SetSimulation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public global::ENUM.SimulationStatus SetSim { get; set; }

        public SetSimulation()
        {
            Header = new global::STRUCT.MessageHeader8();
            SetSim = (global::ENUM.SimulationStatus) (0);
        }

        public SetSimulation(global::STRUCT.MessageHeader8  Header, global::ENUM.SimulationStatus  SetSim)
        {
            this.Header = Header;
            this.SetSim = SetSim;
        }

        public SetSimulation(SetSimulation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.SetSim = other.SetSim;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.SetSim);

            return hash.ToHashCode();
        }

        public bool Equals(SetSimulation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.SetSim.Equals(other.SetSim);
        }

        public override bool Equals(object obj) => this.Equals(obj as SetSimulation);

        public override string ToString() => SetSimulationSupport.Instance.ToString(this);
    }

    public class Collision :  IEquatable<Collision>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public global::STRUCT.RelativePosition8 RelativePosition { get; set; }
        public global::ENUM.CollisionStatus CollisionType { get; set; }

        public Collision()
        {
            Header = new global::STRUCT.MessageHeader8();
            RelativePosition = new global::STRUCT.RelativePosition8();
            CollisionType = (global::ENUM.CollisionStatus) (0);
        }

        public Collision(global::STRUCT.MessageHeader8  Header, global::STRUCT.RelativePosition8  RelativePosition, global::ENUM.CollisionStatus  CollisionType)
        {
            this.Header = Header;
            this.RelativePosition = RelativePosition;
            this.CollisionType = CollisionType;
        }

        public Collision(Collision other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.RelativePosition = new global::STRUCT.RelativePosition8(other.RelativePosition);
            this.CollisionType = other.CollisionType;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.RelativePosition);
            hash.Add(this.CollisionType);

            return hash.ToHashCode();
        }

        public bool Equals(Collision other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.RelativePosition.Equals(other.RelativePosition) && 
            this.CollisionType.Equals(other.CollisionType);
        }

        public override bool Equals(object obj) => this.Equals(obj as Collision);

        public override string ToString() => CollisionSupport.Instance.ToString(this);
    }

    public class MunitionDetonation :  IEquatable<MunitionDetonation>
    {
        public global::STRUCT.MessageHeader8 Header { get; set; }
        public global::STRUCT.Position8 DetonationPosition { get; set; }
        public global::STRUCT.Velocity8 FinalVelocity { get; set; }
        public global::STRUCT.RelativePosition8 RelativePosition { get; set; }
        public global::ENUM.DetonationType DetonationResult { get; set; }
        public ushort MunitionObjectNumber { get; set; }
        public ushort ThreatObjectNumber { get; set; }

        public MunitionDetonation()
        {
            Header = new global::STRUCT.MessageHeader8();
            DetonationPosition = new global::STRUCT.Position8();
            FinalVelocity = new global::STRUCT.Velocity8();
            RelativePosition = new global::STRUCT.RelativePosition8();
            DetonationResult = (global::ENUM.DetonationType) (0);
        }

        public MunitionDetonation(global::STRUCT.MessageHeader8  Header, global::STRUCT.Position8  DetonationPosition, global::STRUCT.Velocity8  FinalVelocity, global::STRUCT.RelativePosition8  RelativePosition, global::ENUM.DetonationType  DetonationResult, ushort  MunitionObjectNumber, ushort  ThreatObjectNumber)
        {
            this.Header = Header;
            this.DetonationPosition = DetonationPosition;
            this.FinalVelocity = FinalVelocity;
            this.RelativePosition = RelativePosition;
            this.DetonationResult = DetonationResult;
            this.MunitionObjectNumber = MunitionObjectNumber;
            this.ThreatObjectNumber = ThreatObjectNumber;
        }

        public MunitionDetonation(MunitionDetonation other)
        {
            if (other == null)
            {
                return;
            }

            this.Header = new global::STRUCT.MessageHeader8(other.Header);
            this.DetonationPosition = new global::STRUCT.Position8(other.DetonationPosition);
            this.FinalVelocity = new global::STRUCT.Velocity8(other.FinalVelocity);
            this.RelativePosition = new global::STRUCT.RelativePosition8(other.RelativePosition);
            this.DetonationResult = other.DetonationResult;
            this.MunitionObjectNumber = other.MunitionObjectNumber;
            this.ThreatObjectNumber = other.ThreatObjectNumber;

        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(this.Header);
            hash.Add(this.DetonationPosition);
            hash.Add(this.FinalVelocity);
            hash.Add(this.RelativePosition);
            hash.Add(this.DetonationResult);
            hash.Add(this.MunitionObjectNumber);
            hash.Add(this.ThreatObjectNumber);

            return hash.ToHashCode();
        }

        public bool Equals(MunitionDetonation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Header.Equals(other.Header) && 
            this.DetonationPosition.Equals(other.DetonationPosition) && 
            this.FinalVelocity.Equals(other.FinalVelocity) && 
            this.RelativePosition.Equals(other.RelativePosition) && 
            this.DetonationResult.Equals(other.DetonationResult) && 
            this.MunitionObjectNumber.Equals(other.MunitionObjectNumber) && 
            this.ThreatObjectNumber.Equals(other.ThreatObjectNumber);
        }

        public override bool Equals(object obj) => this.Equals(obj as MunitionDetonation);

        public override string ToString() => MunitionDetonationSupport.Instance.ToString(this);
    }

} // namespace MSG

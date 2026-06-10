/*
WARNING: THIS FILE IS AUTO-GENERATED. DO NOT MODIFY.

This file was generated from DDSSim.idl
using RTI Code Generator (rtiddsgen) version 4.3.1.
The rtiddsgen tool is part of the RTI Connext DDS distribution.
For more information, type 'rtiddsgen -help' at a command shell
or consult the Code Generator User's Manual.
*/

using System;
using System.Runtime.InteropServices;
using Omg.Types;
using Omg.Types.Dynamic;
using Rti.Types;
using Rti.Dds.Core;
using Rti.Types.Dynamic;
using Rti.Dds.NativeInterface.TypePlugin;

namespace ENUM
{

    namespace Implementation
    {
        internal class ForceIdentifierPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public ForceIdentifierPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::ForceIdentifier")
                .AddMember(new EnumMember("Other", 0))
                .AddMember(new EnumMember("Friend", 1))
                .AddMember(new EnumMember("Hostile", 2))
                .AddMember(new EnumMember("Neutral", 3))
                .AddMember(new EnumMember("SureFriend", 4))
                .AddMember(new EnumMember("Unknown", 5))
                .AddMember(new EnumMember("Suspect", 6))
                .AddMember(new EnumMember("ExFriend", 7))
                .AddMember(new EnumMember("Faker", 8))
                .AddMember(new EnumMember("ExNeutral", 9))
                .AddMember(new EnumMember("ExSureFriend", 10))
                .AddMember(new EnumMember("ExUnknown", 11))
                .AddMember(new EnumMember("Joker", 12))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class ForceIdentifierSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.ForceIdentifier>
    {
        public ForceIdentifierSupport() : base(
            new Implementation.ForceIdentifierPlugin(),
            new Lazy<DynamicType>(() =>Implementation.ForceIdentifierPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static ForceIdentifierSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<ForceIdentifierSupport, global::ENUM.ForceIdentifier>();

    }

    namespace Implementation
    {
        internal class JammingTypePlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public JammingTypePlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::JammingType")
                .AddMember(new EnumMember("NoJamming", 0))
                .AddMember(new EnumMember("ANJ", 1))
                .AddMember(new EnumMember("RGPO", 2))
                .AddMember(new EnumMember("VGPO", 3))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class JammingTypeSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.JammingType>
    {
        public JammingTypeSupport() : base(
            new Implementation.JammingTypePlugin(),
            new Lazy<DynamicType>(() =>Implementation.JammingTypePlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static JammingTypeSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<JammingTypeSupport, global::ENUM.JammingType>();

    }

    namespace Implementation
    {
        internal class ATSStatusPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public ATSStatusPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::ATSStatus")
                .AddMember(new EnumMember("NA", 0))
                .AddMember(new EnumMember("Turning", 1))
                .AddMember(new EnumMember("Evasive", 2))
                .AddMember(new EnumMember("Intercept", 3))
                .AddMember(new EnumMember("TBMStart", 11))
                .AddMember(new EnumMember("TBMSeparation", 12))
                .AddMember(new EnumMember("TBMDebris", 13))
                .AddMember(new EnumMember("EndOfFlight", 21))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class ATSStatusSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.ATSStatus>
    {
        public ATSStatusSupport() : base(
            new Implementation.ATSStatusPlugin(),
            new Lazy<DynamicType>(() =>Implementation.ATSStatusPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static ATSStatusSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<ATSStatusSupport, global::ENUM.ATSStatus>();

    }

    namespace Implementation
    {
        internal class MunitionStatusPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public MunitionStatusPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::MunitionStatus")
                .AddMember(new EnumMember("NA", 0))
                .AddMember(new EnumMember("Launch", 1))
                .AddMember(new EnumMember("Separation", 2))
                .AddMember(new EnumMember("InterceptSuccess", 4))
                .AddMember(new EnumMember("SelfDestruction", 5))
                .AddMember(new EnumMember("EmergencyDestruction", 6))
                .AddMember(new EnumMember("TerrainImpact", 7))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class MunitionStatusSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.MunitionStatus>
    {
        public MunitionStatusSupport() : base(
            new Implementation.MunitionStatusPlugin(),
            new Lazy<DynamicType>(() =>Implementation.MunitionStatusPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MunitionStatusSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MunitionStatusSupport, global::ENUM.MunitionStatus>();

    }

    namespace Implementation
    {
        internal class MunitionTypePlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public MunitionTypePlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::MunitionType")
                .AddMember(new EnumMember("NA", 0))
                .AddMember(new EnumMember("AAM", 1))
                .AddMember(new EnumMember("ABM", 2))
                .AddMember(new EnumMember("Booster", 3))
                .AddMember(new EnumMember("Shroud", 4))
                .AddMember(new EnumMember("HABM", 5))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class MunitionTypeSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.MunitionType>
    {
        public MunitionTypeSupport() : base(
            new Implementation.MunitionTypePlugin(),
            new Lazy<DynamicType>(() =>Implementation.MunitionTypePlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MunitionTypeSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MunitionTypeSupport, global::ENUM.MunitionType>();

    }

    namespace Implementation
    {
        internal class ThreatTypePlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public ThreatTypePlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::ThreatType")
                .AddMember(new EnumMember("Air", 0))
                .AddMember(new EnumMember("TBM", 1))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class ThreatTypeSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.ThreatType>
    {
        public ThreatTypeSupport() : base(
            new Implementation.ThreatTypePlugin(),
            new Lazy<DynamicType>(() =>Implementation.ThreatTypePlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static ThreatTypeSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<ThreatTypeSupport, global::ENUM.ThreatType>();

    }

    namespace Implementation
    {
        internal class SeekerPhasePlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public SeekerPhasePlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::SeekerPhase")
                .AddMember(new EnumMember("NS", 0))
                .AddMember(new EnumMember("SeekerOn", 1))
                .AddMember(new EnumMember("LockOnTarget", 2))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class SeekerPhaseSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.SeekerPhase>
    {
        public SeekerPhaseSupport() : base(
            new Implementation.SeekerPhasePlugin(),
            new Lazy<DynamicType>(() =>Implementation.SeekerPhasePlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SeekerPhaseSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SeekerPhaseSupport, global::ENUM.SeekerPhase>();

    }

    namespace Implementation
    {
        internal class NetworkIDPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public NetworkIDPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::NetworkID")
                .AddMember(new EnumMember("DDS", 0))
                .AddMember(new EnumMember("RTI", 1))
                .AddMember(new EnumMember("LTE", 2))
                .AddMember(new EnumMember("Link16", 3))
                .AddMember(new EnumMember("MDIL", 4))
                .AddMember(new EnumMember("REG", 5))
                .AddMember(new EnumMember("LCHR", 6))
                .AddMember(new EnumMember("EMS", 7))
                .AddMember(new EnumMember("SIU", 8))
                .AddMember(new EnumMember("CIGP", 9))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class NetworkIDSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.NetworkID>
    {
        public NetworkIDSupport() : base(
            new Implementation.NetworkIDPlugin(),
            new Lazy<DynamicType>(() =>Implementation.NetworkIDPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static NetworkIDSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<NetworkIDSupport, global::ENUM.NetworkID>();

    }

    namespace Implementation
    {
        internal class MessageIDPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public MessageIDPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::MessageID")
                .AddMember(new EnumMember("AirThreatInformation", 1))
                .AddMember(new EnumMember("MunitionInformation", 2))
                .AddMember(new EnumMember("UplinkInformation", 3))
                .AddMember(new EnumMember("DownlinkInformation", 4))
                .AddMember(new EnumMember("TimeTickInformation", 5))
                .AddMember(new EnumMember("GroundVehicleInformation", 6))
                .AddMember(new EnumMember("SimulatorStatus", 7))
                .AddMember(new EnumMember("SetLog", 8))
                .AddMember(new EnumMember("SetConfiguration", 9))
                .AddMember(new EnumMember("SetScenario", 10))
                .AddMember(new EnumMember("SetSimulation", 11))
                .AddMember(new EnumMember("Collision", 12))
                .AddMember(new EnumMember("MunitionDetonation", 13))
                .AddMember(new EnumMember("WeaponFire", 14))
                .AddMember(new EnumMember("TargetInformation", 15))
                .AddMember(new EnumMember("TrackNumberInfo", 16))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class MessageIDSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.MessageID>
    {
        public MessageIDSupport() : base(
            new Implementation.MessageIDPlugin(),
            new Lazy<DynamicType>(() =>Implementation.MessageIDPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MessageIDSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MessageIDSupport, global::ENUM.MessageID>();

    }

    namespace Implementation
    {
        internal class SimulatorIDPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public SimulatorIDPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::SimulatorID")
                .AddMember(new EnumMember("TCC", 1))
                .AddMember(new EnumMember("TDS", 2))
                .AddMember(new EnumMember("VGD", 3))
                .AddMember(new EnumMember("DOLU", 4))
                .AddMember(new EnumMember("ELU", 5))
                .AddMember(new EnumMember("ATS_A", 11))
                .AddMember(new EnumMember("ATS_B", 12))
                .AddMember(new EnumMember("ATS_C", 13))
                .AddMember(new EnumMember("ATS_D", 14))
                .AddMember(new EnumMember("MSS_A", 21))
                .AddMember(new EnumMember("MSS_B", 22))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class SimulatorIDSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.SimulatorID>
    {
        public SimulatorIDSupport() : base(
            new Implementation.SimulatorIDPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SimulatorIDPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SimulatorIDSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SimulatorIDSupport, global::ENUM.SimulatorID>();

    }

    namespace Implementation
    {
        internal class SimulationStatusPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public SimulationStatusPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::SimulationStatus")
                .AddMember(new EnumMember("NA", 0))
                .AddMember(new EnumMember("On", 1))
                .AddMember(new EnumMember("Off", 2))
                .AddMember(new EnumMember("Scenario", 3))
                .AddMember(new EnumMember("Start", 4))
                .AddMember(new EnumMember("Stop", 5))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class SimulationStatusSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.SimulationStatus>
    {
        public SimulationStatusSupport() : base(
            new Implementation.SimulationStatusPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SimulationStatusPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SimulationStatusSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SimulationStatusSupport, global::ENUM.SimulationStatus>();

    }

    namespace Implementation
    {
        internal class CollisionStatusPlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public CollisionStatusPlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::CollisionStatus")
                .AddMember(new EnumMember("InElastic", 0))
                .AddMember(new EnumMember("Elastic", 1))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class CollisionStatusSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.CollisionStatus>
    {
        public CollisionStatusSupport() : base(
            new Implementation.CollisionStatusPlugin(),
            new Lazy<DynamicType>(() =>Implementation.CollisionStatusPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static CollisionStatusSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<CollisionStatusSupport, global::ENUM.CollisionStatus>();

    }

    namespace Implementation
    {
        internal class DetonationTypePlugin : Rti.Dds.NativeInterface.TypePlugin.EnumTypePlugin
        {
            public DetonationTypePlugin() : base(CreateDynamicType(isPublic: false))
            {
            }

            internal static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                return dtf.BuildEnum()
                .WithName("ENUM::DetonationType")
                .AddMember(new EnumMember("NA", 0))
                .AddMember(new EnumMember("ED", 29))
                .AddMember(new EnumMember("SDLockOnFail", 30))
                .AddMember(new EnumMember("SDTimeOut", 31))
                .AddMember(new EnumMember("SDDetectFail", 32))
                .AddMember(new EnumMember("SDHitFail", 33))
                .WithExtensibility(ExtensibilityKind.Extensible)
                .Create();
            }
        }
    }

    public class DetonationTypeSupport : Rti.Dds.Topics.TypeSupport<global::ENUM.DetonationType>
    {
        public DetonationTypeSupport() : base(
            new Implementation.DetonationTypePlugin(),
            new Lazy<DynamicType>(() =>Implementation.DetonationTypePlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static DetonationTypeSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<DetonationTypeSupport, global::ENUM.DetonationType>();

    }

} // namespace ENUM

namespace STRUCT
{

    namespace Implementation
    {

        public struct MessageHeader8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.MessageHeader8>
        {

            private ulong TimeTick;
            private ulong TimeStamp;
            private global::ENUM.MessageID MsgID;
            private global::ENUM.SimulatorID SrcSimID;
            private global::ENUM.SimulatorID DstSimID;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.MessageHeader8 sample, bool keysOnly = false)
            {

                sample.TimeTick = TimeTick;
                sample.TimeStamp = TimeStamp;
                sample.MsgID = MsgID;
                sample.SrcSimID = SrcSimID;
                sample.DstSimID = DstSimID;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                TimeTick = (ulong) (0uL);
                TimeStamp = (ulong) (0uL);
                MsgID = (global::ENUM.MessageID) (1);
                SrcSimID = (global::ENUM.SimulatorID) (1);
                DstSimID = (global::ENUM.SimulatorID) (1);
            }

            public void ToNative(global::STRUCT.MessageHeader8 sample, bool keysOnly = false)
            {
                TimeTick = sample.TimeTick;
                TimeStamp = sample.TimeStamp;
                MsgID = sample.MsgID;
                SrcSimID = sample.SrcSimID;
                DstSimID = sample.DstSimID;
            }
        }

        internal class MessageHeader8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.MessageHeader8, MessageHeader8Unmanaged>
        {

            internal MessageHeader8Plugin() : base("global::STRUCT.MessageHeader8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // MessageHeader8 struct
                var MessageHeader8StructMembers = new StructMember[]
                {
                    new StructMember("TimeTick", dtf.GetPrimitiveType<ulong>(), id: 0),
                    new StructMember("TimeStamp", dtf.GetPrimitiveType<ulong>(), id: 1),
                    new StructMember("MsgID", global::ENUM.MessageIDSupport.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("SrcSimID", global::ENUM.SimulatorIDSupport.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("DstSimID", global::ENUM.SimulatorIDSupport.Instance.GetDynamicTypeInternal(isPublic), id: 4)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<MessageHeader8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::MessageHeader8")
                    .AddMembers(MessageHeader8StructMembers));

                return result;
            }
        }
    }
    public class MessageHeader8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.MessageHeader8>
    {
        public MessageHeader8Support() : base(
            new Implementation.MessageHeader8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.MessageHeader8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MessageHeader8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MessageHeader8Support, global::STRUCT.MessageHeader8>();

    }

    namespace Implementation
    {

        public struct StandardDeviation2Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.StandardDeviation2>
        {

            private ushort X;
            private ushort Y;
            private ushort Z;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.StandardDeviation2 sample, bool keysOnly = false)
            {

                sample.X = X;
                sample.Y = Y;
                sample.Z = Z;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                X = (ushort) (0);
                Y = (ushort) (0);
                Z = (ushort) (0);
            }

            public void ToNative(global::STRUCT.StandardDeviation2 sample, bool keysOnly = false)
            {
                X = sample.X;
                Y = sample.Y;
                Z = sample.Z;
            }
        }

        internal class StandardDeviation2Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.StandardDeviation2, StandardDeviation2Unmanaged>
        {

            internal StandardDeviation2Plugin() : base("global::STRUCT.StandardDeviation2", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // StandardDeviation2 struct
                var StandardDeviation2StructMembers = new StructMember[]
                {
                    new StructMember("X", dtf.GetPrimitiveType<ushort>(), id: 0),
                    new StructMember("Y", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("Z", dtf.GetPrimitiveType<ushort>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<StandardDeviation2Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::StandardDeviation2")
                    .AddMembers(StandardDeviation2StructMembers));

                return result;
            }
        }
    }
    public class StandardDeviation2Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.StandardDeviation2>
    {
        public StandardDeviation2Support() : base(
            new Implementation.StandardDeviation2Plugin(),
            new Lazy<DynamicType>(() =>Implementation.StandardDeviation2Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static StandardDeviation2Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<StandardDeviation2Support, global::STRUCT.StandardDeviation2>();

    }

    namespace Implementation
    {

        public struct Position8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Position8>
        {

            private double LocationX;
            private double LocationY;
            private double LocationZ;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.Position8 sample, bool keysOnly = false)
            {

                sample.LocationX = LocationX;
                sample.LocationY = LocationY;
                sample.LocationZ = LocationZ;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                LocationX = (double) (0.0);
                LocationY = (double) (0.0);
                LocationZ = (double) (0.0);
            }

            public void ToNative(global::STRUCT.Position8 sample, bool keysOnly = false)
            {
                LocationX = sample.LocationX;
                LocationY = sample.LocationY;
                LocationZ = sample.LocationZ;
            }
        }

        internal class Position8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Position8, Position8Unmanaged>
        {

            internal Position8Plugin() : base("global::STRUCT.Position8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Position8 struct
                var Position8StructMembers = new StructMember[]
                {
                    new StructMember("LocationX", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("LocationY", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("LocationZ", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Position8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Position8")
                    .AddMembers(Position8StructMembers));

                return result;
            }
        }
    }
    public class Position8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Position8>
    {
        public Position8Support() : base(
            new Implementation.Position8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Position8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Position8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Position8Support, global::STRUCT.Position8>();

    }

    namespace Implementation
    {

        public struct RelativePosition8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.RelativePosition8>
        {

            private double DistanceX;
            private double DistanceY;
            private double DistanceZ;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.RelativePosition8 sample, bool keysOnly = false)
            {

                sample.DistanceX = DistanceX;
                sample.DistanceY = DistanceY;
                sample.DistanceZ = DistanceZ;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                DistanceX = (double) (0.0);
                DistanceY = (double) (0.0);
                DistanceZ = (double) (0.0);
            }

            public void ToNative(global::STRUCT.RelativePosition8 sample, bool keysOnly = false)
            {
                DistanceX = sample.DistanceX;
                DistanceY = sample.DistanceY;
                DistanceZ = sample.DistanceZ;
            }
        }

        internal class RelativePosition8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.RelativePosition8, RelativePosition8Unmanaged>
        {

            internal RelativePosition8Plugin() : base("global::STRUCT.RelativePosition8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // RelativePosition8 struct
                var RelativePosition8StructMembers = new StructMember[]
                {
                    new StructMember("DistanceX", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("DistanceY", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("DistanceZ", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<RelativePosition8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::RelativePosition8")
                    .AddMembers(RelativePosition8StructMembers));

                return result;
            }
        }
    }
    public class RelativePosition8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.RelativePosition8>
    {
        public RelativePosition8Support() : base(
            new Implementation.RelativePosition8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.RelativePosition8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static RelativePosition8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<RelativePosition8Support, global::STRUCT.RelativePosition8>();

    }

    namespace Implementation
    {

        public struct LLA8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.LLA8>
        {

            private double Latitude;
            private double Longitude;
            private double Altitude;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.LLA8 sample, bool keysOnly = false)
            {

                sample.Latitude = Latitude;
                sample.Longitude = Longitude;
                sample.Altitude = Altitude;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Latitude = (double) (0.0);
                Longitude = (double) (0.0);
                Altitude = (double) (0.0);
            }

            public void ToNative(global::STRUCT.LLA8 sample, bool keysOnly = false)
            {
                Latitude = sample.Latitude;
                Longitude = sample.Longitude;
                Altitude = sample.Altitude;
            }
        }

        internal class LLA8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.LLA8, LLA8Unmanaged>
        {

            internal LLA8Plugin() : base("global::STRUCT.LLA8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // LLA8 struct
                var LLA8StructMembers = new StructMember[]
                {
                    new StructMember("Latitude", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("Longitude", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("Altitude", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<LLA8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::LLA8")
                    .AddMembers(LLA8StructMembers));

                return result;
            }
        }
    }
    public class LLA8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.LLA8>
    {
        public LLA8Support() : base(
            new Implementation.LLA8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.LLA8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static LLA8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<LLA8Support, global::STRUCT.LLA8>();

    }

    namespace Implementation
    {

        public struct Velocity8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Velocity8>
        {

            private double VelocityX;
            private double VelocityY;
            private double VelocityZ;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.Velocity8 sample, bool keysOnly = false)
            {

                sample.VelocityX = VelocityX;
                sample.VelocityY = VelocityY;
                sample.VelocityZ = VelocityZ;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                VelocityX = (double) (0.0);
                VelocityY = (double) (0.0);
                VelocityZ = (double) (0.0);
            }

            public void ToNative(global::STRUCT.Velocity8 sample, bool keysOnly = false)
            {
                VelocityX = sample.VelocityX;
                VelocityY = sample.VelocityY;
                VelocityZ = sample.VelocityZ;
            }
        }

        internal class Velocity8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Velocity8, Velocity8Unmanaged>
        {

            internal Velocity8Plugin() : base("global::STRUCT.Velocity8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Velocity8 struct
                var Velocity8StructMembers = new StructMember[]
                {
                    new StructMember("VelocityX", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("VelocityY", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("VelocityZ", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Velocity8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Velocity8")
                    .AddMembers(Velocity8StructMembers));

                return result;
            }
        }
    }
    public class Velocity8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Velocity8>
    {
        public Velocity8Support() : base(
            new Implementation.Velocity8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Velocity8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Velocity8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Velocity8Support, global::STRUCT.Velocity8>();

    }

    namespace Implementation
    {

        public struct Acceleration8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Acceleration8>
        {

            private double AccelerationX;
            private double AccelerationY;
            private double AccelerationZ;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.Acceleration8 sample, bool keysOnly = false)
            {

                sample.AccelerationX = AccelerationX;
                sample.AccelerationY = AccelerationY;
                sample.AccelerationZ = AccelerationZ;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                AccelerationX = (double) (0.0);
                AccelerationY = (double) (0.0);
                AccelerationZ = (double) (0.0);
            }

            public void ToNative(global::STRUCT.Acceleration8 sample, bool keysOnly = false)
            {
                AccelerationX = sample.AccelerationX;
                AccelerationY = sample.AccelerationY;
                AccelerationZ = sample.AccelerationZ;
            }
        }

        internal class Acceleration8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Acceleration8, Acceleration8Unmanaged>
        {

            internal Acceleration8Plugin() : base("global::STRUCT.Acceleration8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Acceleration8 struct
                var Acceleration8StructMembers = new StructMember[]
                {
                    new StructMember("AccelerationX", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("AccelerationY", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("AccelerationZ", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Acceleration8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Acceleration8")
                    .AddMembers(Acceleration8StructMembers));

                return result;
            }
        }
    }
    public class Acceleration8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Acceleration8>
    {
        public Acceleration8Support() : base(
            new Implementation.Acceleration8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Acceleration8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Acceleration8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Acceleration8Support, global::STRUCT.Acceleration8>();

    }

    namespace Implementation
    {

        public struct Orientation8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Orientation8>
        {

            private double Roll;
            private double Pitch;
            private double Yaw;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.Orientation8 sample, bool keysOnly = false)
            {

                sample.Roll = Roll;
                sample.Pitch = Pitch;
                sample.Yaw = Yaw;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Roll = (double) (0.0);
                Pitch = (double) (0.0);
                Yaw = (double) (0.0);
            }

            public void ToNative(global::STRUCT.Orientation8 sample, bool keysOnly = false)
            {
                Roll = sample.Roll;
                Pitch = sample.Pitch;
                Yaw = sample.Yaw;
            }
        }

        internal class Orientation8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Orientation8, Orientation8Unmanaged>
        {

            internal Orientation8Plugin() : base("global::STRUCT.Orientation8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Orientation8 struct
                var Orientation8StructMembers = new StructMember[]
                {
                    new StructMember("Roll", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("Pitch", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("Yaw", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Orientation8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Orientation8")
                    .AddMembers(Orientation8StructMembers));

                return result;
            }
        }
    }
    public class Orientation8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Orientation8>
    {
        public Orientation8Support() : base(
            new Implementation.Orientation8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Orientation8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Orientation8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Orientation8Support, global::STRUCT.Orientation8>();

    }

    namespace Implementation
    {

        public struct AER8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.AER8>
        {

            private double Azimuth;
            private double Elevation;
            private double Roll;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.AER8 sample, bool keysOnly = false)
            {

                sample.Azimuth = Azimuth;
                sample.Elevation = Elevation;
                sample.Roll = Roll;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Azimuth = (double) (0.0);
                Elevation = (double) (0.0);
                Roll = (double) (0.0);
            }

            public void ToNative(global::STRUCT.AER8 sample, bool keysOnly = false)
            {
                Azimuth = sample.Azimuth;
                Elevation = sample.Elevation;
                Roll = sample.Roll;
            }
        }

        internal class AER8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.AER8, AER8Unmanaged>
        {

            internal AER8Plugin() : base("global::STRUCT.AER8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // AER8 struct
                var AER8StructMembers = new StructMember[]
                {
                    new StructMember("Azimuth", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("Elevation", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("Roll", dtf.GetPrimitiveType<double>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<AER8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::AER8")
                    .AddMembers(AER8StructMembers));

                return result;
            }
        }
    }
    public class AER8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.AER8>
    {
        public AER8Support() : base(
            new Implementation.AER8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.AER8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static AER8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<AER8Support, global::STRUCT.AER8>();

    }

    namespace Implementation
    {

        public struct ObjectInfo4Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.ObjectInfo4>
        {

            private global::ENUM.ForceIdentifier IFF;
            private ushort ModelKind;
            private ushort ObjectNumber;
            private byte IsFrozen;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.ObjectInfo4 sample, bool keysOnly = false)
            {

                sample.IFF = IFF;
                sample.ModelKind = ModelKind;
                sample.ObjectNumber = ObjectNumber;
                sample.IsFrozen = Convert.ToBoolean(IsFrozen);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                IFF = (global::ENUM.ForceIdentifier) (0);
                ModelKind = (ushort) (0);
                ObjectNumber = (ushort) (0);
                IsFrozen = 0;
            }

            public void ToNative(global::STRUCT.ObjectInfo4 sample, bool keysOnly = false)
            {
                IFF = sample.IFF;
                ModelKind = sample.ModelKind;
                ObjectNumber = sample.ObjectNumber;
                IsFrozen = Convert.ToByte(sample.IsFrozen);
            }
        }

        internal class ObjectInfo4Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.ObjectInfo4, ObjectInfo4Unmanaged>
        {

            internal ObjectInfo4Plugin() : base("global::STRUCT.ObjectInfo4", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // ObjectInfo4 struct
                var ObjectInfo4StructMembers = new StructMember[]
                {
                    new StructMember("IFF", global::ENUM.ForceIdentifierSupport.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("ModelKind", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("ObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 2),
                    new StructMember("IsFrozen", dtf.GetPrimitiveType<bool>(), id: 3)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<ObjectInfo4Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::ObjectInfo4")
                    .AddMembers(ObjectInfo4StructMembers));

                return result;
            }
        }
    }
    public class ObjectInfo4Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.ObjectInfo4>
    {
        public ObjectInfo4Support() : base(
            new Implementation.ObjectInfo4Plugin(),
            new Lazy<DynamicType>(() =>Implementation.ObjectInfo4Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static ObjectInfo4Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<ObjectInfo4Support, global::STRUCT.ObjectInfo4>();

    }

    namespace Implementation
    {

        public struct Jamming4Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Jamming4>
        {

            private global::ENUM.JammingType Type;
            private ushort JammingPower;
            private ushort JammingPullOff;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.Jamming4 sample, bool keysOnly = false)
            {

                sample.Type = Type;
                sample.JammingPower = JammingPower;
                sample.JammingPullOff = JammingPullOff;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Type = (global::ENUM.JammingType) (0);
                JammingPower = (ushort) (0);
                JammingPullOff = (ushort) (0);
            }

            public void ToNative(global::STRUCT.Jamming4 sample, bool keysOnly = false)
            {
                Type = sample.Type;
                JammingPower = sample.JammingPower;
                JammingPullOff = sample.JammingPullOff;
            }
        }

        internal class Jamming4Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Jamming4, Jamming4Unmanaged>
        {

            internal Jamming4Plugin() : base("global::STRUCT.Jamming4", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Jamming4 struct
                var Jamming4StructMembers = new StructMember[]
                {
                    new StructMember("Type", global::ENUM.JammingTypeSupport.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("JammingPower", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("JammingPullOff", dtf.GetPrimitiveType<ushort>(), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Jamming4Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Jamming4")
                    .AddMembers(Jamming4StructMembers));

                return result;
            }
        }
    }
    public class Jamming4Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Jamming4>
    {
        public Jamming4Support() : base(
            new Implementation.Jamming4Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Jamming4Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Jamming4Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Jamming4Support, global::STRUCT.Jamming4>();

    }

    namespace Implementation
    {

        public struct LCHRStatus2Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.LCHRStatus2>
        {

            private ushort LaunchReadyStatus;
            private ushort LCUStatus;
            private ushort HotInventory;
            private ushort ColdInventory;
            private byte LSSNumber;
            private byte ConnectionStatus;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.LCHRStatus2 sample, bool keysOnly = false)
            {

                sample.LaunchReadyStatus = LaunchReadyStatus;
                sample.LCUStatus = LCUStatus;
                sample.HotInventory = HotInventory;
                sample.ColdInventory = ColdInventory;
                sample.LSSNumber = LSSNumber;
                sample.ConnectionStatus = ConnectionStatus;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                LaunchReadyStatus = (ushort) (0);
                LCUStatus = (ushort) (0);
                HotInventory = (ushort) (0);
                ColdInventory = (ushort) (0);
                LSSNumber = (byte) (0);
                ConnectionStatus = (byte) (0);
            }

            public void ToNative(global::STRUCT.LCHRStatus2 sample, bool keysOnly = false)
            {
                LaunchReadyStatus = sample.LaunchReadyStatus;
                LCUStatus = sample.LCUStatus;
                HotInventory = sample.HotInventory;
                ColdInventory = sample.ColdInventory;
                LSSNumber = sample.LSSNumber;
                ConnectionStatus = sample.ConnectionStatus;
            }
        }

        internal class LCHRStatus2Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.LCHRStatus2, LCHRStatus2Unmanaged>
        {

            internal LCHRStatus2Plugin() : base("global::STRUCT.LCHRStatus2", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // LCHRStatus2 struct
                var LCHRStatus2StructMembers = new StructMember[]
                {
                    new StructMember("LaunchReadyStatus", dtf.GetPrimitiveType<ushort>(), id: 0),
                    new StructMember("LCUStatus", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("HotInventory", dtf.GetPrimitiveType<ushort>(), id: 2),
                    new StructMember("ColdInventory", dtf.GetPrimitiveType<ushort>(), id: 3),
                    new StructMember("LSSNumber", dtf.GetPrimitiveType<byte>(), id: 4),
                    new StructMember("ConnectionStatus", dtf.GetPrimitiveType<byte>(), id: 5)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<LCHRStatus2Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::LCHRStatus2")
                    .AddMembers(LCHRStatus2StructMembers));

                return result;
            }
        }
    }
    public class LCHRStatus2Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.LCHRStatus2>
    {
        public LCHRStatus2Support() : base(
            new Implementation.LCHRStatus2Plugin(),
            new Lazy<DynamicType>(() =>Implementation.LCHRStatus2Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static LCHRStatus2Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<LCHRStatus2Support, global::STRUCT.LCHRStatus2>();

    }

    namespace Implementation
    {

        public struct MFRStatus8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.MFRStatus8>
        {

            private double BeamAzimuth;
            private double BeamAzimuthOffset;
            private double BeamElevation;
            private double BeamElevationOffset;
            private double BeamHorizontalArea;
            private double BeamVerticalArea;
            private uint BeamRange;
            private byte BeamOnOffState;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.MFRStatus8 sample, bool keysOnly = false)
            {

                sample.BeamAzimuth = BeamAzimuth;
                sample.BeamAzimuthOffset = BeamAzimuthOffset;
                sample.BeamElevation = BeamElevation;
                sample.BeamElevationOffset = BeamElevationOffset;
                sample.BeamHorizontalArea = BeamHorizontalArea;
                sample.BeamVerticalArea = BeamVerticalArea;
                sample.BeamRange = BeamRange;
                sample.BeamOnOffState = BeamOnOffState;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                BeamAzimuth = (double) (0.0);
                BeamAzimuthOffset = (double) (0.0);
                BeamElevation = (double) (0.0);
                BeamElevationOffset = (double) (0.0);
                BeamHorizontalArea = (double) (0.0);
                BeamVerticalArea = (double) (0.0);
                BeamRange = (uint) (0u);
                BeamOnOffState = (byte) (0);
            }

            public void ToNative(global::STRUCT.MFRStatus8 sample, bool keysOnly = false)
            {
                BeamAzimuth = sample.BeamAzimuth;
                BeamAzimuthOffset = sample.BeamAzimuthOffset;
                BeamElevation = sample.BeamElevation;
                BeamElevationOffset = sample.BeamElevationOffset;
                BeamHorizontalArea = sample.BeamHorizontalArea;
                BeamVerticalArea = sample.BeamVerticalArea;
                BeamRange = sample.BeamRange;
                BeamOnOffState = sample.BeamOnOffState;
            }
        }

        internal class MFRStatus8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.MFRStatus8, MFRStatus8Unmanaged>
        {

            internal MFRStatus8Plugin() : base("global::STRUCT.MFRStatus8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // MFRStatus8 struct
                var MFRStatus8StructMembers = new StructMember[]
                {
                    new StructMember("BeamAzimuth", dtf.GetPrimitiveType<double>(), id: 0),
                    new StructMember("BeamAzimuthOffset", dtf.GetPrimitiveType<double>(), id: 1),
                    new StructMember("BeamElevation", dtf.GetPrimitiveType<double>(), id: 2),
                    new StructMember("BeamElevationOffset", dtf.GetPrimitiveType<double>(), id: 3),
                    new StructMember("BeamHorizontalArea", dtf.GetPrimitiveType<double>(), id: 4),
                    new StructMember("BeamVerticalArea", dtf.GetPrimitiveType<double>(), id: 5),
                    new StructMember("BeamRange", dtf.GetPrimitiveType<uint>(), id: 6),
                    new StructMember("BeamOnOffState", dtf.GetPrimitiveType<byte>(), id: 7)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<MFRStatus8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::MFRStatus8")
                    .AddMembers(MFRStatus8StructMembers));

                return result;
            }
        }
    }
    public class MFRStatus8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.MFRStatus8>
    {
        public MFRStatus8Support() : base(
            new Implementation.MFRStatus8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.MFRStatus8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MFRStatus8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MFRStatus8Support, global::STRUCT.MFRStatus8>();

    }

    namespace Implementation
    {

        public struct Config8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Config8>
        {

            private NativeString ConfigName;
            private NativeString ConfigVersion;
            private global::ENUM.SimulatorID SimID;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                ConfigName.Destroy();
                ConfigVersion.Destroy();
            }

            public void FromNative(global::STRUCT.Config8 sample, bool keysOnly = false)
            {

                sample.ConfigName = ConfigName.FromNative();
                sample.ConfigVersion = ConfigVersion.FromNative();
                sample.SimID = SimID;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                ConfigName.Initialize(size: ((int) 255), allocateMemory: allocateMemory);
                ConfigVersion.Initialize(size: ((int) 255), allocateMemory: allocateMemory);
                SimID = (global::ENUM.SimulatorID) (1);
            }

            public void ToNative(global::STRUCT.Config8 sample, bool keysOnly = false)
            {
                ConfigName.ToNative(sample.ConfigName, ((int) 255));
                ConfigVersion.ToNative(sample.ConfigVersion, ((int) 255));
                SimID = sample.SimID;
            }
        }

        internal class Config8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Config8, Config8Unmanaged>
        {

            internal Config8Plugin() : base("global::STRUCT.Config8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Config8 struct
                var Config8StructMembers = new StructMember[]
                {
                    new StructMember("ConfigName", dtf.CreateString(((int) 255)), id: 0),
                    new StructMember("ConfigVersion", dtf.CreateString(((int) 255)), id: 1),
                    new StructMember("SimID", global::ENUM.SimulatorIDSupport.Instance.GetDynamicTypeInternal(isPublic), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Config8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Config8")
                    .AddMembers(Config8StructMembers));

                return result;
            }
        }
    }
    public class Config8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Config8>
    {
        public Config8Support() : base(
            new Implementation.Config8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Config8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Config8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Config8Support, global::STRUCT.Config8>();

    }

    namespace Implementation
    {

        public struct AirThreat8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.AirThreat8>
        {

            private global::STRUCT.Implementation.Position8Unmanaged Position;
            private global::STRUCT.Implementation.Velocity8Unmanaged Velocity;
            private global::STRUCT.Implementation.Acceleration8Unmanaged Acceleration;
            private global::STRUCT.Implementation.Orientation8Unmanaged Orientation;
            private global::STRUCT.Implementation.ObjectInfo4Unmanaged Information;
            private global::STRUCT.Implementation.Jamming4Unmanaged JammingStatus;
            private global::ENUM.ATSStatus Status;
            private ushort MeanRCS;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Position.Destroy(optionalsOnly);
                Velocity.Destroy(optionalsOnly);
                Acceleration.Destroy(optionalsOnly);
                Orientation.Destroy(optionalsOnly);
                Information.Destroy(optionalsOnly);
                JammingStatus.Destroy(optionalsOnly);
            }

            public void FromNative(global::STRUCT.AirThreat8 sample, bool keysOnly = false)
            {

                Position.FromNative(sample.Position, keysOnly: false);
                Velocity.FromNative(sample.Velocity, keysOnly: false);
                Acceleration.FromNative(sample.Acceleration, keysOnly: false);
                Orientation.FromNative(sample.Orientation, keysOnly: false);
                Information.FromNative(sample.Information, keysOnly: false);
                JammingStatus.FromNative(sample.JammingStatus, keysOnly: false);
                sample.Status = Status;
                sample.MeanRCS = MeanRCS;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Position.Initialize(allocatePointers, allocateMemory);
                Velocity.Initialize(allocatePointers, allocateMemory);
                Acceleration.Initialize(allocatePointers, allocateMemory);
                Orientation.Initialize(allocatePointers, allocateMemory);
                Information.Initialize(allocatePointers, allocateMemory);
                JammingStatus.Initialize(allocatePointers, allocateMemory);
                Status = (global::ENUM.ATSStatus) (0);
                MeanRCS = (ushort) (0);
            }

            public void ToNative(global::STRUCT.AirThreat8 sample, bool keysOnly = false)
            {
                Position.ToNative(sample.Position, keysOnly: false);
                Velocity.ToNative(sample.Velocity, keysOnly: false);
                Acceleration.ToNative(sample.Acceleration, keysOnly: false);
                Orientation.ToNative(sample.Orientation, keysOnly: false);
                Information.ToNative(sample.Information, keysOnly: false);
                JammingStatus.ToNative(sample.JammingStatus, keysOnly: false);
                Status = sample.Status;
                MeanRCS = sample.MeanRCS;
            }
        }

        internal class AirThreat8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.AirThreat8, AirThreat8Unmanaged>
        {

            internal AirThreat8Plugin() : base("global::STRUCT.AirThreat8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // AirThreat8 struct
                var AirThreat8StructMembers = new StructMember[]
                {
                    new StructMember("Position", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("Velocity", global::STRUCT.Velocity8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("Acceleration", global::STRUCT.Acceleration8Support.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("Orientation", global::STRUCT.Orientation8Support.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("Information", global::STRUCT.ObjectInfo4Support.Instance.GetDynamicTypeInternal(isPublic), id: 4),
                    new StructMember("JammingStatus", global::STRUCT.Jamming4Support.Instance.GetDynamicTypeInternal(isPublic), id: 5),
                    new StructMember("Status", global::ENUM.ATSStatusSupport.Instance.GetDynamicTypeInternal(isPublic), id: 6),
                    new StructMember("MeanRCS", dtf.GetPrimitiveType<ushort>(), id: 7)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<AirThreat8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::AirThreat8")
                    .AddMembers(AirThreat8StructMembers));

                return result;
            }
        }
    }
    public class AirThreat8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.AirThreat8>
    {
        public AirThreat8Support() : base(
            new Implementation.AirThreat8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.AirThreat8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static AirThreat8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<AirThreat8Support, global::STRUCT.AirThreat8>();

    }

    namespace Implementation
    {

        public struct Munition8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Munition8>
        {

            private global::STRUCT.Implementation.Position8Unmanaged Position;
            private global::STRUCT.Implementation.Velocity8Unmanaged Velocity;
            private global::STRUCT.Implementation.Acceleration8Unmanaged Acceleration;
            private global::STRUCT.Implementation.Orientation8Unmanaged Orientation;
            private global::STRUCT.Implementation.ObjectInfo4Unmanaged Information;
            private global::ENUM.MunitionStatus Status;
            private global::ENUM.SeekerPhase SeekerStatus;
            private global::ENUM.MunitionType MissileType;
            private ushort LockOnThreatObjectNumber;
            private ushort MeanRCS;
            private ushort SignalPower;
            private ushort AddressKey;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Position.Destroy(optionalsOnly);
                Velocity.Destroy(optionalsOnly);
                Acceleration.Destroy(optionalsOnly);
                Orientation.Destroy(optionalsOnly);
                Information.Destroy(optionalsOnly);
            }

            public void FromNative(global::STRUCT.Munition8 sample, bool keysOnly = false)
            {

                Position.FromNative(sample.Position, keysOnly: false);
                Velocity.FromNative(sample.Velocity, keysOnly: false);
                Acceleration.FromNative(sample.Acceleration, keysOnly: false);
                Orientation.FromNative(sample.Orientation, keysOnly: false);
                Information.FromNative(sample.Information, keysOnly: false);
                sample.Status = Status;
                sample.SeekerStatus = SeekerStatus;
                sample.MissileType = MissileType;
                sample.LockOnThreatObjectNumber = LockOnThreatObjectNumber;
                sample.MeanRCS = MeanRCS;
                sample.SignalPower = SignalPower;
                sample.AddressKey = AddressKey;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Position.Initialize(allocatePointers, allocateMemory);
                Velocity.Initialize(allocatePointers, allocateMemory);
                Acceleration.Initialize(allocatePointers, allocateMemory);
                Orientation.Initialize(allocatePointers, allocateMemory);
                Information.Initialize(allocatePointers, allocateMemory);
                Status = (global::ENUM.MunitionStatus) (0);
                SeekerStatus = (global::ENUM.SeekerPhase) (0);
                MissileType = (global::ENUM.MunitionType) (0);
                LockOnThreatObjectNumber = (ushort) (0);
                MeanRCS = (ushort) (0);
                SignalPower = (ushort) (0);
                AddressKey = (ushort) (0);
            }

            public void ToNative(global::STRUCT.Munition8 sample, bool keysOnly = false)
            {
                Position.ToNative(sample.Position, keysOnly: false);
                Velocity.ToNative(sample.Velocity, keysOnly: false);
                Acceleration.ToNative(sample.Acceleration, keysOnly: false);
                Orientation.ToNative(sample.Orientation, keysOnly: false);
                Information.ToNative(sample.Information, keysOnly: false);
                Status = sample.Status;
                SeekerStatus = sample.SeekerStatus;
                MissileType = sample.MissileType;
                LockOnThreatObjectNumber = sample.LockOnThreatObjectNumber;
                MeanRCS = sample.MeanRCS;
                SignalPower = sample.SignalPower;
                AddressKey = sample.AddressKey;
            }
        }

        internal class Munition8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Munition8, Munition8Unmanaged>
        {

            internal Munition8Plugin() : base("global::STRUCT.Munition8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Munition8 struct
                var Munition8StructMembers = new StructMember[]
                {
                    new StructMember("Position", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("Velocity", global::STRUCT.Velocity8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("Acceleration", global::STRUCT.Acceleration8Support.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("Orientation", global::STRUCT.Orientation8Support.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("Information", global::STRUCT.ObjectInfo4Support.Instance.GetDynamicTypeInternal(isPublic), id: 4),
                    new StructMember("Status", global::ENUM.MunitionStatusSupport.Instance.GetDynamicTypeInternal(isPublic), id: 5),
                    new StructMember("SeekerStatus", global::ENUM.SeekerPhaseSupport.Instance.GetDynamicTypeInternal(isPublic), id: 6),
                    new StructMember("MissileType", global::ENUM.MunitionTypeSupport.Instance.GetDynamicTypeInternal(isPublic), id: 7),
                    new StructMember("LockOnThreatObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 8),
                    new StructMember("MeanRCS", dtf.GetPrimitiveType<ushort>(), id: 9),
                    new StructMember("SignalPower", dtf.GetPrimitiveType<ushort>(), id: 10),
                    new StructMember("AddressKey", dtf.GetPrimitiveType<ushort>(), id: 11)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Munition8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Munition8")
                    .AddMembers(Munition8StructMembers));

                return result;
            }
        }
    }
    public class Munition8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Munition8>
    {
        public Munition8Support() : base(
            new Implementation.Munition8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Munition8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Munition8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Munition8Support, global::STRUCT.Munition8>();

    }

    namespace Implementation
    {

        public struct GroundVehicle8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.GroundVehicle8>
        {

            private global::STRUCT.Implementation.Position8Unmanaged Position;
            private global::STRUCT.Implementation.Velocity8Unmanaged Velocity;
            private global::STRUCT.Implementation.Acceleration8Unmanaged Acceleration;
            private global::STRUCT.Implementation.Orientation8Unmanaged Orientation;
            private global::STRUCT.Implementation.MFRStatus8Unmanaged MFRStatus;
            private global::STRUCT.Implementation.ObjectInfo4Unmanaged Information;
            private global::STRUCT.Implementation.LCHRStatus2Unmanaged LauncherStatus;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Position.Destroy(optionalsOnly);
                Velocity.Destroy(optionalsOnly);
                Acceleration.Destroy(optionalsOnly);
                Orientation.Destroy(optionalsOnly);
                MFRStatus.Destroy(optionalsOnly);
                Information.Destroy(optionalsOnly);
                LauncherStatus.Destroy(optionalsOnly);
            }

            public void FromNative(global::STRUCT.GroundVehicle8 sample, bool keysOnly = false)
            {

                Position.FromNative(sample.Position, keysOnly: false);
                Velocity.FromNative(sample.Velocity, keysOnly: false);
                Acceleration.FromNative(sample.Acceleration, keysOnly: false);
                Orientation.FromNative(sample.Orientation, keysOnly: false);
                MFRStatus.FromNative(sample.MFRStatus, keysOnly: false);
                Information.FromNative(sample.Information, keysOnly: false);
                LauncherStatus.FromNative(sample.LauncherStatus, keysOnly: false);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Position.Initialize(allocatePointers, allocateMemory);
                Velocity.Initialize(allocatePointers, allocateMemory);
                Acceleration.Initialize(allocatePointers, allocateMemory);
                Orientation.Initialize(allocatePointers, allocateMemory);
                MFRStatus.Initialize(allocatePointers, allocateMemory);
                Information.Initialize(allocatePointers, allocateMemory);
                LauncherStatus.Initialize(allocatePointers, allocateMemory);
            }

            public void ToNative(global::STRUCT.GroundVehicle8 sample, bool keysOnly = false)
            {
                Position.ToNative(sample.Position, keysOnly: false);
                Velocity.ToNative(sample.Velocity, keysOnly: false);
                Acceleration.ToNative(sample.Acceleration, keysOnly: false);
                Orientation.ToNative(sample.Orientation, keysOnly: false);
                MFRStatus.ToNative(sample.MFRStatus, keysOnly: false);
                Information.ToNative(sample.Information, keysOnly: false);
                LauncherStatus.ToNative(sample.LauncherStatus, keysOnly: false);
            }
        }

        internal class GroundVehicle8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.GroundVehicle8, GroundVehicle8Unmanaged>
        {

            internal GroundVehicle8Plugin() : base("global::STRUCT.GroundVehicle8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // GroundVehicle8 struct
                var GroundVehicle8StructMembers = new StructMember[]
                {
                    new StructMember("Position", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("Velocity", global::STRUCT.Velocity8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("Acceleration", global::STRUCT.Acceleration8Support.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("Orientation", global::STRUCT.Orientation8Support.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("MFRStatus", global::STRUCT.MFRStatus8Support.Instance.GetDynamicTypeInternal(isPublic), id: 4),
                    new StructMember("Information", global::STRUCT.ObjectInfo4Support.Instance.GetDynamicTypeInternal(isPublic), id: 5),
                    new StructMember("LauncherStatus", global::STRUCT.LCHRStatus2Support.Instance.GetDynamicTypeInternal(isPublic), id: 6)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<GroundVehicle8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::GroundVehicle8")
                    .AddMembers(GroundVehicle8StructMembers));

                return result;
            }
        }
    }
    public class GroundVehicle8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.GroundVehicle8>
    {
        public GroundVehicle8Support() : base(
            new Implementation.GroundVehicle8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.GroundVehicle8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static GroundVehicle8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<GroundVehicle8Support, global::STRUCT.GroundVehicle8>();

    }

    namespace Implementation
    {

        public struct Uplink2Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Uplink2>
        {

            private ushort AddressKey;
            private ushort MissileType;
            private NativeUnmanagedArray Data;

            public void Destroy(bool optionalsOnly)
            {
                Data.Destroy(optionalsOnly);
            }

            public void FromNative(global::STRUCT.Uplink2 sample, bool keysOnly = false)
            {

                sample.AddressKey = AddressKey;
                sample.MissileType = MissileType;
                Data.FromNative(sample.Data, dimension: (26));
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                AddressKey = (ushort) (0);
                MissileType = (ushort) (0);
                Data.Initialize<ushort>(dimension: (26), allocateMemory: allocateMemory);
            }

            public void ToNative(global::STRUCT.Uplink2 sample, bool keysOnly = false)
            {
                AddressKey = sample.AddressKey;
                MissileType = sample.MissileType;
                Data.ToNative<ushort>(sample.Data, dimension: (26));
            }
        }

        internal class Uplink2Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Uplink2, Uplink2Unmanaged>
        {

            internal Uplink2Plugin() : base("global::STRUCT.Uplink2", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Uplink2 struct
                var Uplink2StructMembers = new StructMember[]
                {
                    new StructMember("AddressKey", dtf.GetPrimitiveType<ushort>(), id: 0),
                    new StructMember("MissileType", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("Data", tsf.CreateArrayWithAccessInfo<ushort>(dtf, dtf.GetPrimitiveType<ushort>(), new uint[] {26}), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Uplink2Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Uplink2")
                    .AddMembers(Uplink2StructMembers));

                return result;
            }
        }
    }
    public class Uplink2Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Uplink2>
    {
        public Uplink2Support() : base(
            new Implementation.Uplink2Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Uplink2Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Uplink2Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Uplink2Support, global::STRUCT.Uplink2>();

    }

    namespace Implementation
    {

        public struct Downlink2Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.Downlink2>
        {

            private ushort AddressKey;
            private ushort CRC;
            private NativeUnmanagedArray Data;

            public void Destroy(bool optionalsOnly)
            {
                Data.Destroy(optionalsOnly);
            }

            public void FromNative(global::STRUCT.Downlink2 sample, bool keysOnly = false)
            {

                sample.AddressKey = AddressKey;
                sample.CRC = CRC;
                Data.FromNative(sample.Data, dimension: (24));
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                AddressKey = (ushort) (0);
                CRC = (ushort) (0);
                Data.Initialize<ushort>(dimension: (24), allocateMemory: allocateMemory);
            }

            public void ToNative(global::STRUCT.Downlink2 sample, bool keysOnly = false)
            {
                AddressKey = sample.AddressKey;
                CRC = sample.CRC;
                Data.ToNative<ushort>(sample.Data, dimension: (24));
            }
        }

        internal class Downlink2Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.Downlink2, Downlink2Unmanaged>
        {

            internal Downlink2Plugin() : base("global::STRUCT.Downlink2", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Downlink2 struct
                var Downlink2StructMembers = new StructMember[]
                {
                    new StructMember("AddressKey", dtf.GetPrimitiveType<ushort>(), id: 0),
                    new StructMember("CRC", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("Data", tsf.CreateArrayWithAccessInfo<ushort>(dtf, dtf.GetPrimitiveType<ushort>(), new uint[] {24}), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<Downlink2Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::Downlink2")
                    .AddMembers(Downlink2StructMembers));

                return result;
            }
        }
    }
    public class Downlink2Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.Downlink2>
    {
        public Downlink2Support() : base(
            new Implementation.Downlink2Plugin(),
            new Lazy<DynamicType>(() =>Implementation.Downlink2Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static Downlink2Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<Downlink2Support, global::STRUCT.Downlink2>();

    }

    namespace Implementation
    {

        public struct TimeTick8Unmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::STRUCT.TimeTick8>
        {

            private ulong TimeTick;
            private uint SyncCycle;

            public void Destroy(bool optionalsOnly)
            {
            }

            public void FromNative(global::STRUCT.TimeTick8 sample, bool keysOnly = false)
            {

                sample.TimeTick = TimeTick;
                sample.SyncCycle = SyncCycle;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                TimeTick = (ulong) (0uL);
                SyncCycle = (uint) (0u);
            }

            public void ToNative(global::STRUCT.TimeTick8 sample, bool keysOnly = false)
            {
                TimeTick = sample.TimeTick;
                SyncCycle = sample.SyncCycle;
            }
        }

        internal class TimeTick8Plugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::STRUCT.TimeTick8, TimeTick8Unmanaged>
        {

            internal TimeTick8Plugin() : base("global::STRUCT.TimeTick8", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // TimeTick8 struct
                var TimeTick8StructMembers = new StructMember[]
                {
                    new StructMember("TimeTick", dtf.GetPrimitiveType<ulong>(), id: 0),
                    new StructMember("SyncCycle", dtf.GetPrimitiveType<uint>(), id: 1)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<TimeTick8Unmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("STRUCT::TimeTick8")
                    .AddMembers(TimeTick8StructMembers));

                return result;
            }
        }
    }
    public class TimeTick8Support : Rti.Dds.Topics.TypeSupport<global::STRUCT.TimeTick8>
    {
        public TimeTick8Support() : base(
            new Implementation.TimeTick8Plugin(),
            new Lazy<DynamicType>(() =>Implementation.TimeTick8Plugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static TimeTick8Support Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<TimeTick8Support, global::STRUCT.TimeTick8>();

    }

} // namespace STRUCT

namespace MSG
{

    namespace Implementation
    {

        public struct AirThreatInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.AirThreatInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private ushort AirCount;
            private NativeSeq AirObjects;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                AirObjects.Destroy<global::STRUCT.AirThreat8, global::STRUCT.Implementation.AirThreat8Unmanaged>(optionalsOnly);
            }

            public void FromNative(global::MSG.AirThreatInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.AirCount = AirCount;
                AirObjects.FromNative<global::STRUCT.AirThreat8, global::STRUCT.Implementation.AirThreat8Unmanaged>(sample.AirObjects);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                AirCount = (ushort) (0);
                AirObjects.Initialize<global::STRUCT.AirThreat8 , global::STRUCT.Implementation.AirThreat8Unmanaged >(max: ((int)900), absoluteMax: ((int)900), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.AirThreatInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                AirCount = sample.AirCount;
                AirObjects.ToNative<global::STRUCT.AirThreat8, global::STRUCT.Implementation.AirThreat8Unmanaged>(sample.AirObjects);
            }
        }

        internal class AirThreatInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.AirThreatInformation, AirThreatInformationUnmanaged>
        {

            internal AirThreatInformationPlugin() : base("global::MSG.AirThreatInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // AirThreatInformation struct
                var AirThreatInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("AirCount", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("AirObjects", tsf.CreateSequenceWithAccessInfo(dtf, global::STRUCT.AirThreat8Support.Instance.GetDynamicTypeInternal(isPublic), ((int)900)), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<AirThreatInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::AirThreatInformation")
                    .AddMembers(AirThreatInformationStructMembers));

                return result;
            }
        }
    }
    public class AirThreatInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.AirThreatInformation>
    {
        public AirThreatInformationSupport() : base(
            new Implementation.AirThreatInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.AirThreatInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static AirThreatInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<AirThreatInformationSupport, global::MSG.AirThreatInformation>();

    }

    namespace Implementation
    {

        public struct MunitionInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.MunitionInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private ushort MunitionCount;
            private NativeSeq MunitionObjects;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                MunitionObjects.Destroy<global::STRUCT.Munition8, global::STRUCT.Implementation.Munition8Unmanaged>(optionalsOnly);
            }

            public void FromNative(global::MSG.MunitionInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.MunitionCount = MunitionCount;
                MunitionObjects.FromNative<global::STRUCT.Munition8, global::STRUCT.Implementation.Munition8Unmanaged>(sample.MunitionObjects);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                MunitionCount = (ushort) (0);
                MunitionObjects.Initialize<global::STRUCT.Munition8 , global::STRUCT.Implementation.Munition8Unmanaged >(max: ((int)900), absoluteMax: ((int)900), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.MunitionInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                MunitionCount = sample.MunitionCount;
                MunitionObjects.ToNative<global::STRUCT.Munition8, global::STRUCT.Implementation.Munition8Unmanaged>(sample.MunitionObjects);
            }
        }

        internal class MunitionInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.MunitionInformation, MunitionInformationUnmanaged>
        {

            internal MunitionInformationPlugin() : base("global::MSG.MunitionInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // MunitionInformation struct
                var MunitionInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("MunitionCount", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("MunitionObjects", tsf.CreateSequenceWithAccessInfo(dtf, global::STRUCT.Munition8Support.Instance.GetDynamicTypeInternal(isPublic), ((int)900)), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<MunitionInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::MunitionInformation")
                    .AddMembers(MunitionInformationStructMembers));

                return result;
            }
        }
    }
    public class MunitionInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.MunitionInformation>
    {
        public MunitionInformationSupport() : base(
            new Implementation.MunitionInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.MunitionInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MunitionInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MunitionInformationSupport, global::MSG.MunitionInformation>();

    }

    namespace Implementation
    {

        public struct GroundVehicleInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.GroundVehicleInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private ushort GroundVehicleCount;
            private NativeSeq GroundVehicleObjects;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                GroundVehicleObjects.Destroy<global::STRUCT.GroundVehicle8, global::STRUCT.Implementation.GroundVehicle8Unmanaged>(optionalsOnly);
            }

            public void FromNative(global::MSG.GroundVehicleInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.GroundVehicleCount = GroundVehicleCount;
                GroundVehicleObjects.FromNative<global::STRUCT.GroundVehicle8, global::STRUCT.Implementation.GroundVehicle8Unmanaged>(sample.GroundVehicleObjects);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                GroundVehicleCount = (ushort) (0);
                GroundVehicleObjects.Initialize<global::STRUCT.GroundVehicle8 , global::STRUCT.Implementation.GroundVehicle8Unmanaged >(max: ((int)100), absoluteMax: ((int)100), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.GroundVehicleInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                GroundVehicleCount = sample.GroundVehicleCount;
                GroundVehicleObjects.ToNative<global::STRUCT.GroundVehicle8, global::STRUCT.Implementation.GroundVehicle8Unmanaged>(sample.GroundVehicleObjects);
            }
        }

        internal class GroundVehicleInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.GroundVehicleInformation, GroundVehicleInformationUnmanaged>
        {

            internal GroundVehicleInformationPlugin() : base("global::MSG.GroundVehicleInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // GroundVehicleInformation struct
                var GroundVehicleInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("GroundVehicleCount", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("GroundVehicleObjects", tsf.CreateSequenceWithAccessInfo(dtf, global::STRUCT.GroundVehicle8Support.Instance.GetDynamicTypeInternal(isPublic), ((int)100)), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<GroundVehicleInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::GroundVehicleInformation")
                    .AddMembers(GroundVehicleInformationStructMembers));

                return result;
            }
        }
    }
    public class GroundVehicleInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.GroundVehicleInformation>
    {
        public GroundVehicleInformationSupport() : base(
            new Implementation.GroundVehicleInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.GroundVehicleInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static GroundVehicleInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<GroundVehicleInformationSupport, global::MSG.GroundVehicleInformation>();

    }

    namespace Implementation
    {

        public struct UplinkInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.UplinkInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private ushort UplinkCount;
            private NativeSeq UplinkMessages;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                UplinkMessages.Destroy<global::STRUCT.Uplink2, global::STRUCT.Implementation.Uplink2Unmanaged>(optionalsOnly);
            }

            public void FromNative(global::MSG.UplinkInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.UplinkCount = UplinkCount;
                UplinkMessages.FromNative<global::STRUCT.Uplink2, global::STRUCT.Implementation.Uplink2Unmanaged>(sample.UplinkMessages);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                UplinkCount = (ushort) (0);
                UplinkMessages.Initialize<global::STRUCT.Uplink2 , global::STRUCT.Implementation.Uplink2Unmanaged >(max: ((int)56), absoluteMax: ((int)56), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.UplinkInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                UplinkCount = sample.UplinkCount;
                UplinkMessages.ToNative<global::STRUCT.Uplink2, global::STRUCT.Implementation.Uplink2Unmanaged>(sample.UplinkMessages);
            }
        }

        internal class UplinkInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.UplinkInformation, UplinkInformationUnmanaged>
        {

            internal UplinkInformationPlugin() : base("global::MSG.UplinkInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // UplinkInformation struct
                var UplinkInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("UplinkCount", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("UplinkMessages", tsf.CreateSequenceWithAccessInfo(dtf, global::STRUCT.Uplink2Support.Instance.GetDynamicTypeInternal(isPublic), ((int)56)), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<UplinkInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::UplinkInformation")
                    .AddMembers(UplinkInformationStructMembers));

                return result;
            }
        }
    }
    public class UplinkInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.UplinkInformation>
    {
        public UplinkInformationSupport() : base(
            new Implementation.UplinkInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.UplinkInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static UplinkInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<UplinkInformationSupport, global::MSG.UplinkInformation>();

    }

    namespace Implementation
    {

        public struct DownlinkInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.DownlinkInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private ushort DownlinkCount;
            private NativeSeq DownlinkMessage;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                DownlinkMessage.Destroy<global::STRUCT.Downlink2, global::STRUCT.Implementation.Downlink2Unmanaged>(optionalsOnly);
            }

            public void FromNative(global::MSG.DownlinkInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.DownlinkCount = DownlinkCount;
                DownlinkMessage.FromNative<global::STRUCT.Downlink2, global::STRUCT.Implementation.Downlink2Unmanaged>(sample.DownlinkMessage);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                DownlinkCount = (ushort) (0);
                DownlinkMessage.Initialize<global::STRUCT.Downlink2 , global::STRUCT.Implementation.Downlink2Unmanaged >(max: ((int)56), absoluteMax: ((int)56), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.DownlinkInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                DownlinkCount = sample.DownlinkCount;
                DownlinkMessage.ToNative<global::STRUCT.Downlink2, global::STRUCT.Implementation.Downlink2Unmanaged>(sample.DownlinkMessage);
            }
        }

        internal class DownlinkInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.DownlinkInformation, DownlinkInformationUnmanaged>
        {

            internal DownlinkInformationPlugin() : base("global::MSG.DownlinkInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // DownlinkInformation struct
                var DownlinkInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("DownlinkCount", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("DownlinkMessage", tsf.CreateSequenceWithAccessInfo(dtf, global::STRUCT.Downlink2Support.Instance.GetDynamicTypeInternal(isPublic), ((int)56)), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<DownlinkInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::DownlinkInformation")
                    .AddMembers(DownlinkInformationStructMembers));

                return result;
            }
        }
    }
    public class DownlinkInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.DownlinkInformation>
    {
        public DownlinkInformationSupport() : base(
            new Implementation.DownlinkInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.DownlinkInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static DownlinkInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<DownlinkInformationSupport, global::MSG.DownlinkInformation>();

    }

    namespace Implementation
    {

        public struct TimeTickInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.TimeTickInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private global::STRUCT.Implementation.TimeTick8Unmanaged TimeTickMessage;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                TimeTickMessage.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.TimeTickInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                TimeTickMessage.FromNative(sample.TimeTickMessage, keysOnly: false);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                TimeTickMessage.Initialize(allocatePointers, allocateMemory);
            }

            public void ToNative(global::MSG.TimeTickInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                TimeTickMessage.ToNative(sample.TimeTickMessage, keysOnly: false);
            }
        }

        internal class TimeTickInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.TimeTickInformation, TimeTickInformationUnmanaged>
        {

            internal TimeTickInformationPlugin() : base("global::MSG.TimeTickInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // TimeTickInformation struct
                var TimeTickInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("TimeTickMessage", global::STRUCT.TimeTick8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<TimeTickInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::TimeTickInformation")
                    .AddMembers(TimeTickInformationStructMembers));

                return result;
            }
        }
    }
    public class TimeTickInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.TimeTickInformation>
    {
        public TimeTickInformationSupport() : base(
            new Implementation.TimeTickInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.TimeTickInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static TimeTickInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<TimeTickInformationSupport, global::MSG.TimeTickInformation>();

    }

    namespace Implementation
    {

        public struct SimulatorStatusUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.SimulatorStatus>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.SimulatorStatus sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
            }

            public void ToNative(global::MSG.SimulatorStatus sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
            }
        }

        internal class SimulatorStatusPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.SimulatorStatus, SimulatorStatusUnmanaged>
        {

            internal SimulatorStatusPlugin() : base("global::MSG.SimulatorStatus", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // SimulatorStatus struct
                var SimulatorStatusStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<SimulatorStatusUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::SimulatorStatus")
                    .AddMembers(SimulatorStatusStructMembers));

                return result;
            }
        }
    }
    public class SimulatorStatusSupport : Rti.Dds.Topics.TypeSupport<global::MSG.SimulatorStatus>
    {
        public SimulatorStatusSupport() : base(
            new Implementation.SimulatorStatusPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SimulatorStatusPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SimulatorStatusSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SimulatorStatusSupport, global::MSG.SimulatorStatus>();

    }

    namespace Implementation
    {

        public struct WeaponFireUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.WeaponFire>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private global::STRUCT.Implementation.Position8Unmanaged MFRPosition;
            private global::STRUCT.Implementation.AER8Unmanaged MFRAntennaAER;
            private global::ENUM.MunitionType MunitionType;
            private ushort MunitionObjectNumber;
            private ushort MunitionAddressKey;
            private ushort EDCode;
            private ushort IPGroundType;
            private ushort IsForcedFire;
            private NativeUnmanagedArray EncodingKeyTable;
            private byte LSSNumber;
            private byte EncodingKeyFlag;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                MFRPosition.Destroy(optionalsOnly);
                MFRAntennaAER.Destroy(optionalsOnly);
                EncodingKeyTable.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.WeaponFire sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                MFRPosition.FromNative(sample.MFRPosition, keysOnly: false);
                MFRAntennaAER.FromNative(sample.MFRAntennaAER, keysOnly: false);
                sample.MunitionType = MunitionType;
                sample.MunitionObjectNumber = MunitionObjectNumber;
                sample.MunitionAddressKey = MunitionAddressKey;
                sample.EDCode = EDCode;
                sample.IPGroundType = IPGroundType;
                sample.IsForcedFire = IsForcedFire;
                EncodingKeyTable.FromNative(sample.EncodingKeyTable, dimension: (55));
                sample.LSSNumber = LSSNumber;
                sample.EncodingKeyFlag = Convert.ToBoolean(EncodingKeyFlag);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                MFRPosition.Initialize(allocatePointers, allocateMemory);
                MFRAntennaAER.Initialize(allocatePointers, allocateMemory);
                MunitionType = (global::ENUM.MunitionType) (0);
                MunitionObjectNumber = (ushort) (0);
                MunitionAddressKey = (ushort) (0);
                EDCode = (ushort) (0);
                IPGroundType = (ushort) (0);
                IsForcedFire = (ushort) (0);
                EncodingKeyTable.Initialize<ushort>(dimension: (55), allocateMemory: allocateMemory);
                LSSNumber = (byte) (0);
                EncodingKeyFlag = 0;
            }

            public void ToNative(global::MSG.WeaponFire sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                MFRPosition.ToNative(sample.MFRPosition, keysOnly: false);
                MFRAntennaAER.ToNative(sample.MFRAntennaAER, keysOnly: false);
                MunitionType = sample.MunitionType;
                MunitionObjectNumber = sample.MunitionObjectNumber;
                MunitionAddressKey = sample.MunitionAddressKey;
                EDCode = sample.EDCode;
                IPGroundType = sample.IPGroundType;
                IsForcedFire = sample.IsForcedFire;
                EncodingKeyTable.ToNative<ushort>(sample.EncodingKeyTable, dimension: (55));
                LSSNumber = sample.LSSNumber;
                EncodingKeyFlag = Convert.ToByte(sample.EncodingKeyFlag);
            }
        }

        internal class WeaponFirePlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.WeaponFire, WeaponFireUnmanaged>
        {

            internal WeaponFirePlugin() : base("global::MSG.WeaponFire", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // WeaponFire struct
                var WeaponFireStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("MFRPosition", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("MFRAntennaAER", global::STRUCT.AER8Support.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("MunitionType", global::ENUM.MunitionTypeSupport.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("MunitionObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 4),
                    new StructMember("MunitionAddressKey", dtf.GetPrimitiveType<ushort>(), id: 5),
                    new StructMember("EDCode", dtf.GetPrimitiveType<ushort>(), id: 6),
                    new StructMember("IPGroundType", dtf.GetPrimitiveType<ushort>(), id: 7),
                    new StructMember("IsForcedFire", dtf.GetPrimitiveType<ushort>(), id: 8),
                    new StructMember("EncodingKeyTable", tsf.CreateArrayWithAccessInfo<ushort>(dtf, dtf.GetPrimitiveType<ushort>(), new uint[] {55}), id: 9),
                    new StructMember("LSSNumber", dtf.GetPrimitiveType<byte>(), id: 10),
                    new StructMember("EncodingKeyFlag", dtf.GetPrimitiveType<bool>(), id: 11)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<WeaponFireUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::WeaponFire")
                    .AddMembers(WeaponFireStructMembers));

                return result;
            }
        }
    }
    public class WeaponFireSupport : Rti.Dds.Topics.TypeSupport<global::MSG.WeaponFire>
    {
        public WeaponFireSupport() : base(
            new Implementation.WeaponFirePlugin(),
            new Lazy<DynamicType>(() =>Implementation.WeaponFirePlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static WeaponFireSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<WeaponFireSupport, global::MSG.WeaponFire>();

    }

    namespace Implementation
    {

        public struct TargetInformationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.TargetInformation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private global::STRUCT.Implementation.Position8Unmanaged TargetPosition;
            private global::STRUCT.Implementation.Velocity8Unmanaged TargetVelocity;
            private global::STRUCT.Implementation.Position8Unmanaged IPPosition;
            private global::STRUCT.Implementation.Velocity8Unmanaged IPVelocity;
            private global::ENUM.ThreatType TargetType;
            private global::STRUCT.Implementation.StandardDeviation2Unmanaged TargetPositionSTD;
            private global::STRUCT.Implementation.StandardDeviation2Unmanaged TargetVelocitySTD;
            private ushort MunitionObjectNumber;
            private ushort DelayTime;
            private byte LSSNumber;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                TargetPosition.Destroy(optionalsOnly);
                TargetVelocity.Destroy(optionalsOnly);
                IPPosition.Destroy(optionalsOnly);
                IPVelocity.Destroy(optionalsOnly);
                TargetPositionSTD.Destroy(optionalsOnly);
                TargetVelocitySTD.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.TargetInformation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                TargetPosition.FromNative(sample.TargetPosition, keysOnly: false);
                TargetVelocity.FromNative(sample.TargetVelocity, keysOnly: false);
                IPPosition.FromNative(sample.IPPosition, keysOnly: false);
                IPVelocity.FromNative(sample.IPVelocity, keysOnly: false);
                sample.TargetType = TargetType;
                TargetPositionSTD.FromNative(sample.TargetPositionSTD, keysOnly: false);
                TargetVelocitySTD.FromNative(sample.TargetVelocitySTD, keysOnly: false);
                sample.MunitionObjectNumber = MunitionObjectNumber;
                sample.DelayTime = DelayTime;
                sample.LSSNumber = LSSNumber;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                TargetPosition.Initialize(allocatePointers, allocateMemory);
                TargetVelocity.Initialize(allocatePointers, allocateMemory);
                IPPosition.Initialize(allocatePointers, allocateMemory);
                IPVelocity.Initialize(allocatePointers, allocateMemory);
                TargetType = (global::ENUM.ThreatType) (0);
                TargetPositionSTD.Initialize(allocatePointers, allocateMemory);
                TargetVelocitySTD.Initialize(allocatePointers, allocateMemory);
                MunitionObjectNumber = (ushort) (0);
                DelayTime = (ushort) (0);
                LSSNumber = (byte) (0);
            }

            public void ToNative(global::MSG.TargetInformation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                TargetPosition.ToNative(sample.TargetPosition, keysOnly: false);
                TargetVelocity.ToNative(sample.TargetVelocity, keysOnly: false);
                IPPosition.ToNative(sample.IPPosition, keysOnly: false);
                IPVelocity.ToNative(sample.IPVelocity, keysOnly: false);
                TargetType = sample.TargetType;
                TargetPositionSTD.ToNative(sample.TargetPositionSTD, keysOnly: false);
                TargetVelocitySTD.ToNative(sample.TargetVelocitySTD, keysOnly: false);
                MunitionObjectNumber = sample.MunitionObjectNumber;
                DelayTime = sample.DelayTime;
                LSSNumber = sample.LSSNumber;
            }
        }

        internal class TargetInformationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.TargetInformation, TargetInformationUnmanaged>
        {

            internal TargetInformationPlugin() : base("global::MSG.TargetInformation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // TargetInformation struct
                var TargetInformationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("TargetPosition", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("TargetVelocity", global::STRUCT.Velocity8Support.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("IPPosition", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("IPVelocity", global::STRUCT.Velocity8Support.Instance.GetDynamicTypeInternal(isPublic), id: 4),
                    new StructMember("TargetType", global::ENUM.ThreatTypeSupport.Instance.GetDynamicTypeInternal(isPublic), id: 5),
                    new StructMember("TargetPositionSTD", global::STRUCT.StandardDeviation2Support.Instance.GetDynamicTypeInternal(isPublic), id: 6),
                    new StructMember("TargetVelocitySTD", global::STRUCT.StandardDeviation2Support.Instance.GetDynamicTypeInternal(isPublic), id: 7),
                    new StructMember("MunitionObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 8),
                    new StructMember("DelayTime", dtf.GetPrimitiveType<ushort>(), id: 9),
                    new StructMember("LSSNumber", dtf.GetPrimitiveType<byte>(), id: 10)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<TargetInformationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::TargetInformation")
                    .AddMembers(TargetInformationStructMembers));

                return result;
            }
        }
    }
    public class TargetInformationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.TargetInformation>
    {
        public TargetInformationSupport() : base(
            new Implementation.TargetInformationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.TargetInformationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static TargetInformationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<TargetInformationSupport, global::MSG.TargetInformation>();

    }

    namespace Implementation
    {

        public struct TrackNumberInfoUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.TrackNumberInfo>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private ushort ObjectNumber;
            private ushort RDRTrackNumber;
            private ushort ECSTrackNumber;
            private ushort Link16TrackNumber;
            private ushort MDILTrackNumber;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.TrackNumberInfo sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.ObjectNumber = ObjectNumber;
                sample.RDRTrackNumber = RDRTrackNumber;
                sample.ECSTrackNumber = ECSTrackNumber;
                sample.Link16TrackNumber = Link16TrackNumber;
                sample.MDILTrackNumber = MDILTrackNumber;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                ObjectNumber = (ushort) (0);
                RDRTrackNumber = (ushort) (0);
                ECSTrackNumber = (ushort) (0);
                Link16TrackNumber = (ushort) (0);
                MDILTrackNumber = (ushort) (0);
            }

            public void ToNative(global::MSG.TrackNumberInfo sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                ObjectNumber = sample.ObjectNumber;
                RDRTrackNumber = sample.RDRTrackNumber;
                ECSTrackNumber = sample.ECSTrackNumber;
                Link16TrackNumber = sample.Link16TrackNumber;
                MDILTrackNumber = sample.MDILTrackNumber;
            }
        }

        internal class TrackNumberInfoPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.TrackNumberInfo, TrackNumberInfoUnmanaged>
        {

            internal TrackNumberInfoPlugin() : base("global::MSG.TrackNumberInfo", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // TrackNumberInfo struct
                var TrackNumberInfoStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("ObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 1),
                    new StructMember("RDRTrackNumber", dtf.GetPrimitiveType<ushort>(), id: 2),
                    new StructMember("ECSTrackNumber", dtf.GetPrimitiveType<ushort>(), id: 3),
                    new StructMember("Link16TrackNumber", dtf.GetPrimitiveType<ushort>(), id: 4),
                    new StructMember("MDILTrackNumber", dtf.GetPrimitiveType<ushort>(), id: 5)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<TrackNumberInfoUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::TrackNumberInfo")
                    .AddMembers(TrackNumberInfoStructMembers));

                return result;
            }
        }
    }
    public class TrackNumberInfoSupport : Rti.Dds.Topics.TypeSupport<global::MSG.TrackNumberInfo>
    {
        public TrackNumberInfoSupport() : base(
            new Implementation.TrackNumberInfoPlugin(),
            new Lazy<DynamicType>(() =>Implementation.TrackNumberInfoPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static TrackNumberInfoSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<TrackNumberInfoSupport, global::MSG.TrackNumberInfo>();

    }

    namespace Implementation
    {

        public struct SetLogUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.SetLog>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private NativeSeq LogSaveNetworkList;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                LogSaveNetworkList.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.SetLog sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                LogSaveNetworkList.FromNative((Sequence<global::ENUM.NetworkID>) sample.LogSaveNetworkList);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                LogSaveNetworkList.Initialize<global::ENUM.NetworkID >(max: ((int)20), absoluteMax: ((int)20), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.SetLog sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                LogSaveNetworkList.ToNative((Sequence<global::ENUM.NetworkID>) sample.LogSaveNetworkList);
            }
        }

        internal class SetLogPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.SetLog, SetLogUnmanaged>
        {

            internal SetLogPlugin() : base("global::MSG.SetLog", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // SetLog struct
                var SetLogStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("LogSaveNetworkList", tsf.CreateSequenceWithAccessInfo(dtf, global::ENUM.NetworkIDSupport.Instance.GetDynamicTypeInternal(isPublic), ((int)20)), id: 1)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<SetLogUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::SetLog")
                    .AddMembers(SetLogStructMembers));

                return result;
            }
        }
    }
    public class SetLogSupport : Rti.Dds.Topics.TypeSupport<global::MSG.SetLog>
    {
        public SetLogSupport() : base(
            new Implementation.SetLogPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SetLogPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SetLogSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SetLogSupport, global::MSG.SetLog>();

    }

    namespace Implementation
    {

        public struct SetConfigurationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.SetConfiguration>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private NativeSeq Config;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                Config.Destroy<global::STRUCT.Config8, global::STRUCT.Implementation.Config8Unmanaged>(optionalsOnly);
            }

            public void FromNative(global::MSG.SetConfiguration sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                Config.FromNative<global::STRUCT.Config8, global::STRUCT.Implementation.Config8Unmanaged>(sample.Config);
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                Config.Initialize<global::STRUCT.Config8 , global::STRUCT.Implementation.Config8Unmanaged >(max: ((int)50), absoluteMax: ((int)50), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.SetConfiguration sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                Config.ToNative<global::STRUCT.Config8, global::STRUCT.Implementation.Config8Unmanaged>(sample.Config);
            }
        }

        internal class SetConfigurationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.SetConfiguration, SetConfigurationUnmanaged>
        {

            internal SetConfigurationPlugin() : base("global::MSG.SetConfiguration", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // SetConfiguration struct
                var SetConfigurationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("Config", tsf.CreateSequenceWithAccessInfo(dtf, global::STRUCT.Config8Support.Instance.GetDynamicTypeInternal(isPublic), ((int)50)), id: 1)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<SetConfigurationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::SetConfiguration")
                    .AddMembers(SetConfigurationStructMembers));

                return result;
            }
        }
    }
    public class SetConfigurationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.SetConfiguration>
    {
        public SetConfigurationSupport() : base(
            new Implementation.SetConfigurationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SetConfigurationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SetConfigurationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SetConfigurationSupport, global::MSG.SetConfiguration>();

    }

    namespace Implementation
    {

        public struct SetScenarioUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.SetScenario>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private NativeString ScenarioName;
            private NativeString ScenarioVersion;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                ScenarioName.Destroy();
                ScenarioVersion.Destroy();
            }

            public void FromNative(global::MSG.SetScenario sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.ScenarioName = ScenarioName.FromNative();
                sample.ScenarioVersion = ScenarioVersion.FromNative();
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                ScenarioName.Initialize(size: ((int) 255), allocateMemory: allocateMemory);
                ScenarioVersion.Initialize(size: ((int) 255), allocateMemory: allocateMemory);
            }

            public void ToNative(global::MSG.SetScenario sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                ScenarioName.ToNative(sample.ScenarioName, ((int) 255));
                ScenarioVersion.ToNative(sample.ScenarioVersion, ((int) 255));
            }
        }

        internal class SetScenarioPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.SetScenario, SetScenarioUnmanaged>
        {

            internal SetScenarioPlugin() : base("global::MSG.SetScenario", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // SetScenario struct
                var SetScenarioStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("ScenarioName", dtf.CreateString(((int) 255)), id: 1),
                    new StructMember("ScenarioVersion", dtf.CreateString(((int) 255)), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<SetScenarioUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::SetScenario")
                    .AddMembers(SetScenarioStructMembers));

                return result;
            }
        }
    }
    public class SetScenarioSupport : Rti.Dds.Topics.TypeSupport<global::MSG.SetScenario>
    {
        public SetScenarioSupport() : base(
            new Implementation.SetScenarioPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SetScenarioPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SetScenarioSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SetScenarioSupport, global::MSG.SetScenario>();

    }

    namespace Implementation
    {

        public struct SetSimulationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.SetSimulation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private global::ENUM.SimulationStatus SetSim;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.SetSimulation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                sample.SetSim = SetSim;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                SetSim = (global::ENUM.SimulationStatus) (0);
            }

            public void ToNative(global::MSG.SetSimulation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                SetSim = sample.SetSim;
            }
        }

        internal class SetSimulationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.SetSimulation, SetSimulationUnmanaged>
        {

            internal SetSimulationPlugin() : base("global::MSG.SetSimulation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // SetSimulation struct
                var SetSimulationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("SetSim", global::ENUM.SimulationStatusSupport.Instance.GetDynamicTypeInternal(isPublic), id: 1)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<SetSimulationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::SetSimulation")
                    .AddMembers(SetSimulationStructMembers));

                return result;
            }
        }
    }
    public class SetSimulationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.SetSimulation>
    {
        public SetSimulationSupport() : base(
            new Implementation.SetSimulationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.SetSimulationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static SetSimulationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<SetSimulationSupport, global::MSG.SetSimulation>();

    }

    namespace Implementation
    {

        public struct CollisionUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.Collision>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private global::STRUCT.Implementation.RelativePosition8Unmanaged RelativePosition;
            private global::ENUM.CollisionStatus CollisionType;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                RelativePosition.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.Collision sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                RelativePosition.FromNative(sample.RelativePosition, keysOnly: false);
                sample.CollisionType = CollisionType;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                RelativePosition.Initialize(allocatePointers, allocateMemory);
                CollisionType = (global::ENUM.CollisionStatus) (0);
            }

            public void ToNative(global::MSG.Collision sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                RelativePosition.ToNative(sample.RelativePosition, keysOnly: false);
                CollisionType = sample.CollisionType;
            }
        }

        internal class CollisionPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.Collision, CollisionUnmanaged>
        {

            internal CollisionPlugin() : base("global::MSG.Collision", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // Collision struct
                var CollisionStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("RelativePosition", global::STRUCT.RelativePosition8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("CollisionType", global::ENUM.CollisionStatusSupport.Instance.GetDynamicTypeInternal(isPublic), id: 2)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<CollisionUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::Collision")
                    .AddMembers(CollisionStructMembers));

                return result;
            }
        }
    }
    public class CollisionSupport : Rti.Dds.Topics.TypeSupport<global::MSG.Collision>
    {
        public CollisionSupport() : base(
            new Implementation.CollisionPlugin(),
            new Lazy<DynamicType>(() =>Implementation.CollisionPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static CollisionSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<CollisionSupport, global::MSG.Collision>();

    }

    namespace Implementation
    {

        public struct MunitionDetonationUnmanaged : Rti.Dds.NativeInterface.TypePlugin.INativeTopicType<global::MSG.MunitionDetonation>
        {

            private global::STRUCT.Implementation.MessageHeader8Unmanaged Header;
            private global::STRUCT.Implementation.Position8Unmanaged DetonationPosition;
            private global::STRUCT.Implementation.Velocity8Unmanaged FinalVelocity;
            private global::STRUCT.Implementation.RelativePosition8Unmanaged RelativePosition;
            private global::ENUM.DetonationType DetonationResult;
            private ushort MunitionObjectNumber;
            private ushort ThreatObjectNumber;

            public void Destroy(bool optionalsOnly)
            {
                if (optionalsOnly)
                {
                    return;
                }
                Header.Destroy(optionalsOnly);
                DetonationPosition.Destroy(optionalsOnly);
                FinalVelocity.Destroy(optionalsOnly);
                RelativePosition.Destroy(optionalsOnly);
            }

            public void FromNative(global::MSG.MunitionDetonation sample, bool keysOnly = false)
            {

                Header.FromNative(sample.Header, keysOnly: false);
                DetonationPosition.FromNative(sample.DetonationPosition, keysOnly: false);
                FinalVelocity.FromNative(sample.FinalVelocity, keysOnly: false);
                RelativePosition.FromNative(sample.RelativePosition, keysOnly: false);
                sample.DetonationResult = DetonationResult;
                sample.MunitionObjectNumber = MunitionObjectNumber;
                sample.ThreatObjectNumber = ThreatObjectNumber;
            }

            public void Initialize(bool allocatePointers = true, bool allocateMemory = true)
            {
                Header.Initialize(allocatePointers, allocateMemory);
                DetonationPosition.Initialize(allocatePointers, allocateMemory);
                FinalVelocity.Initialize(allocatePointers, allocateMemory);
                RelativePosition.Initialize(allocatePointers, allocateMemory);
                DetonationResult = (global::ENUM.DetonationType) (0);
                MunitionObjectNumber = (ushort) (0);
                ThreatObjectNumber = (ushort) (0);
            }

            public void ToNative(global::MSG.MunitionDetonation sample, bool keysOnly = false)
            {
                Header.ToNative(sample.Header, keysOnly: false);
                DetonationPosition.ToNative(sample.DetonationPosition, keysOnly: false);
                FinalVelocity.ToNative(sample.FinalVelocity, keysOnly: false);
                RelativePosition.ToNative(sample.RelativePosition, keysOnly: false);
                DetonationResult = sample.DetonationResult;
                MunitionObjectNumber = sample.MunitionObjectNumber;
                ThreatObjectNumber = sample.ThreatObjectNumber;
            }
        }

        internal class MunitionDetonationPlugin : Rti.Dds.NativeInterface.TypePlugin.InterpretedTypePlugin<global::MSG.MunitionDetonation, MunitionDetonationUnmanaged>
        {

            internal MunitionDetonationPlugin() : base("global::MSG.MunitionDetonation", isKeyed: false, CreateDynamicType(isPublic: false))
            {
                xTypesComplianceMask = 0x0000068C;
            }

            public static DynamicType CreateDynamicType(bool isPublic = true)
            {
                var dtf = ServiceEnvironment.Instance.Internal.GetTypeFactory(isPublic);
                var tsf = ServiceEnvironment.Instance.Internal.TypeSupportFactory;

                // MunitionDetonation struct
                var MunitionDetonationStructMembers = new StructMember[]
                {
                    new StructMember("Header", global::STRUCT.MessageHeader8Support.Instance.GetDynamicTypeInternal(isPublic), id: 0),
                    new StructMember("DetonationPosition", global::STRUCT.Position8Support.Instance.GetDynamicTypeInternal(isPublic), id: 1),
                    new StructMember("FinalVelocity", global::STRUCT.Velocity8Support.Instance.GetDynamicTypeInternal(isPublic), id: 2),
                    new StructMember("RelativePosition", global::STRUCT.RelativePosition8Support.Instance.GetDynamicTypeInternal(isPublic), id: 3),
                    new StructMember("DetonationResult", global::ENUM.DetonationTypeSupport.Instance.GetDynamicTypeInternal(isPublic), id: 4),
                    new StructMember("MunitionObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 5),
                    new StructMember("ThreatObjectNumber", dtf.GetPrimitiveType<ushort>(), id: 6)
                };

                DynamicType result = tsf.CreateTypeWithAccessInfo<MunitionDetonationUnmanaged>(
                    dtf.BuildStruct()
                    .WithExtensibility(ExtensibilityKind.Extensible)
                    .WithName("MSG::MunitionDetonation")
                    .AddMembers(MunitionDetonationStructMembers));

                return result;
            }
        }
    }
    public class MunitionDetonationSupport : Rti.Dds.Topics.TypeSupport<global::MSG.MunitionDetonation>
    {
        public MunitionDetonationSupport() : base(
            new Implementation.MunitionDetonationPlugin(),
            new Lazy<DynamicType>(() =>Implementation.MunitionDetonationPlugin.CreateDynamicType(isPublic: true)))
        {
        }

        public static MunitionDetonationSupport Instance { get; } =
        ServiceEnvironment.Instance.Internal.TypeSupportFactory.CreateTypeSupport<MunitionDetonationSupport, global::MSG.MunitionDetonation>();

    }

} // namespace MSG


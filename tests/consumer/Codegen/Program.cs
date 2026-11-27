using A = DdsModules_GlobalA.Common;
using B = DdsModules_GlobalB.Common;
_ = new DdsModules_Nested.Outer.Common.NestedStatus();
_ = new DdsModules_Nested.Common.RootStatus();
_ = new DdsModules_Keyword.@event.Status();
var a = new A.Status { mode = DdsModules_GlobalA_GlobalMode.Idle };
var b = new B.Status { mode = DdsModules_GlobalB_GlobalMode.Idle };
if ((int)a.mode != 0 || (int)b.mode != 1) throw new Exception("Global enum collision");
if (A.StatusSupport.Instance.DynamicType.Name != "Common::Status" || B.StatusSupport.Instance.DynamicType.Name != "Common::Status")
    throw new Exception("Wire type name changed");
Console.WriteLine("Nested, escaped keyword, global enums and original wire names passed.");

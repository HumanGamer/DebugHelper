using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugHelperTester
{
    public class TestObjectStruct1Base
    {
        public int Var1;
        public int Var2;

        public TestObjectStruct1Base(int v1, int v2)
        {
            Var1 = v1;
            Var2 = v2;
        }

        public override string ToString()
        {
            return $"{{Var1: {Var1}, Var2: {Var2}}}";
        }
    }

    public class TestObjectStruct1 : TestObjectStruct1Base
    {
        public ulong Var3;
        public short Var4;

        public TestObjectStruct1(int v1, int v2, ulong v3, short v4) : base(v1, v2)
        {
            Var3 = v3;
            Var4 = v4;
        }

        public override string ToString()
        {
            return $"{{Var1: {Var1}, Var2: {Var2}, Var3: {Var3:X16}, Var4: {Var4}}}";
        }
    }

    public struct TestObjectStruct2
    {
        public uint Var1;
        public byte Var2;

        public TestObjectStruct2(uint v1, byte v2)
        {
            Var1 = v1;
            Var2 = v2;
        }

        public override string ToString()
        {
            return $"{{Var1: {Var1:X8}, Var2: {Var2:X2}}}";
        }
    }

    public struct TestObjectStruct3
    {
        public long Var1;
        public string Var2;

        public TestObjectStruct3(long v1, string v2)
        {
            Var1 = v1;
            Var2 = v2;
        }

        public override string ToString()
        {
            return $"{{Var1: {Var1}, Var2: {Var2}}}";
        }
    }

    public class TestObject
    {
        public string Str1;
        public ulong Val2;

        public List<object> Structs1;
        public Dictionary<string, TestObjectStruct2> Structs2;
        public TestObjectStruct3[,] Structs3;
        public int[] Structs4;

        public TestObject()
        {
            Structs1 = new List<object>();
            Structs2 = new Dictionary<string, TestObjectStruct2>();
            Structs3 = new TestObjectStruct3[2,2];
            Structs4 = new int[2];

            Str1 = "Pickle";
            Val2 = 0x345567843;
            
            //Structs1.Add(new TestObjectStruct1(7, 100, 0x444333222111, 420));
            //Structs1.Add(new TestObjectStruct1(48, 64, 0x777888999111, 360));
            //Structs1.Add(new TestObjectStruct1Base(32, 128));
            Structs1.Add(7);
            Structs1.Add((byte)3);

            Structs2.Add("Potato", new TestObjectStruct2(0x20, 0xFF));
            Structs2.Add("Sandwich", new TestObjectStruct2(0x10, 0x80));

            Structs3[0, 0] = new TestObjectStruct3(999, "Cheese");
            Structs3[0, 1] = new TestObjectStruct3(888, "Burger");
            Structs3[1, 0] = new TestObjectStruct3(777, "Tomato");
            Structs3[1, 1] = new TestObjectStruct3(555, "Onion");

            Structs4[0] = 42;
            Structs4[1] = 7;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Magic_RDR.Application;
using static Magic_RDR.RPF6.RPF6TOC;

namespace Magic_RDR.Viewers
{
    public partial class ShaderViewerForm : Form
    {
        private IOReader Reader;
        private ShaderDictionary Shader;

        [System.ComponentModel.DisplayName("Vertex Fragments")]
        public Fragment[] VSFragments { get; private set; }
        [System.ComponentModel.DisplayName("Pixel Fragments")]
        public Fragment[] PSFragments { get; private set; }
        [System.ComponentModel.DisplayName("Shader Params (Global?)")]
        public ShaderParameter[] ShaderParameters1 { get; private set; }
        [System.ComponentModel.DisplayName("Shader Params (Temp?)")]
        public ShaderParameter[] ShaderParameters2 { get; private set; }
        [System.ComponentModel.DisplayName("Effect Techniques")]
        public Technique[] Techniques { get; private set; }

        [System.ComponentModel.Browsable(false)]
        public List<Pass> passes = new List<Pass>();

        [System.ComponentModel.DisplayName("All Passes")]
        public Pass[] Passes
        {
            get { return passes.ToArray(); }
        }

        public ShaderViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Text = string.Format("MagicRDR - Shader Data Viewer [{0}]", entry.Entry.Name);

            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();

            byte[] data = null;
            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                data = DataUtils.DecompressZStandard(RPFFile.RPFIO.ReadBytes(file.SizeInArchive));
            else
                data = DataUtils.DecompressDeflate(RPFFile.RPFIO.ReadBytes(file.SizeInArchive), file.FlagInfo.GetTotalSize());

            Reader = new IOReader(new MemoryStream(data), IOReader.Endian.Little);
            Shader = new ShaderDictionary(Reader);
            propertyGrid1.SelectedObject = Shader;
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ShaderDictionary
        {
            [System.ComponentModel.DisplayName("Vertex Fragments")]
            public Fragment[] VSFragments { get; private set; }
            [System.ComponentModel.DisplayName("Pixel Fragments")]
            public Fragment[] PSFragments { get; private set; }
            [System.ComponentModel.DisplayName("Shader Params (Global?)")]
            public ShaderParameter[] ShaderParameters1 { get; private set; }
            [System.ComponentModel.DisplayName("Shader Params (Temp?)")]
            public ShaderParameter[] ShaderParameters2 { get; private set; }
            [System.ComponentModel.DisplayName("Effect Techniques")]
            public Technique[] Techniques { get; private set; }

            [System.ComponentModel.Browsable(false)]
            public List<Pass> passes = new List<Pass>();

            [System.ComponentModel.DisplayName("All Passes")]
            public Pass[] Passes
            {
                get { return passes.ToArray(); }
            }

            private IOReader Reader;

            public ShaderDictionary(object reader)
            {
                Reader = (IOReader)reader;
                uint Magic = Reader.ReadUInt32();
                uint VertexType = Reader.ReadUInt32();
                bool DrawBucket = Reader.ReadByte() != 0;

                byte FragmentCount;
                if (DrawBucket) //PresetParams
                {
                    string Unk_Name = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                    byte UnusedValue = Reader.ReadByte();
                    uint BucketValue = Reader.ReadUInt32();
                    FragmentCount = Reader.ReadByte();
                }
                else FragmentCount = Reader.ReadByte();

                if (FragmentCount > 0)
                {
                    VSFragments = new Fragment[FragmentCount];
                    for (int i = 0; i < FragmentCount; i++)
                    {
                        Fragment csf = new Fragment(Reader);
                        VSFragments[i] = csf;
                    }
                }

                if (Reader.BaseStream.Position >= Reader.BaseStream.Length)
                {
                    return;
                }

                FragmentCount = (byte)(Reader.ReadByte() - 1);
                if (FragmentCount > 0)
                {
                    PSFragments = new Fragment[FragmentCount];
                    for (int i = 0; i < FragmentCount; i++)
                    {
                        Fragment csf = new Fragment(Reader, i == 0);
                        PSFragments[i] = csf;
                    }
                }

                byte ShaderParameterCount = Reader.ReadByte();
                if (ShaderParameterCount > 0)
                {
                    ShaderParameters1 = new ShaderParameter[ShaderParameterCount];
                    for (int i = 0; i < ShaderParameterCount; i++)
                    {
                        ShaderParameter param = new ShaderParameter();
                        param.Read(Reader);
                        ShaderParameters1[i] = param;
                    }
                }

                /*ShaderParameterCount = Reader.ReadByte();
                if (ShaderParameterCount > 0)
                {
                    ShaderParameters2 = new ShaderParameter[ShaderParameterCount];
                    for (int i = 0; i < ShaderParameterCount; i++)
                    {
                        ShaderParameter param = new ShaderParameter();
                        param.Read(Reader);
                        ShaderParameters2[i] = param;
                    }
                }*/

                byte techniqueCount = Reader.ReadByte();
                if (techniqueCount > 0)
                {
                    Techniques = new Technique[techniqueCount];
                    for (int i = 0; i < techniqueCount; i++)
                    {
                        Technique tech = new Technique(Reader, this);
                        Techniques[i] = tech;
                    }
                }
                AssociateParameters();
            }

            private void AssociateParameters()
            {
                if (PSFragments != null && PSFragments.Length > 0)
                {
                    for (int i = 0; i < PSFragments.Length; i++)
                    {
                        if (PSFragments[i].Parameters != null && PSFragments[i].Parameters.Length > 0)
                        {
                            for (int j = 0; j < PSFragments[i].Parameters.Length; j++)
                            {
                                AssocParameterByName(PSFragments[i].Parameters[j]);
                            }
                        }
                    }
                }

                if (VSFragments != null && VSFragments.Length > 0)
                {
                    for (int i = 0; i < VSFragments.Length; i++)
                    {
                        if (VSFragments[i].Parameters != null && VSFragments[i].Parameters.Length > 0)
                        {
                            for (int j = 0; j < VSFragments[i].Parameters.Length; j++)
                            {
                                AssocParameterByName(VSFragments[i].Parameters[j]);
                            }
                        }
                    }
                }
            }

            private void AssocParameterByName(FragmentParameter param)
            {
                if (ShaderParameters1 == null || ShaderParameters2 == null)
                    return;

                for (int i = 0; i < ShaderParameters1.Length; i++)
                {
                    if (ShaderParameters1[i].Name == param.Name)
                    {
                        param.SetShaderParameter(ShaderParameters1[i], 1);
                    }
                }
                for (int i = 0; i < ShaderParameters2.Length; i++)
                {
                    if (ShaderParameters2[i].Name == param.Name)
                    {
                        System.Diagnostics.Debug.Assert(param.ShaderParameter == null);
                        param.SetShaderParameter(ShaderParameters2[i], 2);
                    }
                }
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class Fragment
        {
            [System.ComponentModel.DisplayName("Size in bytes")]

            public ushort size { get; private set; }
            [System.ComponentModel.Browsable(false)]

            public byte[] RawData { get; private set; }
            private IOReader Reader;

            public FragmentParameter[] Parameters { get; private set; }

            public Fragment(object reader, bool pixelShader = false)
            {
                Reader = (IOReader)reader;
                Read(pixelShader);
            }

            public void Read(bool pixelShader = false)
            {
                if (pixelShader)
                {
                    string shaderGroupName = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                    ushort unkValue1 = Reader.ReadUInt16();
                    byte unkValue2 = Reader.ReadByte();
                }
                string fragmentName = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                byte numParameters = Reader.ReadByte();

                if (numParameters > 0)
                {
                    Parameters = new FragmentParameter[numParameters];
                    for (int i = 0; i < numParameters; i++)
                    {
                        FragmentParameter csp = new FragmentParameter(Reader);
                        Parameters[i] = csp;
                    }
                }
                size = Reader.ReadUInt16();
                RawData = Reader.ReadBytes(size);
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class FragmentParameter
        {
            public enum FragmentRegisterType
            {
                Temp = 0, // Temporary Register File
                Input = 1, // Input Register File
                Const = 2, // Constant Register File
                Addr = 3, // Address Register (VS)
                Texture = 3, // Texture Register File (PS)
                RastOut = 4, // Rasterizer Register File
                AttrOut = 5, // Attribute Output Register File
                TexCrdOut = 6, // Texture Coordinate Output Register File
                Output = 6, // Output register file for VS3.0+
                ConstInt = 7, // Constant Integer Vector Register File
                ColorOut = 8, // Color Output Register File
                DepthOut = 9, // Depth Output Register File
                Sampler = 10, // Sampler State Register File
                Const2 = 11, // Constant Register File  2048 - 4095
                Const3 = 12, // Constant Register File  4096 - 6143
                Const4 = 13, // Constant Register File  6144 - 8191
                ConstBool = 14, // Constant Boolean register file
                Loop = 15, // Loop counter register file
                TempFloat16 = 16, // 16-bit float temp register file
                MiscType = 17, // Miscellaneous (single) registers.
                Label = 18, // Label
                Predicate = 19  // Predicate register
            }

            [System.ComponentModel.DisplayName("Register Type")]
            public FragmentRegisterType RegisterType { get; private set; }

            [System.ComponentModel.DisplayName("Register Index")]
            public ushort RegisterIndex { get; private set; }
            public string Name { get; private set; }

            [System.ComponentModel.DisplayName("Shader Parameter")]
            public ShaderParameter ShaderParameter { get; private set; }

            [System.ComponentModel.DisplayName("Const = 1, Temp = 2?")]
            public int ShaderParameterListIndex { get; private set; }


            public void SetShaderParameter(ShaderParameter p, int i)
            {
                ShaderParameter = p;
                ShaderParameterListIndex = i;
            }

            public FragmentParameter(object reader)
            {
                Name = ((IOReader)reader).ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, ((IOReader)reader).ReadByte());
            }

            public override string ToString()
            {
                return Name;
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class Pass
        {
            private ShaderDictionary Parent;

            public byte VSIndex { get; private set; }
            public byte PSIndex { get; private set; }
            public Fragment PSFragment { get; private set; }
            public Fragment VSFragment { get; private set; }
            public uint[] Unknown { get; private set; }

            public Pass(object reader, ShaderDictionary parent)
            {
                IOReader Reader = (IOReader)reader;
                Parent = parent;
                VSIndex = Reader.ReadByte();
                PSIndex = Reader.ReadByte();
                PSFragment = parent.PSFragments[PSIndex - 1 < 0 ? 0 : PSIndex - 1];
                VSFragment = parent.VSFragments[VSIndex];
                byte dataCount = Reader.ReadByte();

                if (dataCount > 0)
                {
                    Unknown = new uint[dataCount * 2];
                    for (int i = 0; i < dataCount; i++)
                    {
                        Unknown[i * 2] = Reader.ReadUInt32();
                        Unknown[i * 2 + 1] = Reader.ReadUInt32();
                    }
                }
                parent.passes.Add(this);
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class Technique
        {
            private ShaderDictionary Parent;
            public string Name { get; private set; }
            public Pass[] Passes { get; private set; }

            public Technique(object reader, ShaderDictionary parent)
            {
                IOReader Reader = (IOReader)reader;
                Parent = parent;
                Name = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                byte passesCount = Reader.ReadByte();

                if (passesCount > 0)
                {
                    /*Passes = new Pass[passesCount];
                    for (int i = 0; i < passesCount; i++)
                    {
                        Passes[i] = new Pass(Reader, parent);
                    }*/
                }
            }

            public override string ToString()
            {
                string retValue = Name;
                if (Passes != null) retValue += " (" + Passes.Length.ToString() + " Passes)";
                return retValue;
            }
        }

        #region Values
        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ShaderParameter
        {
            public enum ShaderParameterType
            {
                @int = 1,
                @float = 2,
                float2 = 3,
                float3 = 4,
                float4 = 5,
                sampler = 6,
                @bool = 7,
                float4x3 = 8,
                float4x4 = 9
            }

            public ShaderParameterType Type { get; private set; }
            public byte ArrayCount { get; private set; }
            public bool ValueIsArray { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            public byte ValueLength { get; private set; }
            public Value Value { get; private set; }
            public Annotation[] Annotations { get; private set; }

            public void ReadAnnotations(object reader)
            {
                byte annotationsCount = ((IOReader)reader).ReadByte();
                if (annotationsCount > 0)
                {
                    Annotations = new Annotation[annotationsCount];
                    for (int i = 0; i < annotationsCount; i++)
                    {
                        Annotations[i] = new Annotation(reader);
                    }
                }
            }

            private void ReadValue(IOReader br)
            {
                if (ValueLength == 0) return;

                switch (Type)
                {
                    case ShaderParameterType.@int:
                        Value = new ValueInt(br);
                        break;
                    case ShaderParameterType.@float:
                        Value = new ValueFloat(br);
                        break;
                    case ShaderParameterType.float2:
                        Value = new ValueFloat2(br);
                        break;
                    case ShaderParameterType.float3:
                        Value = new ValueFloat3(br);
                        break;
                    case ShaderParameterType.float4:
                        Value = new ValueFloat4(br);
                        break;
                    case ShaderParameterType.@bool:
                        Value = new ValueBool(br);
                        break;
                    case ShaderParameterType.float4x3:
                        Value = new ValueFloat4x3(br);
                        break;
                    case ShaderParameterType.float4x4:
                        Value = new ValueFloat4x4(br);
                        break;
                    case ShaderParameterType.sampler:
                        Value = new ValueSamplerState(br, ValueLength);
                        break;
                }
            }

            private void ReadValueArray(IOReader br)
            {
                if (ValueLength == 0) return;

                Value[] Values = new Value[ArrayCount];
                for (int i = 0; i < ArrayCount; i++)
                {
                    switch (Type)
                    {
                        case ShaderParameterType.@int:
                            Values[i] = new ValueInt(br);
                            break;
                        case ShaderParameterType.@float:
                            Values[i] = new ValueFloat(br);
                            break;
                        case ShaderParameterType.float2:
                            Values[i] = new ValueFloat2(br);
                            break;
                        case ShaderParameterType.float3:
                            Values[i] = new ValueFloat3(br);
                            break;
                        case ShaderParameterType.float4:
                            Values[i] = new ValueFloat4(br);
                            break;
                        case ShaderParameterType.@bool:
                            Values[i] = new ValueBool(br);
                            break;
                        case ShaderParameterType.float4x3:
                            Values[i] = new ValueFloat4x3(br);
                            break;
                        case ShaderParameterType.float4x4:
                            Values[i] = new ValueFloat4x4(br);
                            break;
                        case ShaderParameterType.sampler:
                            int len = ValueLength / ArrayCount;
                            Values[i] = new ValueSamplerState(br, len);
                            break;
                    }
                }
                Value = new ValueList(Values);
            }

            public void Read(object reader)
            {
                var Reader = reader as IOReader;
                Type = (ShaderParameterType)Reader.ReadByte();
                ArrayCount = Reader.ReadByte();
                ushort unkValue = Reader.ReadUInt16();

                if (ArrayCount > 0)
                {
                    ValueIsArray = true;
                }

                Name = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                Description = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                ReadAnnotations(Reader);

                ValueLength = Reader.ReadByte();
                //if (ValueIsArray)
                    //ReadValueArray(Reader);
                //else
                    //ReadValue(Reader);
            }

            public override string ToString()
            {
                string a = "";
                if (Annotations != null && Annotations.Length > 0)
                {
                    a = " (" + Annotations.Length.ToString() + ")";
                }
                return Description + a;
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class Annotation
        {
            public enum AnnotationType
            {
                @int,
                @float,
                @string
            }

            public string Name { get; private set; }
            public AnnotationType Type { get; private set; }
            public Value Value { get; private set; }
            public Annotation() { }

            public Annotation(object reader)
            {
                IOReader Reader = reader as IOReader;
                Name = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, Reader.ReadByte());
                Type = (AnnotationType)Reader.ReadByte();

                switch (Type)
                {
                    case AnnotationType.@int:
                        Value = new ValueInt(Reader);
                        break;
                    case AnnotationType.@float:
                        Value = new ValueFloat(Reader);
                        break;
                    case AnnotationType.@string:
                        Value = new ValueString(Reader);
                        break;
                    default:
                        break;
                }
            }

            public override string ToString()
            {
                return Name + " = " + Value.ToString();
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public abstract class Value
        {

        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueInt : Value
        {
            public int Value { get; private set; }

            public ValueInt(object reader)
            {
                Value = ((IOReader)reader).ReadInt32();
            }

            public override string ToString()
            {
                return "int";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueFloat : Value
        {
            public float Value { get; private set; }

            public ValueFloat(object reader)
            {
                Value = ((IOReader)reader).ReadSingle();
            }

            public override string ToString()
            {
                return "float";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueFloat2 : Value
        {
            public float X { get; private set; }
            public float Y { get; private set; }

            public ValueFloat2(object reader)
            {
                X = ((IOReader)reader).ReadSingle();
                Y = ((IOReader)reader).ReadSingle();
            }

            public override string ToString()
            {
                return "float2(" +
                    ", " + X.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", " + Y.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ")";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueFloat3 : Value
        {
            public float X { get; private set; }
            public float Y { get; private set; }
            public float Z { get; private set; }

            public ValueFloat3(object reader)
            {
                X = ((IOReader)reader).ReadSingle();
                Y = ((IOReader)reader).ReadSingle();
                Z = ((IOReader)reader).ReadSingle();
            }

            public override string ToString()
            {
                return "float3(" +
                    ", " + X.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", " + Y.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", " + Z.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ")";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueFloat4 : Value
        {
            public float X { get; private set; }
            public float Y { get; private set; }
            public float Z { get; private set; }
            public float W { get; private set; }

            public ValueFloat4(object reader)
            {
                X = ((IOReader)reader).ReadSingle();
                Y = ((IOReader)reader).ReadSingle();
                Z = ((IOReader)reader).ReadSingle();
                W = ((IOReader)reader).ReadSingle();
            }

            public override string ToString()
            {
                return "float4";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueBool : Value
        {
            public bool value { get; private set; }

            public ValueBool(object reader)
            {
                value = ((IOReader)reader).ReadInt32() != 0;
            }

            public override string ToString()
            {
                return "bool";
            }
        }

        public class ValueFloat4x3 : Value
        {
            public float _11 { get; private set; }
            public float _12 { get; private set; }
            public float _13 { get; private set; }

            public float _21 { get; private set; }
            public float _22 { get; private set; }
            public float _23 { get; private set; }

            public float _31 { get; private set; }
            public float _32 { get; private set; }
            public float _33 { get; private set; }

            public float _41 { get; private set; }
            public float _42 { get; private set; }
            public float _43 { get; private set; }

            public ValueFloat4x3(object reader)
            {
                var Reader = reader as IOReader;
                _11 = Reader.ReadSingle();
                _12 = Reader.ReadSingle();
                _13 = Reader.ReadSingle();

                _21 = Reader.ReadSingle();
                _22 = Reader.ReadSingle();
                _23 = Reader.ReadSingle();

                _31 = Reader.ReadSingle();
                _32 = Reader.ReadSingle();
                _33 = Reader.ReadSingle();

                _41 = Reader.ReadSingle();
                _42 = Reader.ReadSingle();
                _43 = Reader.ReadSingle();
            }

            public override string ToString()
            {
                return "float4x3";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueFloat4x4 : Value
        {
            public float M11 { get; private set; }
            public float M12 { get; private set; }
            public float M13 { get; private set; }
            public float M14 { get; private set; }

            public float M21 { get; private set; }
            public float M22 { get; private set; }
            public float M23 { get; private set; }
            public float M24 { get; private set; }

            public float M31 { get; private set; }
            public float M32 { get; private set; }
            public float M33 { get; private set; }
            public float M34 { get; private set; }

            public float M41 { get; private set; }
            public float M42 { get; private set; }
            public float M43 { get; private set; }
            public float M44 { get; private set; }

            public ValueFloat4x4(object reader)
            {
                var Reader = reader as IOReader;
                M11 = Reader.ReadSingle();
                M12 = Reader.ReadSingle();
                M13 = Reader.ReadSingle();
                M14 = Reader.ReadSingle();

                M21 = Reader.ReadSingle();
                M22 = Reader.ReadSingle();
                M23 = Reader.ReadSingle();
                M24 = Reader.ReadSingle();

                M31 = Reader.ReadSingle();
                M32 = Reader.ReadSingle();
                M33 = Reader.ReadSingle();
                M34 = Reader.ReadSingle();

                M41 = Reader.ReadSingle();
                M42 = Reader.ReadSingle();
                M43 = Reader.ReadSingle();
                M44 = Reader.ReadSingle();
            }

            public override string ToString()
            {
                return "float4x4";
            }
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueSamplerState : Value
        {
            public int[] Values { get; private set; }
            public string[] valuesString { get; private set; }

            IOReader Reader;

            public ValueSamplerState(object reader, int count)
            {
                Reader = (IOReader)reader;
                Read(count);
            }

            public void Read(int count)
            {
                System.Diagnostics.Debug.Assert(count % 2 == 0);
                Values = new int[count];
                List<string> test = new List<string>();

                for (int i = 0; i < count / 2; i++)
                {
                    Values[i * 2] = Reader.ReadInt32();
                    Values[i * 2 + 1] = Reader.ReadInt32();

                    SamplerStateType type = (SamplerStateType)Values[i * 2] + 1;
                    int value = Values[i * 2 + 1];

                    switch (type)
                    {
                        case SamplerStateType.AddressU:
                        case SamplerStateType.AddressV:
                        case SamplerStateType.AddressW:
                            test.Add(type.ToString() + "=" + ((TextureAddress)value).ToString());
                            break;
                        case SamplerStateType.BorderColor:
                            byte[] tmp = BitConverter.GetBytes(value);
                            string color = "r:" + (tmp[0] / 255f).ToString(System.Globalization.CultureInfo.InvariantCulture) +
                                ", g:" + (tmp[1] / 255f).ToString(System.Globalization.CultureInfo.InvariantCulture) +
                                ", b:" + (tmp[2] / 255f).ToString(System.Globalization.CultureInfo.InvariantCulture) +
                                ", a:" + (tmp[3] / 255f).ToString(System.Globalization.CultureInfo.InvariantCulture);
                            test.Add(type.ToString() + "=" + color);
                            break;
                        case SamplerStateType.MagFilter:
                        case SamplerStateType.MinFilter:
                        case SamplerStateType.MipFilter:
                            test.Add(type.ToString() + "=" + ((TextureFilterType)value).ToString());
                            break;
                        case SamplerStateType.MipMapLodBias:
                            test.Add(type.ToString() + "=" + BitConverter.ToSingle(BitConverter.GetBytes(value), 0).ToString(System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case SamplerStateType.MaxAnisotropy:
                            test.Add(type.ToString() + "=" + value.ToString());
                            break;
                        default:
                            break;
                    }
                }
                valuesString = test.ToArray();
            }

            public override string ToString()
            {
                return "SamplerState";
            }
        }

        public class ValueList : Value, IList<Value>
        {

            private List<Value> list;

            public ValueList()
            {
                list = new List<Value>();
            }

            public ValueList(int count)
            {
                list = new List<Value>(count);
            }

            public ValueList(Value[] array)
            {
                list = new List<Value>(array);
            }

            #region IList<ShaderParameter> Members

            public int IndexOf(Value item)
            {
                return list.IndexOf(item);
            }

            public void Insert(int index, Value item)
            {
                list.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                list.RemoveAt(index);
            }

            public Value this[int index]
            {
                get
                {
                    return list[index];
                }
                set
                {
                    list[index] = value;
                }
            }
            #endregion

            #region ICollection<ParameterValue> Members

            public void Add(Value item)
            {
                list.Add(item);
            }

            public void Clear()
            {
                list.Clear();
            }

            public bool Contains(Value item)
            {
                return list.Contains(item);
            }

            public void CopyTo(Value[] array, int arrayIndex)
            {
                list.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return list.Count; }
            }

            [System.ComponentModel.Browsable(false)]
            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(Value item)
            {
                return list.Remove(item);
            }

            #endregion

            #region IEnumerable<ShaderParameter> Members

            public IEnumerator<Value> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }

            #endregion
        }

        [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ValueString : Value
        {
            public string Value { get; private set; }

            public ValueString(object reader)
            {
                Value = ((IOReader)reader).ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, ((IOReader)reader).ReadByte());
            }

            public override string ToString()
            {
                return Value;
            }
        }

        #endregion

        public enum SamplerStateType
        {
            AddressU = 1,       // TextureAddress                   0
            AddressV = 2,       // TextureAddress                   1
            AddressW = 3,       // TextureAddress                   2
            BorderColor = 4,    //  D3DColor                        3
            MagFilter = 5,      // TextureFilterType                4
            MinFilter = 6,      // TextureFilterType                5
            MipFilter = 7,      // TextureFilterType                6
            MipMapLodBias = 8,  //  MIPMAPLODBIAS, default = 0      7
            MaxMipLevel = 9,
            MaxAnisotropy = 10,
            SrgbTexture = 11,
            ElementIndex = 12,
            DMapOffset = 13
        }

        public enum TextureAddress
        {
            Wrap = 1,
            Mirror = 2,
            Clamp = 3,
            Border = 4,
            MirrorOnce = 5
        }

        public enum TextureFilterType
        {
            None = 0,
            Point = 1,
            Linear = 2,
            Anisotropic = 3,
            PyramidalQuad = 6,
            GaussianQuad = 7,
            ConvolutionMono = 8
        }
    }
}

namespace Magic_RDR.Models
{
    public class Shader
    {
        public ShaderParam[] Parameters { get; set; }
        public byte ParameterCount { get; set; }
        public byte RenderBucket { get; set; }
        public ushort ParameterSize { get; set; }
        public ushort ParameterDataSize { get; set; }
        public uint ShaderHash { get; set; }
        public string ShaderName { get; set; }
        public uint RenderBucketMask { get; set; }

        public Shader(ShaderParam[] parameters, byte paramCount, byte renderBucket, ushort paramSize, ushort paramDataSize, uint shaderHash, string shaderName, uint renderBucketMask)
        {
            Parameters = parameters;
            ParameterCount = paramCount;
            RenderBucket = renderBucket;
            ParameterSize = paramSize;
            ParameterDataSize = paramDataSize;
            ShaderHash = shaderHash;
            ShaderName = shaderName;
            RenderBucketMask = renderBucketMask;
        }
    }

    public class ShaderParam
    {
        public byte RegisterCount { get; set; }
        public byte RegisterIndex { get; set; }
        public byte DataType { get; set; }
        public object Param { get; set; }

        public ShaderParam(byte registerCount, byte registerIndex, byte dataType, object parameters)
        {
            RegisterCount = registerCount;
            RegisterIndex = registerIndex;
            DataType = dataType;
            Param = parameters;
        }

        public static (string[], string) GetVertexType(string shaderName)
        {
            string[] declaration;
            switch (shaderName)
            {
                case "rage_curvedmodel":
                case "rdr2_alpha_bspec_ao_cloth":
                case "rdr2_binoculars_barrel":
                case "rdr2_bump_spec_ao_cloth":
                case "rdr2_low_lod":
                case "rdr2_low_lod_singlesided":
                case "rdr2_scope_barrel":
                case "rdr2_treerock_prototype":
                    declaration = new string[] { "Vector3 Position", "Vector3 Normal", "uint Colour", "Vector2 Texcoord0", "Vector2 Texcoord1", "Vector4 Tangents" };
                    return (declaration, "PNCTTX");
                case "rage_lightprepass":
                case "rdr2_footprint":
                case "rdr2_grass":
                case "rdr2_taa":
                    declaration = new string[] { "Vector3 Position", "Vector3 Normal", "uint Colour", "Vector2 Texcoord0", "Vector2 Texcoord1" };
                    return (declaration, "PNCTT");
                case "rdr2_alpha":
                case "rdr2_diffuse":
                case "rdr2_poster":
                    declaration = new string[] { "Vector3 Position", "uint BlendWeights", "uint BlendIndices", "Vector3 Normal", "uint Colour", "Vector2 Texcoord" };
                    return (declaration, "PBBNCT");
                case "rdr2_alpha_bspec_ao_cpv":
                case "rdr2_bump_ambocc_cpv":
                case "rdr2_bump_spec":
                case "rdr2_debris":
                case "rdr2_glass_notint":
                case "rdr2_glass_notint_nodistortion":
                    declaration = new string[] { "Vector3 Position", "uint BlendWeights", "uint BlendIndices", "Vector3 Normal", "uint Colour0", "Vector2 Texcoord", "Vector4 Tangents" };
                    return (declaration, "PBBNCTX");
                case "rdr2_alpha_cloth":
                case "rdr2_diffuse_cloth":
                    declaration = new string[] { "Vector3 Position", "Vector3 Normal", "uint Colour", "Vector2 Texcoord" };
                    return (declaration, "Default");
                case "rdr2_alpha_foliage":
                case "rdr2_alpha_foliage_no_fade":
                    declaration = new string[] { "Vector3 Position", "Vector3 Normal", "uint Colour0", "uint Colour1", "Vector2 Texcoord", "Vector4 Tangents" };
                    return (declaration, "PNCCTX");
                case "rdr2_atmoscatt":
                    declaration = new string[] { "Vector3 Position", "Vector2 Texcoord0", "Vector2 Texcoord1" };
                    return (declaration, "PTT");
                case "rdr2_beacon":
                case "rdr2_injury":
                case "rdr2_traintrack_low_lod":
                    declaration = new string[] { "Vector3 Position", "uint Colour", "Vector2 Texcoord0", "Vector2 Texcoord1" };
                    return (declaration, "PCT");
                case "rdr2_billboard":
                case "rdr2_binoculars_lens":
                case "rdr2_flattenterrain":
                case "rdr2_map":
                case "rdr2_postfx":
                case "rdr2_scope_lens":
                case "rdr2_scope_lens_distortion":
                    declaration = new string[] { "Vector3 Position", "Vector2 Texcoord" };
                    return (declaration, "PT");
                case "rdr2_breakableglass":
                case "rdr2_bump_spec_ao_dirt_cloth":
                case "rdr2_layer_2_nospec_ambocc_decal":
                case "rdr2_low_lod_decal":
                    declaration = new string[] { "Vector3 Position", "Vector3 Normal", "uint Colour", "Vector2 Texcoord0", "Vector2 Texcoord1", "Vector2 Texcoord2", "Vector4 Tangents" };
                    return (declaration, "PNCTTTX");
                case "rdr2_bump":
                case "rdr2_bump_ambocc_alphaao":
                case "rdr2_bump_spec_alpha":
                case "rdr2_bump_spec_ambocc_cpv":
                case "rdr2_low_lod_nodirt":
                case "rdr2_low_lod_nodirt_singlesided":
                case "rdr2_terrain":
                case "rdr2_terrain4":
                case "rdr2_terrain_shoreline":
                case "rdr2_traintrack":
                    declaration = new string[] { "Vector3 Position", "uint Normal", "uint Colour", "ushort TexcoordX", "ushort TexcoordY", "ushort TangentX", "ushort TangentY", "ushort TangentZ", "ushort TangentW" };
                    return (declaration, "PCCH2H4");
                case "rdr2_door_glow":
                case "rdr2_lod4_water":
                case "rdr2_terrain_low_lod":
                case "rdr2_window_glow":
                    declaration = new string[] { "Vector3 Position", "Vector3 Normal", "uint Colour" };
                    return (declaration, "PNC");
                case "rdr2_layer_2_nospec_ambocc":
                case "rdr2_layer_2_nospec_ambocc_bridge":
                    declaration = new string[] { "Vector3 Position", "uint BlendWeights", "uint BlendIndices", "Vector3 Normal", "uint Colour0", "Vector2 Texcoord0", "Vector2 Texcoord1", "Vector2 Texcoord2", "Vector4 Tangents" };
                    return (declaration, "PBBNCTTTX");
                case "rdr2_river_water":
                case "rdr2_river_water_joint":
                    declaration = new string[4] { "Vector3 Position", "uint Colour", "Vector3 Texcoord0", "Vector3 Texcoord1" };
                    return (declaration, "PCTT");
                case "rdr2_shadowonly":
                    declaration = new string[] { "Vector3 Position", "uint Colour0", "uint Colour1" };
                    return (declaration, "PCC");
                case "rdr2_alpha_bspec_ao":
                case "rdr2_alpha_bspec_ao_shared":
                case "rdr2_bump_ambocc":
                case "rdr2_glass_nodistortion_bump_spec_ao":
                case "rdr2_glow":
                    declaration = new string[] { "Vector3 Position", "uint BlendWeights", "uint BlendIndices", "Vector3 Normal", "uint Colour0", "Vector2 Texcoord0", "Vector2 Texcoord1", "Vector4 Tangents" };
                    return (declaration, "PBBNCTTX");
                default:
                    declaration = new string[] { "Not implemented yet" };
                    return (declaration, "Unknown");
            }
        }

        public static string[] GetParameterType(string vertexType)
        {
            switch (vertexType)
            {
                case "PCCH2H4":
                    return new string[] { "TextureSampler", "Colors", "physicsmaterial" };
                case "PNCTTX":
                    return new string[] { "TextureSampler", "TextureSampler2", "Colors", "dirtMapFactor", "physicsmaterial" };
                case "PNCTTTX":
                    return new string[] { "TextureSampler", "TextureSampler3", "BumpSampler", "AmbOccSampler", "Colors", "bumpiness", "specularColorFactor", "specularFactor", "physicsmaterial" };
                case "PBBNCTTTX":
                    return new string[] { "TextureSampler", "TextureSampler2", "BumpSampler", "AmbOccSampler", "Colors", "bumpiness", "specularColorFactor", "specularFactor", "specularDirtMapMax", "specularDirtMapMin", "specularDirtMapOffset", "specularDirtMapInfluence", "dirtMapFactor", "FresnelTerm", "specularGreenBias", "specularGreenScale", "physicsmaterial" };
                case "PBBNCTTX":
                    return new string[] { "TextureSampler", "BumpSampler", "AmbOccSampler", "Colors", "bumpiness", "specularColorFactor", "specularFactor", "FresnelTerm", "specularGreenBias", "specularGreenScale", "physicsmaterial" };
                default:
                    return new string[] { "Not implemented yet" };
            }
        }
    }
}

Shader "3/VizCurvature"
{
    Properties
    {
        [Enum(f3BC.Editor.OutputType)]_Output ("Output", Int) = 7
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 5.0
            struct appdata
            {
                float4 vertex : POSITION;
                float2 color : COLOR;
                float2 tex0 : TEXCOORD0;
                float2 tex1 : TEXCOORD1;
                float2 tex2 : TEXCOORD2;
                float2 tex3 : TEXCOORD3;
                float2 tex4 : TEXCOORD4;
                float2 tex5 : TEXCOORD5;
                float2 tex6 : TEXCOORD6;
                float2 tex7 : TEXCOORD7;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 tex0 : TEXCOORD0;
                float2 tex1 : TEXCOORD1;
                float2 tex2 : TEXCOORD2;
                float2 tex3 : TEXCOORD3;
                float2 tex4 : TEXCOORD4;
                float2 tex5 : TEXCOORD5;
                float2 tex6 : TEXCOORD6;
                float2 tex7 : TEXCOORD7;
                float2 color : TEXCOORD8;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.tex0 = v.tex0;
                o.tex1 = v.tex1;
                o.tex2 = v.tex2;
                o.tex3 = v.tex3;
                o.tex4 = v.tex4;
                o.tex5 = v.tex5;
                o.tex6 = v.tex6;
                o.tex7 = v.tex7;
                o.color = v.color;
                return o;
            }

            int _Output;

            float2 frag(v2f i) : SV_Target
            {
                switch (_Output)
                {
                case 8:
                    return i.color;
                    break;
                case 0:
                    return i.tex0;
                    break;
                case 1:
                    return i.tex1;
                    break;
                case 2:
                    return i.tex2;
                    break;
                case 3:
                    return i.tex3;
                    break;
                case 4:
                    return i.tex4;
                    break;
                case 5:
                    return i.tex5;
                    break;
                case 6:
                    return i.tex6;
                    break;
                case 7:
                    return i.tex7;
                    break;
                default:
                    return float2(0, 0);
                }
            }
            ENDCG
        }
    }
}
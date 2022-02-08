Shader "3/VizCurvature"
{
    Properties
    {
        [Enum(f3BC.Editor.Dest)] _Dest ("Dest", Int) = 0
        [Enum(f3BC.Editor.UVSet)] _UVSet ("UVSet", Int) = 0
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
                float2 curvature : TEXCOORD0;
            };

            int _UVSet;
            int _Dest;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                switch (_Dest)
                {
                case 0: o.curvature = v.color;
                    break;
                case 1:
                    switch (_UVSet)
                    {
                    case 0: o.curvature = v.tex0;
                        break;
                    case 1: o.curvature = v.tex1;
                        break;
                    case 2: o.curvature = v.tex2;
                        break;
                    case 3: o.curvature = v.tex3;
                        break;
                    case 4: o.curvature = v.tex4;
                        break;
                    case 5: o.curvature = v.tex5;
                        break;
                    case 6: o.curvature = v.tex6;
                        break;
                    case 7: o.curvature = v.tex7;
                        break;
                    default: o.curvature = 0;
                        break;
                    }
                    break;
                default: o.curvature = 0;
                    break;
                }
                return o;
            }

            float2 frag(v2f i) : SV_Target
            {
                return i.curvature;
            }
            ENDCG
        }
    }
}
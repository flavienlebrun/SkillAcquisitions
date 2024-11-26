Shader "Unlit/Unlit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float4 _MainTex_ST;
			uniform sampler2D _MainTex;
			struct appdata_t
			{
				float4 vertex :POSITION0;
				float2 texcoord :TEXCOORD0;
			};

			struct OUT_Data_Vert
			{
				float2 texcoord :TEXCOORD0;
				float4 vertex :SV_POSITION;
			};

			struct v2f
			{
				float2 texcoord :TEXCOORD0;
				float4 vertex :SV_POSITION;
			};

			struct OUT_Data_Frag
			{
				float4 color :SV_Target;
			};

			float4 u_xlat0;
			float4 u_xlat1;

			OUT_Data_Vert vert(appdata_t IN)
			{
				OUT_Data_Vert OUT;
				OUT.texcoord.xy = TRANSFORM_TEX(IN.texcoord.xy, _MainTex);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				return OUT;
			}

		
			OUT_Data_Frag frag(v2f IN)
			{
				OUT_Data_Frag OUT;
				OUT.color = tex2D(_MainTex, IN.texcoord.xy);
				return OUT;
			}
			ENDCG
		}
	}
}


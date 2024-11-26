Shader "Square/BorderColorFilter"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}

	_CrossMagentaInput("CrossMagentaInput", Color) = (1,1,1,1)
    _OutputColor("OutputColor", Color) = (1,1,1,1)
  }
  SubShader
  {
    Pass 
    {
      CGPROGRAM
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"
      
      #define CODE_BLOCK_VERTEX

	  uniform sampler2D _MainTex;

      uniform float4 _CrossMagentaInput;
      uniform float4 _OutputColor;

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

      OUT_Data_Vert vert(appdata_t IN)
      {
          OUT_Data_Vert OUT;
          OUT.texcoord.xy = IN.texcoord.xy;
          OUT.vertex = UnityObjectToClipPos(IN.vertex);
          return OUT;
      }
      
	  float4 OutputTexture;

	  float4 frag(v2f IN) : SV_Target
      {

		float4 OUT;
	  
		OutputTexture = tex2D(_MainTex, IN.texcoord.xy);

		// CrossMagenta

		if (OutputTexture.r > _CrossMagentaInput.r && OutputTexture.b > _CrossMagentaInput.b)
		{
			OutputTexture = _OutputColor;
		}

		OUT = OutputTexture;
		  
        return OUT;
      }   
      ENDCG     
    } 
  }
}

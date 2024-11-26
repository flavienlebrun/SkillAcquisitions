Shader "Square/MainColorFilter"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}

	_CrossGreenInput("CrossGreenInput", Color) = (1,1,1,1)
	_CrossGreenOutput("CrossGreenOutput", Color) = (1,1,1,1)

	// Vous pouvez créer d'autre couleur pour d'autres éléments sauf le magenta

	_ColorReplaceBackground("ColorReplaceBackground", Color) = (1,1,1,1)
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

	  uniform float4 _CrossGreenInput;
	  uniform float4 _CrossGreenOutput;
	  uniform float4 _ColorReplaceBackground;

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

	  float4 frag(v2f IN) : SV_Target
      {
		float4 OutputTexture;
	  
		OutputTexture = tex2D(_MainTex, IN.texcoord.xy);

		// Si les pixels sont noirs 
		if (OutputTexture.r < 0.1f && OutputTexture.b < 0.1f && OutputTexture.g < 0.1f)
		{
			OutputTexture = _ColorReplaceBackground;
		}

		// CrossGreen
		if (OutputTexture.g > _CrossGreenInput.g && OutputTexture.a > _CrossGreenInput.a)
		{
			OutputTexture = _CrossGreenOutput;
		}
		  
        return OutputTexture;
      }   
      ENDCG     
    } 
  }
}

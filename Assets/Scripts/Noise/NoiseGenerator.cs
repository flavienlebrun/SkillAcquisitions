using UnityEngine;

/*
 * Les ressources utilisées pour réaliser le code :
 * https://www.youtube.com/watch?v=WP-Bm65Q-1Y&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=2
 * https://www.youtube.com/watch?v=MRNFcywkUSA&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=3
 * https://catlikecoding.com/unity/tutorials/noise/
 */

public class NoiseGenerator : MonoBehaviour
{
    [Header("NoiseConfig")]
    [SerializeField]
    private NoiseConfig NoiseConfig = null;

    [Header("Rendering")]
    [SerializeField]
    private RenderTexture outputRenderTexture = null;

    [Header("ReferenceOffset")]
    [SerializeField]
    private bool enabledReferenceOffset = false;

    [SerializeField]
    private GameObject offsetObject = null;

    ////////////////////////////////////////////////////////
    private TextureFormat textureFormat = TextureFormat.RGB24;
    private int resolutionX;
    private int resolutionY;
    private bool mipChain = false; 
    private bool linear = false;
    private TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    private FilterMode filterMode = FilterMode.Point;
    private int anisoLevel = 9;
    private float scale;
    private float frequency;
    private float amplitude;
    private int octaves;
    private float persistance;
    private float lacunarity;
    private int seed;
    private bool offsetDynamic;
    private int speed;
    private Vector2 offset;
    private Gradient coloring;
    private Vector3 curPos;

    private void Update()
    {
        SetupNoiseConfig();
        GenerateNoise();
    }

    private void SetupNoiseConfig()
    {
        textureFormat = NoiseConfig.textureFormat;
        resolutionX = NoiseConfig.resolutionX;
        resolutionY = NoiseConfig.resolutionY;
        mipChain = NoiseConfig.mipChain;
        linear = NoiseConfig.linear;
        wrapMode = NoiseConfig.wrapMode;
        filterMode = NoiseConfig.filterMode;
        anisoLevel = NoiseConfig.anisoLevel;

        scale = NoiseConfig.scale;
        frequency = NoiseConfig.frequency; 
        amplitude = NoiseConfig.amplitude;
        octaves = NoiseConfig.octaves;
        persistance = NoiseConfig.persistance;
        lacunarity = NoiseConfig.lacunarity;
        seed = NoiseConfig.seed;
        offsetDynamic = NoiseConfig.offsetDynamic;
        coloring = NoiseConfig.coloring;

        if (!offsetDynamic)
        {
            offset = NoiseConfig.offset;
        }
    }

    private void GenerateNoise()
    {
        float[,] noise = new float[resolutionX, resolutionY];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        if (enabledReferenceOffset)
        {
            if (curPos.x != offsetObject.transform.position.x)
            {            
                offset.x = Random.Range(0f, 10000f);
                float y = curPos.y;
                Vector3 position2 = offsetObject.transform.position;
                if (y != position2.y)
                {
                    offset.y = Random.Range(0f, 10000f);
                }
            }
            float x2 = offsetObject.transform.position.x;
            float y2 = offsetObject.transform.position.y;
            float z2 = offsetObject.transform.position.z;
            curPos = new Vector3(x2, y2, z2);

        }
        else if(offsetDynamic)
        {
            offset.x = Random.Range(0f, 10000f);
            offset.y = Random.Range(0f, 10000f);
        }

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = resolutionX / 2f;
        float halfHeight = resolutionY / 2f;

        for(int y = 0; y < resolutionY;y++)
        {
            for(int x = 0; x < resolutionX; x++)
            {
                float frequency2 = frequency;
                float amplitude2 = amplitude;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfWidth) / scale * 0.3f * frequency2 + octaveOffsets[i].x;
                    float sampleY = (y-halfHeight) / scale * frequency2 + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude2;

                    amplitude2 *= persistance;
                    frequency2 *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noise[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < resolutionY; y++)
        {
            for (int x = 0; x < resolutionX; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noise[x, y]);
            }
        }

        int width = noise.GetLength(0);
        int height = noise.GetLength(1);

        Texture2D texture = new Texture2D(width, height, textureFormat,mipChain, linear);
        texture.wrapMode = wrapMode;
        texture.filterMode = filterMode;
        texture.anisoLevel = NoiseConfig.anisoLevel;

        Color[] colorNoise = new Color[width * height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                colorNoise[y * width + x] = coloring.Evaluate(noise[x, y]);
            }
        }

        texture.SetPixels(colorNoise);
        texture.Apply();

        outputRenderTexture.enableRandomWrite = true;
        RenderTexture.active = outputRenderTexture;
        Graphics.Blit(texture,outputRenderTexture);
    }
}

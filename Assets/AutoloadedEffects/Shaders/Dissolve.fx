sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float dissolveIntensity;
float2 sampleOffset;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = sampleColor;
    float2 offset;
    offset.x = 4;
    offset.y = 4;
    
    // Sample noise texture for dithering
    float noise = tex2D(noiseTexture, coords + sampleOffset).r;
    
    color.a = 1;
    if (dissolveIntensity > noise)
        color.a = 0;
        
    return tex2D(baseTexture, coords) * color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
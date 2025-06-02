sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float dissolveIntensity;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = sampleColor;
    
    // Sample noise texture for dithering
    float noise = tex2D(noiseTexture, coords).r;
    
    float dissolveOpacity = 1;
    if (dissolveIntensity > noise)
        dissolveOpacity = 0;
        
    return tex2D(baseTexture, coords) * color * dissolveOpacity;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
sampler baseTexture : register(s0);
sampler AlphaMap : register(s2);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);   
    float4 texColor = tex2D(AlphaMap, coords);
    
    if (texColor.r == 0)
        return color * 0.5f;
    
    return color;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
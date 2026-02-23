sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);

float3 colors[10];
float strength;
float time;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    
    // Sample noise texture for dithering
    float4 noise = tex2D(noiseTexture, (coords * 0.75) + float2(1, 0) * time);
    float ditherValue = (noise.r - 0.5) * 0.3;
    
    // Get the pixel's luminance with dithering applied
    float avg = (color.r + color.g + color.b) / 3.0;
    avg += ditherValue; // Apply dither before quantization
    avg = saturate(avg); // Clamp to [0,1] range
    
    int maxColors = 10;
    avg *= (maxColors - 1); // Scale to [0, maxColors-1]
    
    // Get the two nearest palette indices
    int lowerIndex = (int) floor(avg);
    int upperIndex = min(lowerIndex + 1, maxColors - 1);
    float fraction = avg - floor(avg);
    
    // Use dithering to decide between lower and upper palette colors
    float threshold = noise.r; // Use noise as threshold for dithering decision
    int chosenIndex = (fraction > threshold) ? upperIndex : lowerIndex;
    
    // Ensure index is within bounds
    chosenIndex = clamp(chosenIndex, 0, maxColors - 1);
    
    return float4(
        lerp(color.r, colors[chosenIndex].r, strength),
        lerp(color.g, colors[chosenIndex].g, strength),
        lerp(color.b, colors[chosenIndex].b, strength),
        1.0
    );
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
#include smooth.cl #type0 #type1
#include utils.cl #type0 #type1

#type0 GetPerlinNoise(__global #type0* image, int idx, int channel, int channelCount, float maxValue, int width, int height, int depth, float persistence, int octaves)
{

	float amplitude = 1;
	float totalAmplitude = 0;
	#type1 result = ToFloatN(image[idx], maxValue);

	for(int i = octaves-1; i >= 0; i--)
	{
		int samplePeriod = 1 << (i + 1);
		float sampleFrequency = 1.0f / samplePeriod;
		result += ToFloatN(GetSmoothNoise(image, idx, channel, channelCount, maxValue, width, height, depth, samplePeriod, sampleFrequency), maxValue) * (#type1)(amplitude);
		totalAmplitude += amplitude;
		amplitude *= persistence;
	}

	result /= (#type1)(totalAmplitude);

	return FromFloatN(clamp(result, (#type1)(0.0f), (#type1)(255.0f)), maxValue);
}
__kernel void perlin(__global #type0* image, int3 dimensions, int channelCount, float maxValue, __global uchar* channelEnableState, float persistence, int octaves)
{
	int idx = get_global_id(0);
	int channel = (int)fmod((float)idx, (float)channelCount);
	if(channelEnableState[channel]==0)
	{
		return;
	}
	image[idx] = GetPerlinNoise(image, idx, channel, channelCount, maxValue, dimensions.x, dimensions.y, dimensions.z, persistence, octaves);
	
}
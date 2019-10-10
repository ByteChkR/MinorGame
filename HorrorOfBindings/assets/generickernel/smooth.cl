#include utils.cl #type0 #type1

#type0 GetSmoothNoise(__global #type0* image, int idx, int channel, int channelCount, float maxValue, int width, int height, int depth, int samplePeriod, float sampleFrequency)
{
	int index = idx / channelCount;
	int3 index3D = Get3DimensionalIndex(width, height, index);

	int sample_w0 = (index3D.x / samplePeriod) * samplePeriod;
	int sample_w1 = (int)fmod((float)(sample_w0 + samplePeriod), (float)width);
	#type1 horizontalBlend = (index3D.x - sample_w0) * sampleFrequency;

	int sample_h0 = (index3D.y / samplePeriod) * samplePeriod;
	int sample_h1 = (int)fmod((float)(sample_h0 + samplePeriod), (float)height);
	#type1 verticalBlend = (index3D.y - sample_h0) * sampleFrequency;

	int sample_d0 = (index3D.z / samplePeriod) * samplePeriod;
	int sample_d1 = (int)fmod((float)(sample_d1 + samplePeriod), (float)depth);
	#type1 depthBlend = (index3D.z - sample_d0) * sampleFrequency;

	int w0h0d0 = GetFlattenedIndex(sample_w0, sample_h0, sample_d0, width, height) * channelCount + channel;
	int w1h0d0 = GetFlattenedIndex(sample_w1, sample_h0, sample_d0, width, height) * channelCount + channel;
	int w0h1d0 = GetFlattenedIndex(sample_w0, sample_h1, sample_d0, width, height) * channelCount + channel;
	int w1h1d0 = GetFlattenedIndex(sample_w1, sample_h1, sample_d0, width, height) * channelCount + channel;
	int w0h0d1 = GetFlattenedIndex(sample_w0, sample_h0, sample_d1, width, height) * channelCount + channel;
	int w1h0d1 = GetFlattenedIndex(sample_w1, sample_h0, sample_d1, width, height) * channelCount + channel;
	int w0h1d1 = GetFlattenedIndex(sample_w0, sample_h1, sample_d1, width, height) * channelCount + channel;
	int w1h1d1 = GetFlattenedIndex(sample_w1, sample_h1, sample_d1, width, height) * channelCount + channel;

	#type1 top0 = (#type1)(Lerpf(ToFloatN(image[w0h0d0], maxValue), ToFloatN(image[w1h0d0], maxValue), horizontalBlend));
	#type1 top1 = (#type1)(Lerpf(ToFloatN(image[w0h0d1], maxValue), ToFloatN(image[w1h0d1], maxValue), horizontalBlend));
	#type1 bottom0 = (#type1)(Lerpf(ToFloatN(image[w0h1d0], maxValue), ToFloatN(image[w1h1d0], maxValue), horizontalBlend));
	#type1 bottom1 = (#type1)(Lerpf(ToFloatN(image[w0h1d1], maxValue), ToFloatN(image[w1h1d1], maxValue), horizontalBlend));
	#type1 top = (#type1)(Lerpf(top0, top1, depthBlend));
	#type1 bottom = (#type1)(Lerpf(bottom0, bottom1, depthBlend));

	return FromFloatN(Lerpf(top, bottom, verticalBlend), maxValue);


}


__kernel void smooth(__global #type0* image, int3 dimensions, int channelCount, float maxValue, __global uchar* channelEnableState, int octaves)
{


	int idx = get_global_id(0);
	int channel = (int)fmod((float)idx, channelCount);
	if(channelEnableState[channel]==0)
	{
		return;
	}

	int samplePeriod = 1 << octaves;
	float sampleFrequency = 1.0f / samplePeriod;

	image[idx] = GetSmoothNoise(image, idx, channel, channelCount, maxValue, dimensions.x, dimensions.y, dimensions.z, samplePeriod, sampleFrequency);
}
using System.Collections.Generic;
using System.Linq;

namespace AstroMiner.Utilities;

// Credit https://stackoverflow.com/a/20679895
public class FrameCounter
{
    public const int MaximumSamples = 100;

    private readonly Queue<float> _sampleBuffer = new();
    public long TotalFrames { get; private set; }
    public float TotalSeconds { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    public void Update(float deltaTime)
    {
        CurrentFramesPerSecond = 1.0f / deltaTime;

        _sampleBuffer.Enqueue(CurrentFramesPerSecond);

        if (_sampleBuffer.Count > MaximumSamples)
        {
            _sampleBuffer.Dequeue();
            AverageFramesPerSecond = _sampleBuffer.Average(i => i);
        }
        else
        {
            AverageFramesPerSecond = CurrentFramesPerSecond;
        }

        TotalFrames++;
        TotalSeconds += deltaTime;
    }
}
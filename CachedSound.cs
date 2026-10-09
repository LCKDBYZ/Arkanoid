using NAudio.Wave;

namespace Arkanoid {
    internal class CachedSound {
        public float[] AudioData { get; }
        public WaveFormat WaveFormat { get; }

        public CachedSound(string path) {
            using (var reader = new AudioFileReader(path)) {
                WaveFormat = reader.WaveFormat;
                var data = new List<float>();
                var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                int read;
                // Use the Span<float> overload
                while ((read = reader.Read(buffer)) > 0) {
                    data.AddRange(buffer.Take(read));
                }
                AudioData = data.ToArray();
            }
        }
    }
}

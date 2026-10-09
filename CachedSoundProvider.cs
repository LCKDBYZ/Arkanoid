using NAudio.Wave;

namespace Arkanoid {
    internal class CachedSoundSampleProvider : ISampleProvider {
        private readonly CachedSound cachedSound;
        private int position;

        public CachedSoundSampleProvider(CachedSound cachedSound) {
            this.cachedSound = cachedSound;
            position = 0;
        }

        public WaveFormat WaveFormat => cachedSound.WaveFormat;

        public int Read(Span<float> buffer) {
            int available = cachedSound.AudioData.Length - position;
            int toCopy = Math.Min(available, buffer.Length);
            if (toCopy <= 0) return 0;

            cachedSound.AudioData.AsSpan(position, toCopy).CopyTo(buffer);
            position += toCopy;
            return toCopy;
        }
    }
}

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Arkanoid {

    internal static class SoundManager {
        private const int MixerSampleRate = 44100;
        private const int MixerChannels = 2;

        private static readonly Dictionary<SoundEffect, CachedSound> sounds = new Dictionary<SoundEffect, CachedSound>();
        private static WaveOut outputDevice;
        private static MixingSampleProvider mixer;

        public static bool Enabled { get; set; } = true;

        public static void Init() {
            try {
                mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(MixerSampleRate, MixerChannels));
                mixer.ReadFully = true; // the mixer keeps running even when nothing is playing

                outputDevice = new WaveOut();
                outputDevice.BufferMilliseconds = 60; // low latency
                outputDevice.Init(mixer);
                

                outputDevice.Play();
                
            }
            catch {
                // No audio device available, the game runs without sound
                mixer = null;
                return;
            }

            Register(SoundEffect.MenuOpen, "menu_open.wav");
            Register(SoundEffect.BrickHit, "brick_hit.wav");
            Register(SoundEffect.BrickBreak, "brick_break.wav");
            Register(SoundEffect.PaddleHit, "paddle_hit.wav");
            Register(SoundEffect.WallHit, "wall_hit.wav");
            Register(SoundEffect.LoseLife, "lose_life.wav");
            Register(SoundEffect.GameOver, "game_over.wav");
            Register(SoundEffect.Win, "win.wav");
            Play(SoundEffect.MenuOpen);

        }

        private static void Register(SoundEffect effect, string fileName) {
            string path = Path.Combine(AppContext.BaseDirectory, "Sounds", fileName);
            if (!File.Exists(path)) return;

            try {
                sounds[effect] = new CachedSound(path);
            }
            catch {
                // Unsupported file, skip it
            }
        }

        public static void Play(SoundEffect effect) {
            if (!Enabled || mixer == null) return;

            if (sounds.TryGetValue(effect, out CachedSound sound)) {
                ISampleProvider input = new CachedSoundSampleProvider(sound);

                // The mixer has a fixed format, so every sound is converted to match it
                if (input.WaveFormat.Channels == 1)
                    input = new MonoToStereoSampleProvider(input);
                if (input.WaveFormat.SampleRate != MixerSampleRate)
                    input = new WdlResamplingSampleProvider(input, MixerSampleRate);

                input = new VolumeSampleProvider(input) {
                    Volume = 1.0f
                };

                mixer.AddMixerInput(input);
            }
        }
    }
   


}
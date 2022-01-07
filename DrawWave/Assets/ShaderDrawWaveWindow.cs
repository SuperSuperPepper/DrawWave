using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Editor
{
    public class ShaderDrawWaveWindow:EditorWindow
    {
        [MenuItem("Test/DrawWave")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShaderDrawWaveWindow>();
            window.Show();
        }

        private string wavPath
        {
            get { return Path.Combine(Application.dataPath, "message_education.wav"); }
        }

        private Rect _r;
        private RenderTexture _renderTexture;
        private AudioWaveTextureDrawer _drawer;


        private Texture2D _texture;
        private void OnEnable()
        {
            _drawer =new AudioWaveTextureDrawer(DrawWaveMode.Cpu);
            _r =new Rect(0,0,750,25);
            switch (_drawer.mode)
            {
                case DrawWaveMode.Cpu:
                    CpuDraw();
                    break;
                case DrawWaveMode.Shader:
                    ShaderDraw();
                    break;
                case DrawWaveMode.ComputeShader:
                    ComputeShaderDraw();                          
                    break;
            }
        }

        void CpuDraw()
        {
            var samples  = WavUtility.ToAudioData(wavPath);
            Profiler.BeginSample("drawWave");
            var wave = new float[750];
            _texture =new Texture2D(750, 25, TextureFormat.RGBA32, false);
            PaintWaveformSpectrum(_texture,ref samples,ref wave, Color.white, 0, 4,4);
            Profiler.EndSample();
        }

        void ShaderDraw()
        {
            var samples  = WavUtility.ToAudioData(wavPath);
            Profiler.BeginSample("drawWave");
            _drawer.GpuDrawWave(samples,0,samples.Length,Color.green,Color.yellow);
            Profiler.EndSample();
        }

        void ComputeShaderDraw()
        {
            var samples  = WavUtility.ToAudioData(wavPath);
            Profiler.BeginSample("drawWave");
            _drawer.GpuDrawWave(samples,0,samples.Length,Color.gray,Color.red);
            Profiler.EndSample();
        }

        private void OnGUI()
        {
            switch (_drawer.mode)
            {
                case DrawWaveMode.Cpu:
                    GUI.DrawTexture(_r,_texture);
                    break;
                case DrawWaveMode.Shader:
                case DrawWaveMode.ComputeShader:
                    GUI.DrawTexture(_r,_drawer.waveTexture);                    
                    break;
            }
        }
        
        
        public void PaintWaveformSpectrum(Texture2D _texture,ref float[] _samples,ref float[] _waveform, Color col,double startTime,double endTime,double allTime)
        {
            if (allTime<0.01f||_texture==null)
                return;

 
            var s1 = (int)(_samples.Length * (startTime / allTime));
            var s2 = (int)(_samples.Length * (endTime / (allTime)));
            if (s2 > _samples.Length) s2 = _samples.Length;
            _drawer.CpuDrawWave(_texture,_samples,_waveform,s1,s2,col);
        }
    }
}
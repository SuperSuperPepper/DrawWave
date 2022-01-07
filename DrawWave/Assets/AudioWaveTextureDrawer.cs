using UnityEditor;
using UnityEngine;

namespace Editor
{
    public enum DrawWaveMode
    {
        Cpu,
        Shader,
        ComputeShader
    }
    
    public class AudioWaveTextureDrawer
    {
        private readonly string _computeShaderFilePath ="Assets/AudioWaveComputeShader.compute";
        private readonly string _shaderName = "Hidden/Editor/AudioWave";
        private readonly int _width = 750;
        private readonly int _unitHeight = 25 ; //support 20 clip
        
        
        private Shader _waveShader;
        private Material _waveMaterial;
        private RenderTexture _renderTexture;
        
        
        //compute shader
        private ComputeShader _computeShader;
        private DrawWaveMode _mode;

        public RenderTexture waveTexture
        {
            get { return _renderTexture; }
        }

        public DrawWaveMode mode
        {
            get { return _mode; }
        }
        
        
        public AudioWaveTextureDrawer(DrawWaveMode mode =DrawWaveMode.ComputeShader)
        {
            _mode = mode;
            Init();
        }

        void Init()
        {
            switch (_mode)
            {
                case DrawWaveMode.Cpu :
                    CpuInit();
                    break;
                case DrawWaveMode.Shader:
                    ShaderInit();
                    break;
                case DrawWaveMode.ComputeShader:
                    ComputeShaderInit();
                    break;
            }
        }

        void CpuInit()
        {
            
        }

        void ShaderInit()
        {
            _waveShader =Shader.Find(_shaderName);
            _waveMaterial =new Material(_waveShader);
            _renderTexture = CreateRenderRexture(_width, _unitHeight,true);
        }

        void ComputeShaderInit()
        {
            _renderTexture = CreateRenderRexture(_width, _unitHeight,true);
            _computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(_computeShaderFilePath);
        }

     
        
        void Destroy()
        {
            switch (_mode)
            {
                case DrawWaveMode.Cpu :
                    CpuInit();
                    break;
                case DrawWaveMode.Shader:
                    _renderTexture.Release();
                    Object.DestroyImmediate(_renderTexture);
                    break;
                case DrawWaveMode.ComputeShader:
                    _renderTexture.Release();
                    Object.DestroyImmediate(_renderTexture);
                    break;
            }
        }

        private RenderTexture CreateRenderRexture(int width, int height, bool linear)
        {
            RenderTextureReadWrite readWrite = linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB;
            RenderTexture rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32, readWrite);
            rt.hideFlags = HideFlags.DontSave;
            rt.useMipMap = false;
            rt.filterMode = FilterMode.Point;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        public void GpuDrawWave(float[] samples,int preCount, int postCount,Color bgColor,Color waveColor)
        {
            switch (_mode)
            {
                case DrawWaveMode.Shader:
                    ShaderDrawWave(samples,preCount,postCount,bgColor,waveColor);
                    break;
                case DrawWaveMode.ComputeShader:
                    ComputeShaderDrawWave(samples,preCount,postCount,bgColor,waveColor);
                    break;
            }
            
        }
        
        void ShaderDrawWave(float[] samples,int s1, int s2,Color bg,Color waveColor)
        {
            int packSize = ( (s2-s1) / _width ) + 1;
            int s = 0;
            //shader can't set dynamic length floats. so wave length must set in this 
            float[] waveform =new float[_width];
            for (int i = s1; i <s2; i += packSize) {
                waveform[s] = Mathf.Abs(samples[i]);
                s++;
            }
            // set value
            
            _waveMaterial.SetFloatArray("_sampleArray",waveform);
            _waveMaterial.SetColor("_ColorBg",bg);
            _waveMaterial.SetColor("_ColorWave",waveColor);

            // set rt
            RenderTexture.active = _renderTexture;
            GL.Clear(true, true, new Color32(0, 0, 0,255 ));
            Graphics.Blit(null, _waveMaterial, 0);
            RenderTexture.active = null;
        }

        void ComputeShaderDrawWave(float[] samples,int preCount, int postCount,Color bg,Color waveColor)
        {
            int packSize = ( (postCount-preCount) / _width ) + 1;
            
            ComputeBuffer sampleBuffer =new ComputeBuffer(samples.Length,sizeof(float));
            sampleBuffer.SetData(samples);
            
            _computeShader.SetBuffer(0,"Samples",sampleBuffer);
            _computeShader.SetTexture(0,"Result",_renderTexture);
            _computeShader.SetInt("Offset",preCount);
            _computeShader.SetInt("PackSize",packSize);
            _computeShader.SetVector("BgColor",new Vector4(bg.r,bg.g,bg.b,bg.a));
            _computeShader.SetVector("WaveColor",new Vector4(waveColor.r,waveColor.g,waveColor.b,waveColor.a));
            int threadGroupsX = Mathf.CeilToInt(_renderTexture.width / 8f);
            int threadGroupsY = Mathf.CeilToInt(_renderTexture.height / 8f);
            
            _computeShader.Dispatch(0,threadGroupsX,threadGroupsY,1);
            sampleBuffer.Release();
            sampleBuffer.Dispose();
        }

        public void CpuDrawWave(Texture2D _texture,float[] samples,float[] waveform,int s1, int s2, Color waveColor)
        {
            int packSize = ( (s2-s1) / 750 ) + 1;
            int s = 0;
            for (int i = s1; i <s2; i += packSize) {
                waveform[s] = Mathf.Abs(samples[i]);
                s++;
            }
            Color bgColor =Color.gray;
            for (int x = 0; x < 750; x++) {
                for (int y = 0; y < 25; y++) {
                    _texture.SetPixel(x, y, bgColor);
                }
            }
 
            for (int x = 0; x < waveform.Length; x++) {
                for (int y = 0; y <= waveform[x] * ((float)25 * .75f); y++) {
                    _texture.SetPixel(x, ( 25 / 2 ) + y, waveColor);
                    _texture.SetPixel(x, ( 25 / 2 ) - y, waveColor);
                }
            }
            _texture.Apply();
        }
     
    }
}
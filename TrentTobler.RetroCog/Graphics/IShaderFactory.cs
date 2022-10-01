using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL;

namespace TrentTobler.RetroCog.Graphics;

public interface IShaderFactory
{
    public int Compile(string name);
}

public class ShaderFactory : IShaderFactory
{
    private IAssetProvider AssetProvider { get; }
    private ILogger Logger { get; }
    private IGlApi GlApi { get; }

    private static readonly IReadOnlyDictionary<string, ShaderType> _fileShaderTypeMap = new Dictionary<string, ShaderType>(StringComparer.OrdinalIgnoreCase)
    {
        ["vert.glsl"] = ShaderType.VertexShader,
        ["frag.glsl"] = ShaderType.FragmentShader,
    };

    public ShaderFactory(IAssetProvider assetProvider, IGlApi glApi, ILogger logger)
    {
        AssetProvider = assetProvider;
        GlApi = glApi;
        Logger = logger;
    }

    public IReadOnlyDictionary<ShaderType, string> Load(string name)
    {
        var files = AssetProvider.ListFiles(name).ToArray();
        var result = new Dictionary<ShaderType, string>();

        foreach (var file in files)
        {
            if (_fileShaderTypeMap.TryGetValue(file, out var shaderType))
            {
                result.Add(shaderType, AssetProvider.LoadString(name, file));
                continue;
            }
            Logger.LogWarning("{name}: skipping invalid shader file {file}", name, file);
        }

        return result;
    }

    public int Compile(IReadOnlyDictionary<ShaderType, string> sourceCode)
    {
        var program = GlApi.CreateProgram();
        if (program == 0)
        {
            Logger.LogError("{err}: CreateProgram failed", GlApi.GetError());
            return 0;
        }

        var shaders = new List<int>(sourceCode.Count);

        foreach (var shaderSource in sourceCode)
        {
            var key = shaderSource.Key;
            var shader = GlApi.CreateShader(key);
            Logger.LogInformation("Shader#{program}.{key}: Compiling ...", program, key);

            GlApi.ShaderSource(shader, shaderSource.Value);
            GlApi.CompileShader(shader);

            GlApi.GetShader(shader, ShaderParameter.CompileStatus, out var compileStatus);

            var shaderLog = GlApi.GetShaderInfoLog(shader);
            if (compileStatus == 0)
            {
                Logger.LogWarning("Shader#{program}.{key}: compile failure: {shaderLog}", program, key, shaderLog);
                GlApi.DeleteShader(shader);
            }
            else
            {
                Logger.LogInformation("Shader#{program}.{key}: compile success: {shaderLog}", program, key, shaderLog);
                GlApi.AttachShader(program, shader);
                shaders.Add(shader);
            }
        }

        Logger.LogInformation("Shader#{program}: Linking ...", program);
        GlApi.LinkProgram(program);
        GlApi.GetProgram(program, GetProgramParameterName.LinkStatus, out var linkStatus);
        var programLog = GlApi.GetProgramInfoLog(program);
        if (linkStatus == 0)
        {
            Logger.LogError("Shader#{program}: link failed: {programLog}", program, programLog);
        }
        else
        {
            Logger.LogInformation("Shader#{program}: link success: {programLog}", program, programLog);
        }

        foreach (var shader in shaders)
        {
            GlApi.DetachShader(program, shader);
            GlApi.DeleteShader(shader);
        }

        if (linkStatus == 0)
        {
            GlApi.DeleteProgram(program);
            return 0;
        }

        return program;
    }

    public int Compile(string name) => Compile(Load(name));
}

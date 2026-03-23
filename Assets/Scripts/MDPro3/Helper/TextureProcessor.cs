using SevenZip;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class TextureProcessor
{

    #region Common

    public enum BlendMode
    {
        Multiply,    // 相乘
        Replace,     // 替换
        Additive,    // 相加
        Subtractive, // 相减
        Overwrite    // 完全覆盖
    }

    private const string PATH_APPLY_MASK_ALPHA = "ComputeShader/ApplyMaskAlpha";
    private const string PATH_INVERT_ALPHA = "ComputeShader/InvertAlpha";
    private const string PATH_TEXTURE_RESIZE = "ComputeShader/TextureResize";

    private static ComputeShader shaderApplyMaskAlpha;
    private static ComputeShader ShaderApplyMaskAlpha => 
        shaderApplyMaskAlpha != null ? shaderApplyMaskAlpha 
        : shaderApplyMaskAlpha = LoadComputeShader(PATH_APPLY_MASK_ALPHA);
    private static ComputeShader shaderInvertMaskAlpha;
    private static ComputeShader ShaderInvertMaskAlpha =>
        shaderInvertMaskAlpha != null ? shaderInvertMaskAlpha
        : shaderInvertMaskAlpha = LoadComputeShader(PATH_INVERT_ALPHA);
    private static ComputeShader shaderTextureResize;
    private static ComputeShader ShaderTextureResize =>
        shaderTextureResize != null ? shaderTextureResize
        : shaderTextureResize = LoadComputeShader(PATH_TEXTURE_RESIZE);

    private static ComputeShader LoadComputeShader(string path)
    {
        ComputeShader shader = Resources.Load<ComputeShader>(path);
        return shader;
    }

    private static void DispatchComputeShader(ComputeShader shader, int kernel, Texture sourceTex)
    {
        DispatchComputeShader(shader, kernel, sourceTex.width, sourceTex.height);
    }

    private static void DispatchComputeShader(ComputeShader shader, int kernel, int width, int height)
    {
        shader.GetKernelThreadGroupSizes(kernel, out uint threadGroupSizeX, out uint threadGroupSizeY, out _);

        int threadGroupsX = Mathf.CeilToInt(width / (float)threadGroupSizeX);
        int threadGroupsY = Mathf.CeilToInt(height / (float)threadGroupSizeY);

        shader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
    }

    private static Texture2D RenderTextureToTexture2D(RenderTexture rt, Texture sourceTex)
    {
        Texture2D resultTex = new(sourceTex.width, sourceTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        resultTex.ReadPixels(new Rect(0, 0, sourceTex.width, sourceTex.height), 0, 0);
        resultTex.Apply();
        RenderTexture.active = null;
        return resultTex;
    }


    private static float GetChannelValue(Color color, int channel)
    {
        return channel switch
        {
            // R
            0 => color.r,
            // G
            1 => color.g,
            // B
            2 => color.b,
            // A
            3 => color.a,
            _ => color.a,// 默认使用alpha通道
        };
    }

    private static Color SampleTextureBilinear(Color32[] pixels, int width, int height, float u, float v)
    {
        if (width == 0 || height == 0)
            return Color.clear;

        // 计算双线性采样坐标
        float x = u * (width - 1);
        float y = v * (height - 1);

        int xFloor = Mathf.FloorToInt(x);
        int yFloor = Mathf.FloorToInt(y);
        int xCeil = Mathf.Min(xFloor + 1, width - 1);
        int yCeil = Mathf.Min(yFloor + 1, height - 1);

        // 获取四个采样点的颜色索引
        int index00 = yFloor * width + xFloor;
        int index10 = yFloor * width + xCeil;
        int index01 = yCeil * width + xFloor;
        int index11 = yCeil * width + xCeil;

        // 获取颜色（注意：数组可能越界，需要检查）
        Color32 c00 = (index00 >= 0 && index00 < pixels.Length) ? pixels[index00] : new Color32(0, 0, 0, 0);
        Color32 c10 = (index10 >= 0 && index10 < pixels.Length) ? pixels[index10] : new Color32(0, 0, 0, 0);
        Color32 c01 = (index01 >= 0 && index01 < pixels.Length) ? pixels[index01] : new Color32(0, 0, 0, 0);
        Color32 c11 = (index11 >= 0 && index11 < pixels.Length) ? pixels[index11] : new Color32(0, 0, 0, 0);

        // 计算权重
        float xWeight = x - xFloor;
        float yWeight = y - yFloor;

        // 将Color32转换为Color进行插值
        Color color00 = c00;
        Color color10 = c10;
        Color color01 = c01;
        Color color11 = c11;

        // 双线性插值
        Color top = Color.Lerp(color00, color10, xWeight);
        Color bottom = Color.Lerp(color01, color11, xWeight);
        return Color.Lerp(top, bottom, yWeight);
    }

    private static float ApplyBlendMode(float sourceAlpha, float maskValue, BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Multiply => sourceAlpha * maskValue,
            BlendMode.Replace => maskValue,
            BlendMode.Additive => Mathf.Clamp01(sourceAlpha + maskValue),
            BlendMode.Subtractive => Mathf.Clamp01(sourceAlpha - maskValue),
            BlendMode.Overwrite => sourceAlpha > 0 ? maskValue : 0,
            _ => sourceAlpha * maskValue,
        };
    }

    private static Texture2D EnsureTextureReadable(Texture2D texture)
    {
        if (texture == null) return null;
        if (texture.isReadable) return texture;

        RenderTexture rt = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.ARGB32
        );

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);

        Texture2D readableTex = new Texture2D(
            texture.width,
            texture.height,
            TextureFormat.ARGB32,
            false
        );

        readableTex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readableTex;
    }

    #endregion

    #region ApplyMaskToAlpha

    public static Texture2D ApplyMaskToAlpha(
        Texture2D sourceTex,
        Texture2D maskTex,
        BlendMode blendMode = BlendMode.Multiply,
        float threshold = 0.0f,
        bool invertMask = false,
        int maskChannel = 3
    )
    {
        // 1. 参数检查
        if (sourceTex == null || maskTex == null)
            throw new ArgumentNullException("Source and mask textures cannot be null.");

        // 2. 检查长宽比
        float sourceAspect = (float)sourceTex.width / sourceTex.height;
        float maskAspect = (float)maskTex.width / maskTex.height;

        if (Mathf.Abs(sourceAspect - maskAspect) > 0.01f)
        {
            Debug.LogWarning($"Aspect ratios don't match! Source: {sourceAspect:F3}, Mask: {maskAspect:F3}." +
                           " Results may be distorted.");
        }

        // 检查平台是否支持Compute Shader
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogWarning("Current platform does not support Compute Shaders. Falling back to CPU method.");
            return ApplyMaskToAlpha_CPU(sourceTex, maskTex, blendMode, threshold, invertMask, maskChannel);
        }

        // 3. 准备Compute Shader
        ComputeShader shader = ShaderApplyMaskAlpha;
        int kernelHandle = shader.FindKernel("CSMain");

        // 4. 创建输出纹理
        RenderTexture outputRT = RenderTexturePool.Get(sourceTex.width, sourceTex.height);

        // 5. 设置Compute Shader参数
        shader.SetTexture(kernelHandle, "SourceTex", sourceTex);
        shader.SetTexture(kernelHandle, "MaskTex", maskTex);
        shader.SetTexture(kernelHandle, "Result", outputRT);

        // 设置纹理尺寸
        shader.SetFloats("SourceSize", new float[] { sourceTex.width, sourceTex.height });
        shader.SetFloats("MaskSize", new float[] { maskTex.width, maskTex.height });

        // 设置混合模式和其他参数
        shader.SetInt("_BlendMode", (int)blendMode);
        shader.SetFloat("_Threshold", threshold);
        shader.SetFloat("_InvertMask", invertMask ? 1.0f : 0.0f);
        shader.SetInt("_MaskChannel", maskChannel);

        // 6. 调度线程组
        DispatchComputeShader(shader, kernelHandle, sourceTex);

        // 7. 转换回Texture2D
        Texture2D resultTex = RenderTextureToTexture2D(outputRT, sourceTex);

        // 8. 清理
        RenderTexturePool.Return(outputRT);

        return resultTex;
    }

    public static Texture2D ApplyMaskToAlpha_CPU(
        Texture2D sourceTex,
        Texture2D maskTex,
        BlendMode blendMode = BlendMode.Multiply,
        float threshold = 0.0f,
        bool invertMask = false,
        int maskChannel = 3
    )
    {
        // 确保纹理可读写
        Texture2D sourceReadable = EnsureTextureReadable(sourceTex);
        Texture2D maskReadable = EnsureTextureReadable(maskTex);

        // 使用32位颜色数组
        Color32[] sourcePixels = sourceReadable.GetPixels32();
        Color32[] maskPixels = maskReadable.GetPixels32();

        int sourceWidth = sourceReadable.width;
        int sourceHeight = sourceReadable.height;
        int maskWidth = maskReadable.width;
        int maskHeight = maskReadable.height;
        bool sameSize = (sourceWidth == maskWidth) && (sourceHeight == maskHeight);

        // 创建结果像素数组
        Color32[] resultPixels = new Color32[sourcePixels.Length];

        // 使用并行处理提高性能
        Parallel.For(0, sourceHeight, y =>
        {
            for (int x = 0; x < sourceWidth; x++)
            {
                int sourceIndex = y * sourceWidth + x;
                Color32 sourceColor = sourcePixels[sourceIndex];
                Color maskColor;

                if (sameSize)
                {
                    // 尺寸完全一致时，直接通过相同坐标访问，无需UV映射和采样
                    int maskIndex = y * maskWidth + x;
                    maskColor = maskPixels[maskIndex];
                }
                else
                {
                    // 尺寸不一致时，计算对应的mask UV坐标
                    float u = sourceWidth > 1 ? x / (float)(sourceWidth - 1) : 0.5f;
                    float v = sourceHeight > 1 ? y / (float)(sourceHeight - 1) : 0.5f;

                    // 获取mask颜色（使用双线性采样）
                    maskColor = SampleTextureBilinear(maskPixels, maskWidth, maskHeight, u, v);
                }

                // 根据指定通道获取mask值
                float maskValue = GetChannelValue(maskColor, maskChannel);

                // 应用阈值
                if (threshold > 0)
                {
                    maskValue = maskValue >= threshold ? 1.0f : 0.0f;
                }

                // 反转mask
                if (invertMask)
                {
                    maskValue = 1.0f - maskValue;
                }

                // 应用混合模式
                float sourceAlpha = sourceColor.a / 255f; // 转换为0-1范围
                float newAlphaFloat = ApplyBlendMode(sourceAlpha, maskValue, blendMode);

                // 转换为byte
                byte newAlpha = (byte)Mathf.Clamp(Mathf.RoundToInt(newAlphaFloat * 255), 0, 255);

                // 保持原RGB，只修改Alpha
                resultPixels[sourceIndex] = new Color32(
                    sourceColor.r,
                    sourceColor.g,
                    sourceColor.b,
                    newAlpha
                );
            }
        });

        Texture2D result = new(sourceWidth, sourceHeight, TextureFormat.ARGB32, false);
        result.SetPixels32(resultPixels);
        result.Apply();

        // 清理临时纹理
        CleanupTemporaryTextures(sourceReadable, sourceTex, maskReadable, maskTex);

        return result;
    }

    private static void CleanupTemporaryTextures(
        Texture2D temp1, Texture2D original1,
        Texture2D temp2, Texture2D original2
    )
    {
        if (temp1 != null && temp1 != original1)
        {
            UnityEngine.Object.Destroy(temp1);
        }

        if (temp2 != null && temp2 != original2)
        {
            UnityEngine.Object.Destroy(temp2);
        }
    }

    #endregion

    #region InvertAlpha

    public static Texture2D InvertAlpha(Texture2D texture)
    {
        if(texture == null)
            throw new ArgumentNullException("Texture cannot be null.");
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogWarning("Current platform does not support Compute Shaders. Falling back to CPU method.");
            return InvertAlpha_CPU(texture);
        }

        ComputeShader shader = ShaderInvertMaskAlpha;
        int kernelHandle = shader.FindKernel("CSMain");
        RenderTexture outputRT = RenderTexturePool.Get(texture.width, texture.height);
        shader.SetTexture(kernelHandle, "SourceTex", texture);
        shader.SetTexture(kernelHandle, "Result", outputRT);
        DispatchComputeShader(shader, kernelHandle, texture);
        Texture2D resultTex = RenderTextureToTexture2D(outputRT, texture);
        RenderTexturePool.Return(outputRT);
        return resultTex;
    }

    public static Texture2D InvertAlpha_CPU(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Source Texture is null!");
            return null;
        }

        int width = texture.width;
        int height = texture.height;

        // 创建目标纹理
        Texture2D resultTexture = new(width, height, TextureFormat.ARGB32, false);

        // 使用GetPixels32获取像素数据
        Color32[] pixels = texture.GetPixels32();

        // 使用Parallel.For进行并行处理（提高性能）
        Parallel.For(0, pixels.Length, i =>
        {
            Color32 original = pixels[i];
            // 反转alpha值，保持RGB不变
            pixels[i] = new Color32(original.r, original.g, original.b,
                                  (byte)(255 - original.a));
        });

        // 应用修改到纹理
        resultTexture.SetPixels32(pixels);
        resultTexture.Apply();
        return resultTexture;
    }

    #endregion

    #region Texture Resize

    public static Texture2D ResizeTexture2D(Texture2D texture, int newWidth, int newHeight)
    {
        if (texture == null)
            throw new ArgumentNullException("Texture cannot be null.");
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogWarning("Current platform does not support Compute Shaders. Falling back to CPU method.");
            return ResizeTexture2D_CPU(texture, newWidth, newHeight);
        }

        ComputeShader shader = ShaderTextureResize;
        int kernelHandle = shader.FindKernel("CSMain");
        RenderTexture outputRT = RenderTexturePool.Get(texture.width, texture.height);

        shader.SetTexture(kernelHandle, "Result", outputRT);
        shader.SetTexture(kernelHandle, "Source", texture);
        shader.SetInt("SourceWidth", texture.width);
        shader.SetInt("SourceHeight", texture.height);
        shader.SetInt("DestWidth", newWidth);
        shader.SetInt("DestHeight", newHeight);
        DispatchComputeShader(shader, kernelHandle, outputRT);
        Texture2D resultTex = RenderTextureToTexture2D(outputRT, outputRT);
        RenderTexturePool.Return(outputRT);
        return resultTex;
    }

    public static Texture2D ResizeTexture2D_CPU(Texture2D texture, int newWidth, int newHeight)
    {
        Color32[] srcPixels = texture.GetPixels32();
        Color32[] dstPixels = ResizePixelsBilinear32(srcPixels, texture.width, texture.height, newWidth, newHeight);
        Texture2D dst = new(newWidth, newHeight, texture.format, false);
        dst.SetPixels32(dstPixels);
        dst.Apply();

        return dst;
    }

    private static Color32[] ResizePixelsBilinear32(Color32[] srcPixels, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
    {
        Color32[] dstPixels = new Color32[dstWidth * dstHeight];

        for (int y = 0; y < dstHeight; y++)
        {
            for (int x = 0; x < dstWidth; x++)
            {
                float u = (x + 0.5f) / dstWidth;
                float v = (y + 0.5f) / dstHeight;
                Color sampledColor = SampleTextureBilinear(srcPixels, srcWidth, srcHeight, u, v);
                dstPixels[y * dstWidth + x] = sampledColor;
            }
        }

        return dstPixels;
    }

    #endregion

    #region PadBottomToMatchAspect

    public static Texture2D PadBottomToMatchAspect(Texture2D source, float targetAspect)
    {
        if (source == null) return null;

        int width = source.width;
        int height = source.height;
        float currentAspect = (float)width / height;

        if (currentAspect <= targetAspect)
            return source;

        int newHeight = Mathf.CeilToInt(width / targetAspect);

        Texture2D result = new(width, newHeight, source.format, false);

        Color32[] transparentPixels = new Color32[width * newHeight];
        for (int i = 0; i < transparentPixels.Length; i++)
            transparentPixels[i] = new Color32(0, 0, 0, 0);
        result.SetPixels32(transparentPixels);
        result.Apply();

        int dstY = newHeight - height;
        Graphics.CopyTexture(source, 0, 0, 0, 0, width, height,
                             result, 0, 0, 0, dstY);

        return result;
    }

    #endregion

    #region Crop Texture

    public static Texture2D GetCroppingTex(Texture2D texture, int startX, int startY, int width, int height)
    {
        int destWidth = width - startX;
        int destHeight = height - startY;

        var result = new Texture2D(destWidth, destHeight, texture.format, false);
        Graphics.CopyTexture(texture, 0, 0, startX, startY, destWidth, destHeight,
                             result, 0, 0, 0, 0);

        return result;
    }

    #endregion

}
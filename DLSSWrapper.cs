using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class DLSSWrapper
{
	[DllImport("DLSSImporter")]
	static extern bool IsDLSSLoaded();

	[DllImport("DLSSImporter")]
	static extern bool IsHDR();

	[DllImport("DLSSImporter")]
	static extern int InitDLSSExt();

	[DllImport("DLSSImporter")]
	public static extern void UnloadDLSS();

	[DllImport("DLSSImporter")]
	public static extern bool ObtainOptimalResolution(int TargetWidth, int TargetHeight, int PerfQualityValue, out int pRecommendedOptimalRenderWidth, out int pRecommendedOptimalRenderHeight, out int pDynamicMaximumRenderSizeHeight, out int pDynamicMinimumRenderSizeHeight, out int pDynamicMaximumRenderSizeWidth, out int pDynamicMinimumRenderSizeWidth, out float pSharpness);

	[DllImport("DLSSImporter")]
	static extern void SetCreateDLSSFeatureParameters(int width, int height, int targetWidth, int targetHeight, int PerfQualityValue);

	[DllImport("DLSSImporter")]
	static extern IntPtr GetDLSSEvaluateFuncExtInt();

	[DllImport("DLSSImporter")]
	static extern int PrepareHandle();

	[DllImport("DLSSImporter")]
	static extern void SetDLSSEvaluateParametersExtInt(IntPtr resourceIn, IntPtr resourceOut, IntPtr depth, IntPtr motionVectors, int reset, int viewportWidth, int viewportHeight, float sharpness, float mvScaleX, float mvScaleY, float jitterX, float jitterY, int dlssHandleIdx);

	[DllImport("DLSSImporter")]
	static extern int GetDLSSEvaluateLastResult();

	[DllImport("DLSSImporter")]
	public static extern void ReleaseAllHandles();

	[DllImport("DLSSImporter")]
	static extern void ReleaseHandleInt(int dlssHandleIdx, out int releaseFeatureResult);

	DLSSWrapper()
	{
		this.Sharpness = 0.33f;
		this.Quality = 2;
		this.JitterOffsets = Vector2.zero;
		this.MVScale = Vector2.one;
		this._srcCopyUAVPtr = IntPtr.Zero;
		this._motionVectorsCopyPtr = IntPtr.Zero;
		this._depthCopyPtr = IntPtr.Zero;
		this._textureBufferPtr = IntPtr.Zero;
		this._mrt3 = new RenderTargetIdentifier[3];
		this._mrt2 = new RenderTargetIdentifier[2];
		this._dlssHandle = -1;
		this._dlssHandlesToRelease = new List<int>();
		this._featureInWidth = -1;
		this._featureInHeight = -1;
		this._featureOutWidth = -1;
		this._featureOutHeight = -1;
		this._featureQuality = -1;
		//base..ctor();
		throw new InvalidOperationException("Use DLSSWrapper(Material) ctor. Not DLSSWrapper()");
	}

	public bool IsDLSSLibraryLoaded()
	{
		return this.DebugDisable || DLSSWrapper.WantToDebugDLSSViaRenderdoc || DLSSWrapper.IsDLSSLoaded();
	}

	public static bool IsDLSSSupported()
	{
		bool result;
		try
		{
			if (DLSSWrapper._isSupportedCachedValue != null)
			{
				result = DLSSWrapper._isSupportedCachedValue.Value;
			}
			else if (DLSSWrapper.DebugPretendDLSSIsSupported || DLSSWrapper.WantToDebugDLSSViaRenderdoc)
			{
				DLSSWrapper._isSupportedCachedValue = new bool?(true);
				result = true;
			}
			else if (DLSSWrapper.IsDLSSLoaded())
			{
				DLSSWrapper._isSupportedCachedValue = new bool?(true);
				result = true;
			}
			else
			{
				bool flag = DLSSWrapper.InitDLSSExt() == 0;
				DLSSWrapper.UnloadDLSS();
				DLSSWrapper._isSupportedCachedValue = new bool?(flag);
				result = flag;
			}
		}
		finally
		{
		}
		return result;
	}

	public DLSSWrapper.InitErrors InitializeDLSS()
	{
		if (this.DebugDisable || DLSSWrapper.WantToDebugDLSSViaRenderdoc)
		{
			return DLSSWrapper.InitErrors.INIT_SUCCESS;
		}
		return (DLSSWrapper.InitErrors)DLSSWrapper.InitDLSSExt();
	}

	public DLSSWrapper(Material CopyDLSSSourcesMat, Material DebugMaterial)
	{
		this.Sharpness = 0.33f;
		this.Quality = 2;
		this.JitterOffsets = Vector2.zero;
		this.MVScale = Vector2.one;
		this._srcCopyUAVPtr = IntPtr.Zero;
		this._motionVectorsCopyPtr = IntPtr.Zero;
		this._depthCopyPtr = IntPtr.Zero;
		this._textureBufferPtr = IntPtr.Zero;
		this._mrt3 = new RenderTargetIdentifier[3];
		this._mrt2 = new RenderTargetIdentifier[2];
		this._dlssHandle = -1;
		this._dlssHandlesToRelease = new List<int>();
		this._featureInWidth = -1;
		this._featureInHeight = -1;
		this._featureOutWidth = -1;
		this._featureOutHeight = -1;
		this._featureQuality = -1;
		//base..ctor();
		this._matCopySources = CopyDLSSSourcesMat;
		this._debugMaterial = DebugMaterial;
		this._mesh = new Mesh();
		Vector3[] vertices = new Vector3[]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f)
		};
		this._mesh.vertices = vertices;
		Vector2[] uv = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		this._mesh.uv = uv;
		int[] triangles = new int[]
		{
			0,
			1,
			2,
			2,
			3,
			0
		};
		this._mesh.triangles = triangles;
	}

	public void OnDestroy()
	{
		if (this._dlssHandle != -1)
		{
			int num;
			DLSSWrapper.ReleaseHandleInt(this._dlssHandle, out num);
		}
		foreach (int dlssHandleIdx in this._dlssHandlesToRelease)
		{
			int num2;
			DLSSWrapper.ReleaseHandleInt(dlssHandleIdx, out num2);
		}
		this._dlssHandlesToRelease.Clear();
	}

	public void OnRenderImage(RenderTexture src, RenderTexture dest, bool flipOutputUpDown, Vector2 jitterOffset)
	{
		this.InitializeResourcesIfNeeded(src, dest);
		if (this._cmdBufEvaluate == null)
		{
			this._cmdBufEvaluate = new CommandBuffer();
			this._cmdBufEvaluate.name = "DLSSEvaluate";
		}
		this._cmdBufEvaluate.Clear();
		if (this._dlssHandlesToRelease.Count > 0)
		{
			foreach (int dlssHandleIdx in this._dlssHandlesToRelease)
			{
				int num;
				DLSSWrapper.ReleaseHandleInt(dlssHandleIdx, out num);
			}
			this._dlssHandlesToRelease.Clear();
		}
		if (!this.DebugDisable && !DLSSWrapper.WantToDebugDLSSViaRenderdoc)
		{
			if (this._dlssHandle == -1)
			{
				DLSSWrapper.SetCreateDLSSFeatureParameters(this._featureInWidth, this._featureInHeight, this._featureOutWidth, this._featureOutHeight, this._featureQuality);
				this._dlssHandle = DLSSWrapper.PrepareHandle();
			}
			if (src.width != this._featureInWidth || src.height != this._featureInHeight || this._motionVectorsCopy.width != this._featureInWidth || this._motionVectorsCopy.height != this._featureInHeight || this._srcCopyUAV.width != this._featureInWidth || this._srcCopyUAV.height != this._featureInHeight)
			{
				Debug.LogError("Wrong DLSS SIZE!");
			}
			DLSSWrapper.SetDLSSEvaluateParametersExtInt(this._srcCopyUAVPtr, this._textureBufferPtr, this._depthCopyPtr, this._motionVectorsCopyPtr, 0, src.width, src.height, this.Sharpness, (float)(-(float)this._motionVectorsCopy.width), (float)this._motionVectorsCopy.height, jitterOffset.x, jitterOffset.y, this._dlssHandle);
		}
		if (!this.DebugDisable && !DLSSWrapper.WantToDebugDLSSViaRenderdoc)
		{
			this._cmdBufEvaluate.IssuePluginEvent(DLSSWrapper.GetDLSSEvaluateFuncExtInt(), this._dlssHandle);
		}
		bool flag = false;
		if (DLSSWrapper.IsDLSSLoaded())
		{
			flag = true;
		}
		if (flag)
		{
			this._cmdBufEvaluate.EnableShaderKeyword("UPDOWN");
		}
		else
		{
			this._cmdBufEvaluate.DisableShaderKeyword("UPDOWN");
		}
		int num2 = (dest != null) ? dest.width : Screen.width;
		int num3 = (dest != null) ? dest.height : Screen.height;
		this._cmdBufEvaluate.SetRenderTarget(dest);
		this._cmdBufEvaluate.SetViewport(new Rect(0f, 0f, (float)num2, (float)num3));
		if (!this.DebugMode)
		{
			if (this._propertiesOut == null)
			{
				this._propertiesOut = new MaterialPropertyBlock();
			}
			if (!this.DebugDisable && !DLSSWrapper.WantToDebugDLSSViaRenderdoc)
			{
				this._propertiesOut.SetTexture("_MainTex", this._textureBuffer);
			}
			else
			{
				this._propertiesOut.SetTexture("_MainTex", src);
			}
			if (flipOutputUpDown)
			{
				flag = !flag;
			}
			if (flag)
			{
				this._cmdBufEvaluate.EnableShaderKeyword("UPDOWN");
			}
			else
			{
				this._cmdBufEvaluate.DisableShaderKeyword("UPDOWN");
			}
			this._cmdBufEvaluate.DrawMesh(this._mesh, Matrix4x4.identity, this._matCopySources, 0, 1, this._propertiesOut);
		}
		else
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetTexture("_MainTex", this._textureBuffer);
			materialPropertyBlock.SetTexture("_DepthTex", this._depthCopy);
			materialPropertyBlock.SetTexture("_MotionVectorsTex", this._motionVectorsCopy);
			materialPropertyBlock.SetTexture("_SrcColorTex", this._srcCopyUAV);
			this._cmdBufEvaluate.DrawMesh(this._mesh, Matrix4x4.identity, this._debugMaterial, 0, 0, materialPropertyBlock);
		}
		Graphics.ExecuteCommandBuffer(this._cmdBufEvaluate);
	}

	void EnableCopyDepthKeyword(CommandBuffer cmd, DLSSWrapper.DEPTH_COPY_MODE mode)
	{
		switch (mode)
		{
		case DLSSWrapper.DEPTH_COPY_MODE.INVERT:
			cmd.EnableShaderKeyword("DLSS_INVERSEDEPTH");
			return;
		case DLSSWrapper.DEPTH_COPY_MODE.CONST_ZERO:
			cmd.EnableShaderKeyword("DLSS_DEPTHZERO");
			return;
		case DLSSWrapper.DEPTH_COPY_MODE.CONST_ONE:
			cmd.EnableShaderKeyword("DLSS_DEPTHONE");
			return;
		default:
			return;
		}
	}

	void DisableCopyDepthKeyword(CommandBuffer cmd, DLSSWrapper.DEPTH_COPY_MODE mode)
	{
		switch (mode)
		{
		case DLSSWrapper.DEPTH_COPY_MODE.INVERT:
			cmd.DisableShaderKeyword("DLSS_INVERSEDEPTH");
			return;
		case DLSSWrapper.DEPTH_COPY_MODE.CONST_ZERO:
			cmd.DisableShaderKeyword("DLSS_DEPTHZERO");
			return;
		case DLSSWrapper.DEPTH_COPY_MODE.CONST_ONE:
			cmd.DisableShaderKeyword("DLSS_DEPTHONE");
			return;
		default:
			return;
		}
	}

	public void CopyDepthMotion(RenderTexture source, RenderTexture dst, DLSSWrapper.DEPTH_COPY_MODE depthCopyMode)
	{
		this.InitializeResourcesIfNeeded(source, dst);
		if (this._motionVectorsCopy == null || this._depthCopy == null || this._srcCopyUAV == null)
		{
			return;
		}
		if (this._cmdBufCopy == null)
		{
			this._cmdBufCopy = new CommandBuffer();
			this._cmdBufCopy.name = "DLSSCopy";
		}
		this._cmdBufCopy.Clear();
		this._cmdBufCopy.SetGlobalVector("DLSSMVScale", this.MVScale);
		this.EnableCopyDepthKeyword(this._cmdBufCopy, depthCopyMode);
		this._mrt3[0] = this._srcCopyUAV.colorBuffer;
		this._mrt3[1] = this._motionVectorsCopy.colorBuffer;
		this._mrt3[2] = this._depthCopy.colorBuffer;
		this._cmdBufCopy.DisableShaderKeyword("UPDOWN");
		this._cmdBufCopy.SetRenderTarget(this._mrt3, this._srcCopyUAV.depthBuffer);
		if (this._copyDepthMotionProperties == null)
		{
			this._copyDepthMotionProperties = new MaterialPropertyBlock();
		}
		this._copyDepthMotionProperties.SetTexture("_MainTex", source);
		this._cmdBufCopy.DrawMesh(this._mesh, Matrix4x4.identity, this._matCopySources, 0, 0, this._copyDepthMotionProperties);
		this.DisableCopyDepthKeyword(this._cmdBufCopy, depthCopyMode);
		Graphics.ExecuteCommandBuffer(this._cmdBufCopy);
	}

	void InitializeResourcesIfNeeded(RenderTexture src, RenderTexture dest)
	{
		int num = (dest != null) ? dest.width : Screen.width;
		int num2 = (dest != null) ? dest.height : Screen.height;
		bool flag = this._textureBuffer != null && (this._textureBuffer.width != num || this._textureBuffer.height != num2);
		bool flag2 = this._motionVectorsCopy != null && (this._motionVectorsCopy.width != src.width || this._motionVectorsCopy.height != src.height);
		if (flag || flag2 || (this._dlssHandle == -1 && !this.DebugDisable && !DLSSWrapper.WantToDebugDLSSViaRenderdoc))
		{
			if (this._motionVectorsCopy != null)
			{
				this._motionVectorsCopy.Release();
                UnityEngine.Object.DestroyImmediate(this._motionVectorsCopy);
				this._motionVectorsCopy = null;
			}
			if (this._textureBuffer != null)
			{
				this._textureBuffer.Release();
				UnityEngine.Object.DestroyImmediate(this._textureBuffer);
				this._textureBuffer = null;
			}
			this._srcCopyUAVPtr = IntPtr.Zero;
			if (this._srcCopyUAV != null)
			{
				this._srcCopyUAV.Release();
				UnityEngine.Object.DestroyImmediate(this._srcCopyUAV);
				this._srcCopyUAV = null;
			}
			this._depthCopyPtr = IntPtr.Zero;
			if (this._depthCopy != null)
			{
				this._depthCopy.Release();
				UnityEngine.Object.DestroyImmediate(this._depthCopy);
				this._depthCopy = null;
			}
			if (this._dlssHandle != -1)
			{
				this._dlssHandlesToRelease.Add(this._dlssHandle);
			}
			this._dlssHandle = -1;
			this._createdDLSSBuffers = false;
			this._featureQuality = this.Quality;
		}
		if (this._dlssHandle == -1)
		{
			this._featureInWidth = src.width;
			this._featureInHeight = src.height;
			this._featureOutWidth = num;
			this._featureOutHeight = num2;
		}
		if (!this._createdDLSSBuffers)
		{
			this._motionVectorsCopy = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R16G16_SFloat);
			this._motionVectorsCopy.enableRandomWrite = true;
			this._motionVectorsCopy.name = "DLSSMotionIn";
			this._motionVectorsCopy.Create();
			this._motionVectorsCopyPtr = this._motionVectorsCopy.GetNativeDepthBufferPtr();
			RenderTextureFormat format = (dest != null) ? dest.format : RenderTextureFormat.RGB111110Float;
			if (DLSSWrapper.IsHDR())
			{
				format = RenderTextureFormat.ARGBFloat;
			}
			this._textureBuffer = new RenderTexture(num, num2, 0, format);
			if (this._textureBuffer.IsCreated())
			{
				this._textureBuffer.Release();
			}
			this._textureBuffer.enableRandomWrite = true;
			this._textureBuffer.name = "DLSSOut";
			this._textureBuffer.Create();
			this._textureBufferPtr = this._textureBuffer.GetNativeDepthBufferPtr();
			this._srcCopyUAV = new RenderTexture(src.width, src.height, 0, DLSSWrapper.IsHDR() ? RenderTextureFormat.ARGBFloat : src.format, 1);
			if (this._srcCopyUAV.IsCreated())
			{
				this._srcCopyUAV.Release();
			}
			this._srcCopyUAV.enableRandomWrite = true;
			this._srcCopyUAV.name = "DLSSColorIn";
			this._srcCopyUAV.Create();
			this._srcCopyUAVPtr = this._srcCopyUAV.GetNativeDepthBufferPtr();
			this._depthCopy = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.RHalf);
			this._depthCopy.depth = 0;
			this._depthCopy.name = "DLSSDepthIn";
			this._depthCopy.Create();
			this._depthCopyPtr = this._depthCopy.GetNativeDepthBufferPtr();
			this._createdDLSSBuffers = true;
			Action renderTexturesAreChanged = this.RenderTexturesAreChanged;
			if (renderTexturesAreChanged == null)
			{
				return;
			}
			renderTexturesAreChanged();
		}
	}

	// Note: this type is marked as 'beforefieldinit'.
	static DLSSWrapper()
	{
	}

	public RenderTexture MotionVectorsBuffer
	{
		get
		{
			return this._motionVectorsCopy;
		}
	}

	public RenderTexture DepthBuffer
	{
		get
		{
			return this._depthCopy;
		}
	}

	public event Action RenderTexturesAreChanged;

	bool _createdDLSSBuffers;

	public float Sharpness;

	public bool DebugMode;

	public bool DebugDisable;

	public static bool DebugPretendDLSSIsSupported;

	public static bool WantToDebugDLSSViaRenderdoc;

	public int Quality;

	public Vector2 JitterOffsets;

	public Vector2 MVScale;

	RenderTexture _srcCopyUAV;

	RenderTexture _motionVectorsCopy;

	RenderTexture _depthCopy;

	RenderTexture _textureBuffer;

	IntPtr _srcCopyUAVPtr;

	IntPtr _motionVectorsCopyPtr;

	IntPtr _depthCopyPtr;

	IntPtr _textureBufferPtr;

	MaterialPropertyBlock _propertiesOut;

	MaterialPropertyBlock _copyDepthMotionProperties;

	CommandBuffer _cmdBufEvaluate;

	CommandBuffer _cmdBufCopy;

	RenderTargetIdentifier[] _mrt3;

	RenderTargetIdentifier[] _mrt2;

	Material _matCopySources;

	Mesh _mesh;

	int _dlssHandle;

	List<int> _dlssHandlesToRelease;

	int _featureInWidth;

	int _featureInHeight;

	int _featureOutWidth;

	int _featureOutHeight;

	int _featureQuality;

	Material _debugMaterial;

	static bool? _isSupportedCachedValue;

	public enum InitErrors
	{
		INIT_SUCCESS,
		INIT_INTERNAL_INIT_ERROR,
		INIT_GET_CAPABILITY_FAILED,
		INIT_OLD_DRIVER,
		INIT_NOT_SUPPORTED_HARDWARE_OR_PLATFORM,
		INIT_DENIED_FOR_THIS_APP
	}

	public enum DEPTH_COPY_MODE
	{
		NO_MODIFICATION,
		INVERT,
		CONST_ZERO,
		CONST_ONE
	}
}

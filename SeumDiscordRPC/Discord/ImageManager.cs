using System;
using System.Runtime.InteropServices;

namespace Discord;

public class ImageManager
{
	internal struct FFIEvents
	{
	}

	internal struct FFIMethods
	{
		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate void FetchCallback(IntPtr ptr, Result result, ImageHandle handleResult);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate void FetchMethod(IntPtr methodsPtr, ImageHandle handle, bool refresh, IntPtr callbackData, FetchCallback callback);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate Result GetDimensionsMethod(IntPtr methodsPtr, ImageHandle handle, ref ImageDimensions dimensions);

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		internal delegate Result GetDataMethod(IntPtr methodsPtr, ImageHandle handle, byte[] data, int dataLen);

		internal FetchMethod Fetch;

		internal GetDimensionsMethod GetDimensions;

		internal GetDataMethod GetData;
	}

	public delegate void FetchHandler(Result result, ImageHandle handleResult);

	private IntPtr MethodsPtr;

	private object MethodsStructure;

	private FFIMethods Methods
	{
		get
		{
			if (MethodsStructure == null)
			{
				MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
			}
			return (FFIMethods)MethodsStructure;
		}
	}

	internal ImageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
	{
		if (eventsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
		InitEvents(eventsPtr, ref events);
		MethodsPtr = ptr;
		if (MethodsPtr == IntPtr.Zero)
		{
			throw new ResultException(Result.InternalError);
		}
	}

	private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
	{
		Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
	}

	[MonoPInvokeCallback]
	private static void FetchCallbackImpl(IntPtr ptr, Result result, ImageHandle handleResult)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(ptr);
		FetchHandler obj = (FetchHandler)gCHandle.Target;
		gCHandle.Free();
		obj(result, handleResult);
	}

	public void Fetch(ImageHandle handle, bool refresh, FetchHandler callback)
	{
		GCHandle value = GCHandle.Alloc(callback);
		Methods.Fetch(MethodsPtr, handle, refresh, GCHandle.ToIntPtr(value), FetchCallbackImpl);
	}

	public ImageDimensions GetDimensions(ImageHandle handle)
	{
		ImageDimensions dimensions = default(ImageDimensions);
		Result result = Methods.GetDimensions(MethodsPtr, handle, ref dimensions);
		if (result != Result.Ok)
		{
			throw new ResultException(result);
		}
		return dimensions;
	}

	public void GetData(ImageHandle handle, byte[] data)
	{
		Result result = Methods.GetData(MethodsPtr, handle, data, data.Length);
		if (result != Result.Ok)
		{
			throw new ResultException(result);
		}
	}
}

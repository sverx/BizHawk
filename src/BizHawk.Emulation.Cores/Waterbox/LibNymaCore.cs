using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BizHawk.BizInvoke;
using BizHawk.Common;

namespace BizHawk.Emulation.Cores.Waterbox
{
	public abstract unsafe class LibNymaCore : LibWaterboxCore
	{
		[StructLayout(LayoutKind.Sequential)]
		public class InitData
		{
			/// <summary>
			/// Filename without extension.  Used in autodetect
			/// </summary>
			public string FileNameBase;
			/// <summary>
			/// Just the extension.  Used in autodetect.  LOWERCASE PLEASE.
			/// </summary>
			public string FileNameExt;
			/// <summary>
			/// Full filename.  This will be fopen()ed by the core
			/// </summary>
			public string FileNameFull;
		}

		/// <summary>
		/// Do this before calling anything, even settings queries
		/// </summary>
		[BizImport(CC, Compatibility = true)]
		public abstract void PreInit();

		/// <summary>
		/// Load a ROM
		/// </summary>
		[BizImport(CC, Compatibility = true)]
		public abstract bool InitRom([In]InitData data);

		/// <summary>
		/// Load some CDs
		/// </summary>
		[BizImport(CC)]
		public abstract bool InitCd(int numdisks);

		public enum CommandType : short
		{
			NONE = 0x00,
			RESET = 0x01,
			POWER = 0x02,

			INSERT_COIN = 0x07,

			TOGGLE_DIP0 = 0x10,
			TOGGLE_DIP1,
			TOGGLE_DIP2,
			TOGGLE_DIP3,
			TOGGLE_DIP4,
			TOGGLE_DIP5,
			TOGGLE_DIP6,
			TOGGLE_DIP7,
			TOGGLE_DIP8,
			TOGGLE_DIP9,
			TOGGLE_DIP10,
			TOGGLE_DIP11,
			TOGGLE_DIP12,
			TOGGLE_DIP13,
			TOGGLE_DIP14,
			TOGGLE_DIP15,
		}

		[StructLayout(LayoutKind.Sequential)]
		public new class FrameInfo : LibWaterboxCore.FrameInfo
		{
			/// <summary>
			/// true to skip video rendering
			/// </summary>
			public short SkipRendering;
			/// <summary>
			/// true to skip audion rendering
			/// </summary>
			public short SkipSoundening;
			/// <summary>
			/// a single command to run at the start of this frame
			/// </summary>
			public CommandType Command;
			/// <summary>
			/// True to render to a single framebuffer size (LCM * LCM)
			/// </summary>
			public short RenderConstantSize;
			/// <summary>
			/// raw data for each input port, assumed to be MAX_PORTS * MAX_PORT_DATA long
			/// </summary>
			public byte* InputPortData;
			/// <summary>
			/// If the core calls time functions, this is the value that will be used
			/// </summary>
			public long FrontendTime;
		}

		/// <summary>
		/// Gets raw layer data to be handled by NymaCore.GetLayerData
		/// </summary>
		[BizImport(CC)]
		public abstract byte* GetLayerData();

		/// <summary>
		/// Set enabled layers (or is it disabled layers?).  Only call if NymaCore.GetLayerData() returned non null
		/// </summary>
		/// <param name="layers">bitmask in order defined by NymaCore.GetLayerData</param>
		[BizImport(CC)]
		public abstract void SetLayers(ulong layers);

		[BizImport(CC)]
		public abstract void DumpInputs();

		/// <summary>
		/// Set what input devices we're going to use
		/// </summary>
		/// <param name="devices">MUST end with a null string</param>
		[BizImport(CC, Compatibility = true)]
		public abstract void SetInputDevices(string[] devices);

		public enum VideoSystem : int
		{
			NONE,
			PAL,
			PAL_M,
			NTSC,
			SECAM
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SystemInfo
		{
			public int MaxWidth;
			public int MaxHeight;
			public int NominalWidth;
			public int NominalHeight;
			public VideoSystem VideoSystem;
			public int FpsFixed;
			public int LcmWidth;
			public int LcmHeight;
		}

		[BizImport(CC, Compatibility = true)]
		public abstract SystemInfo* GetSystemInfo();

		[BizImport(CC)]
		public abstract void DumpSettings();

		public delegate void FrontendSettingQuery(string setting, IntPtr dest);
		[BizImport(CC)]
		public abstract void SetFrontendSettingQuery(FrontendSettingQuery q);

		[StructLayout(LayoutKind.Sequential)]
		public class TOC
		{
			public int FirstTrack;
			public int LastTrack;
			public int DiskType;

			[StructLayout(LayoutKind.Sequential)]
			public struct Track
			{
				public int Adr;
				public int Control;
				public int Lba;
				public int Valid;
			}

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 101)]
			public Track[] Tracks;
		}
		[UnmanagedFunctionPointer(CC)]
		public delegate void CDTOCCallback(int disk, [In, Out]TOC toc);
		[UnmanagedFunctionPointer(CC)]
		public delegate void CDSectorCallback(int disk, int lba, IntPtr dest);
		[BizImport(CC)]
		public abstract void SetCDCallbacks(CDTOCCallback toccallback, CDSectorCallback sectorcallback);
		[BizImport(CC)]
		public abstract IntPtr GetFrameThreadProc();
	}
}

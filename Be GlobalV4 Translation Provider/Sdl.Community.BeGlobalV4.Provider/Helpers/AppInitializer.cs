﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Sdl.Community.BeGlobalV4.Provider.Model;
using Sdl.Community.BeGlobalV4.Provider.ViewModel;
using Sdl.Community.Toolkit.LanguagePlatform.ExcelParser;
using Sdl.Community.Toolkit.LanguagePlatform.Models;
using Sdl.Desktop.IntegrationApi;
using Sdl.Desktop.IntegrationApi.Extensions;
using Sdl.TranslationStudioAutomation.IntegrationApi;

namespace Sdl.Community.BeGlobalV4.Provider.Helpers
{
	[ApplicationInitializer]
	public sealed class AppInitializer : IApplicationInitializer
	{
		private static Constants _constants = new Constants();

		public void Execute()
		{
			if (Application.Current == null)
			{
				new Application();
			}

			if (Application.Current != null)
			{
				Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
			}
		}
		
		public static List<ExcelSheet> WriteMTCodesLocally()
		{
			var mtCloudFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _constants.SDLCommunity, _constants.SDLMachineTranslationCloud);
			var excelFilePath = Path.Combine(mtCloudFolderPath, "MTLanguageCodes.xlsx");
			var excelParser = new ExcelParser();

			if (!Directory.Exists(mtCloudFolderPath))
			{
				Directory.CreateDirectory(mtCloudFolderPath);
			}
			WriteExcelLocally(excelFilePath, mtCloudFolderPath);
			return excelParser.ReadExcel(excelFilePath, 0);
		}

		public static ProjectsController GetProjectController()
		{
			return SdlTradosStudio.Application?.GetController<ProjectsController>();
		}

		public static FilesController GetFileController()
		{
			return SdlTradosStudio.Application?.GetController<FilesController>();
		}

		public static EditorController GetEditorController()
		{
			return SdlTradosStudio.Application?.GetController<EditorController>();
		}

		public static List<MTCodeModel> GetMTCodes()
		{
			return new MTCodesViewModel(WriteMTCodesLocally())?.MTCodes?.ToList();
		}

		private static void WriteExcelLocally(string excelFilePath, string mtCloudFolderPath)
		{
			try
			{
				var resource = PluginResources.MTLanguageCodes;

				if (!File.Exists(excelFilePath))
				{
					File.WriteAllBytes(Path.Combine(mtCloudFolderPath, "MTLanguageCodes.xlsx"), resource);
				}
			}
			catch (Exception ex)
			{
				Log.Logger.Error($"{_constants.WriteExcelLocally} {ex.Message}\n {ex.StackTrace}");
				throw;
			}
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdl.Community.PostEdit.Compare.Core.Reports;
using Sdl.Community.PostEdit.Compare.DAL.ExcelTableModel;
using Sdl.Community.PostEdit.Compare.DAL.PostEditModificationsAnalysis;
using static Sdl.Community.PostEdit.Compare.Core.Reports.Report;

namespace Sdl.Community.PostEdit.Compare.Core.Helper
{
	public static class ExcelReportHelper
	{
		public static List<PEMModel> CreateExcelDataModels(PEMpAnalysisData analysisData)
		{
			var analyseModelsList = new List<PEMModel>();

			var exactMatchValues = GetValuesForExactMatch(analysisData);
			var fuzzy99Values = GetValuesForFuzzy99(analysisData);
			var fuzzy94Values = GetValuesForFuzzy94(analysisData);
			var fuzzy84Values = GetValuesForFuzzy84(analysisData);
			var fuzzy74Values = GetValuesForFuzzy74(analysisData);
			var newValues = GetNewValues(analysisData);
			var totalValues = GetTotalValues(analysisData);

			analyseModelsList.AddRange(exactMatchValues);
			analyseModelsList.AddRange(fuzzy99Values);
			analyseModelsList.AddRange(fuzzy94Values);
			analyseModelsList.AddRange(fuzzy84Values);
			analyseModelsList.AddRange(fuzzy74Values);
			analyseModelsList.AddRange(newValues);
			analyseModelsList.AddRange(totalValues);
			return analyseModelsList;
		}

		private static List<PEMModel> GetTotalValues(PEMpAnalysisData analysisData)
		{
			var totalValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Total,Constants.Segments,analysisData.totalSegments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Total,Constants.Characters,analysisData.totalCharacters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Total,Constants.Words,analysisData.totalWords)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Total,Constants.Percent,analysisData.totalPercent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return totalValuesList;
		}

	

		private static List<PEMModel> GetNewValues(PEMpAnalysisData analysisData)
		{
			var newValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.New,Constants.Segments,analysisData.newSegments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.New,Constants.Characters,analysisData.newCharacters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.New,Constants.Words,analysisData.newWords)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.New,Constants.Percent,analysisData.newPercent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return newValuesList;
		}

		private static List<PEMModel> GetValuesForFuzzy74(PEMpAnalysisData analysisData)
		{
			var fuzzy74ValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy74,Constants.Segments,analysisData.fuzzy74Segments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy74,Constants.Characters,analysisData.fuzzy74Characters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy74,Constants.Words,analysisData.fuzzy74Words)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy74,Constants.Percent,analysisData.fuzzy74Percent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return fuzzy74ValuesList;
		}

		private static List<PEMModel> GetValuesForFuzzy84(PEMpAnalysisData analysisData)
		{
			var fuzzy84ValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy84,Constants.Segments,analysisData.fuzzy84Segments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy84,Constants.Characters,analysisData.fuzzy84Characters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy84,Constants.Words,analysisData.fuzzy84Words)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy84,Constants.Percent,analysisData.fuzzy84Percent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return fuzzy84ValuesList;
		}

		private static List<PEMModel> GetValuesForFuzzy94(PEMpAnalysisData analysisData)
		{
			var fuzzy94ValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy94,Constants.Segments,analysisData.fuzzy94Segments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy94,Constants.Characters,analysisData.fuzzy94Characters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy94,Constants.Words,analysisData.fuzzy94Words)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy94,Constants.Percent,analysisData.fuzzy94Percent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return fuzzy94ValuesList;
		}
		private static List<PEMModel> GetValuesForFuzzy99(PEMpAnalysisData analysisData)
		{
			var fuzzy99ValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy99,Constants.Segments,analysisData.fuzzy99Segments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy99,Constants.Characters,analysisData.fuzzy99Characters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy99,Constants.Words,analysisData.fuzzy99Words)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.Fuzzy99,Constants.Percent,analysisData.fuzzy99Percent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return fuzzy99ValuesList;
		}
		private static List<PEMModel> GetValuesForExactMatch(PEMpAnalysisData analysisData)
		{
			var exactMatchValuesList = new List<PEMModel>
			{
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Segments,analysisData.exactSegments)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Characters,analysisData.exactCharacters)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Words,analysisData.exactWords)
				},
				new PEMModel
				{
					AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Percent,analysisData.exactPercent)
				}
				//new PEMModel
				//{
				//	AnalyseResult = Tuple.Create(Constants.ExactMatch,Constants.Total,analysisData.tot)
				//},
			};
			return exactMatchValuesList;
		}
		//public static List<PEMModel> CreateExcelDataModels(PEMpAnalysisData analysisData)
		//{
		//	var model = new List<PEMModel>
		//	{
		//		new PEMModel
		//	{
		//		AnalyseResult = Tuple.Create("segments", analysisData.exactSegments)
		//		}
		//	};

		//	}

		//var pemDataModel = new PEMModel
		//{
		//	Hundred = new PEMResults
		//	{
		//		PropertyName ="100%",
		//		SegmentsNo = analysisData.exactSegments,
		//		CharactersNo = analysisData.exactCharacters,
		//		WordsNo = analysisData.exactWords,
		//		Percent = analysisData.exactPercent,
		//		Tags = analysisData.exactTags

		//	},
		//	Fuzzy99 = new PEMResults
		//	{
		//		PropertyName = "95% - 99%",
		//		SegmentsNo = analysisData.fuzzy99Segments,
		//		CharactersNo = analysisData.fuzzy99Characters,
		//		WordsNo = analysisData.fuzzy99Words,
		//		Percent = analysisData.fuzzy99Percent,
		//		Tags = analysisData.fuzzy99Tags

		//	},
		//	Fuzzy94 = new PEMResults
		//	{
		//		SegmentsNo = analysisData.fuzzy94Segments,
		//		CharactersNo = analysisData.fuzzy94Characters,
		//		WordsNo = analysisData.fuzzy94Words,
		//		Percent = analysisData.fuzzy94Percent,
		//		Tags = analysisData.fuzzy94Tags

		//	},
		//	Fuzzy84= new PEMResults
		//	{
		//		SegmentsNo = analysisData.fuzzy84Segments,
		//		CharactersNo = analysisData.fuzzy84Characters,
		//		WordsNo = analysisData.fuzzy84Words,
		//		Percent = analysisData.fuzzy84Percent,
		//		Tags = analysisData.fuzzy84Tags

		//	},
		//	Fuzzy74 = new PEMResults
		//	{
		//		SegmentsNo = analysisData.fuzzy74Segments,
		//		CharactersNo = analysisData.fuzzy74Characters,
		//		WordsNo = analysisData.fuzzy74Words,
		//		Percent = analysisData.fuzzy74Percent,
		//		Tags = analysisData.fuzzy74Tags

		//	},
		//	New = new PEMResults
		//	{
		//		SegmentsNo = analysisData.newSegments,
		//		CharactersNo = analysisData.newCharacters,
		//		WordsNo = analysisData.newWords,
		//		Percent = analysisData.newPercent,
		//		Tags = analysisData.newTags

		//	},
		//	Total = new PEMResults
		//	{
		//		SegmentsNo = analysisData.totalSegments,
		//		CharactersNo = analysisData.totalCharacters,
		//		WordsNo = analysisData.totalWords,
		//		Percent = analysisData.totalPercent,
		//		Tags = analysisData.totalTags

		//	},
		//};
		//return pemDataModel;
		//}
	}
}


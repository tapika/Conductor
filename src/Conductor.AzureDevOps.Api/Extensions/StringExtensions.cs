using System;
using System.Text.RegularExpressions;

namespace Conductor.AzureDevOps.Api.Extensions
{
	public static class StringExtensions
	{
		public const string gitRefsHeads = "refs/heads/";

		public static string MakeRefSpec(this string s) => s.StartsWith(gitRefsHeads, StringComparison.Ordinal) ? s : $"{gitRefsHeads}{s}";

		public static string GetBranchNameOnly(this string s)
		{
			if (s.StartsWith(gitRefsHeads))
			{
				return s.Substring(gitRefsHeads.Length);
			}

			return s;
		}

	}
}

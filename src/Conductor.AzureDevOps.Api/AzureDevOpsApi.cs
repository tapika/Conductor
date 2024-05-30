using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Conductor.AzureDevOps.Api.Extensions;
using Conductor.AzureDevOps.Api.Models;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Conductor.AzureDevOps.Api
{
	public class AzureDevOpsApi
	{
		private static readonly ISerializer s_serializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			NullValueHandling = NullValueHandling.Ignore
		});


		/// <summary>
		/// Invokes function and catches any server exception. Server exception is re-thrown with server returned error message.
		/// </summary>
		public static async Task<T> try_catch_server_exception<T>(Func<Task<T>> function)
		{
			try
			{
				return await function();
			}
			catch (Exception ex)
			{
				if (ex is AggregateException aggex)
				{
					ex = aggex.InnerException;
				}

				if (ex is FlurlHttpException fhttpex)
				{
					string serverErrorMessage = null;
					try
					{
						string json = fhttpex.Call.Response.GetStringAsync().Result;
						JObject obj = JObject.Parse(json);
						// Sometimes error message resides in result value itself.
						if (obj.ContainsKey("value"))
						{
							obj = obj["value"].ToObject<JObject>();
						}
						serverErrorMessage = obj.GetValue("message", StringComparison.OrdinalIgnoreCase)?.Value<string>();
					}
					catch
					{
						// Could not extract server side error , just continue with original exception.
					}

					if (serverErrorMessage != null)
					{
						throw new ArgumentException(serverErrorMessage);
					}
				}
				throw ex;
			}
		}

		public async Task<IEnumerable<UserEntitlement>> ListUserEntitlementsAsync(string organization, NetworkCredential credentials)
		{
			// https://learn.microsoft.com/en-us/rest/api/azure/devops/memberentitlementmanagement/user-entitlements/get-user-entitlements?view=azure-devops-rest-5.1

			var result = await $"https://vsaex.dev.azure.com/{organization}/_apis/userentitlements?top=10000&api-version=5.1-preview.2"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.GetJsonNamedNodeAsync<IEnumerable<UserEntitlement>>("members")
				.ConfigureAwait(false);

			return result;
		}

		public async Task<IEnumerable<TeamProjectReference>> ListProjectsAsync(string organization, NetworkCredential credentials)
		{
			var result = await $"https://dev.azure.com/{organization}/_apis/projects?api-version=6.0"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.GetJsonNamedNodeAsync<IEnumerable<TeamProjectReference>>("value")
				.ConfigureAwait(false);

			return result;
		}

		public async Task<IEnumerable<GitRepository>> ListRepositoriesAsync(string organization, string projectName, NetworkCredential credentials)
		{
			var result = await $"https://dev.azure.com/{organization}/{projectName}/_apis/git/repositories?api-version=6.0"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.GetJsonFirstNodeAsync<IEnumerable<GitRepository>>()
				.ConfigureAwait(false);

			return result;
		}

		public async Task<IEnumerable<GitPullRequest>> ListPullRequestsAsync(string organization, string projectName, string repositoryId, NetworkCredential credentials, 
			PullRequestQueryOptions queryopt = null)
		{
			string query = "";

			if (queryopt != null)
			{
				if (queryopt.Skip.HasValue)
				{
					query += $"$skip={queryopt.Skip.Value}&";
				}

				if (queryopt.Top.HasValue)
				{
					query += $"$top={queryopt.Top.Value}&";
				}

				if (queryopt.Status.HasValue)
				{
					query += $"searchCriteria.status={queryopt.Status.Value}&";
				}
			}

			query += $"api-version=6.0";
			string url = $"https://dev.azure.com/{organization}/{projectName}/_apis/git/repositories/{repositoryId}/pullrequests?{query}";

			return await try_catch_server_exception(async () =>
			{
				var result = await url
					.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
					.WithBasicAuth(credentials.UserName, credentials.Password)
					.WithHeader("Accept", "application/json")
					.GetJsonFirstNodeAsync<IEnumerable<GitPullRequest>>()
					.ConfigureAwait(false);
				return result;
			});		
		}

		public async Task<IEnumerable<PolicyConfiguration>> ListPolicyConfigurations(string organization, string project, NetworkCredential credentials)
		{
			return await try_catch_server_exception(async () =>
			{
				var result = await $"https://dev.azure.com/{organization}/{project}/_apis/policy/configurations?api-version=6.0"
					.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
					.WithBasicAuth(credentials.UserName, credentials.Password)
					.WithHeader("Accept", "application/json")
                    .GetJsonNamedNodeAsync<IEnumerable<PolicyConfiguration>>("value")
                    .ConfigureAwait(false);

                return result;
            });
		}

		static string FirstCharToLowerCase(string str)
		{
			if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
				return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

			return str;
		}

		string SerializeQuery(object query)
		{
			List<String> properties = new List<string>();
			if (query != null)
			{
				foreach (var p in query.GetType().GetProperties())
				{
					var value = p.GetValue(query, null);
					if (value == null)
						continue;

					string key = p.Name;
					var jprop = p.GetCustomAttribute<JsonPropertyAttribute>();
					if (jprop != null)
						key = jprop.PropertyName;

					key = FirstCharToLowerCase(key);
					string valueEscaped = HttpUtility.UrlEncode(value.ToString());
					properties.Add($"{key}={valueEscaped}");
				}
			}

			properties.Add("api-version=6.0");

			// queryString will be set to "Id=1&State=26&Prefix=f&Index=oo"                  
			string queryString = String.Join("&", properties);
			return queryString;
		}


		public async Task<IEnumerable<PolicyConfiguration>> ListPolicyConfigurations(NetworkCredential credentials, string organization, string project, PolicyConfigurationsQuery queryopt = null)
		{
			if (queryopt == null)
			{
				return await ListPolicyConfigurations(organization, project, credentials);
			}
			
			if (queryopt.RepositoryId != null && queryopt.RefName == null)
			{
				throw new ArgumentException($"{nameof(PolicyConfigurationsQuery.RefName)} must be set");
			}
			
			string url = $"https://dev.azure.com/{organization}/{project}/_apis/git/policy/configurations?{SerializeQuery(queryopt)}";

			return await try_catch_server_exception(async () =>
			{
				var result = await url
					.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
					.WithBasicAuth(credentials.UserName, credentials.Password)
					.WithHeader("Accept", "application/json")
					.GetJsonNamedNodeAsync<IEnumerable<PolicyConfiguration>>("value")
					.ConfigureAwait(false);

				return result;
			});
		}

		public async Task<IEnumerable<Build>> ListBuildsAsync(NetworkCredential credentials, string organization, string project, BuildListQuery queryopt = null)
		{
			string url = $"https://dev.azure.com/{organization}/{project}/_apis/build/builds?{SerializeQuery(queryopt)}";

			var result = await url
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.GetJsonNamedNodeAsync<IEnumerable<Build>>("value")
				.ConfigureAwait(false);

			return result;
		}

		public async Task<Build> GetBuildAsync(NetworkCredential credentials, string organization, string projectName, int buildId)
		{
			var result = await $"https://dev.azure.com/{organization}/{projectName}/_apis/build/builds/{buildId}?api-version=6.0"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.GetJsonAsync<Build>()
				.ConfigureAwait(false);

			return result;
		}

		public async Task<Build> UpdateBuildAsync(NetworkCredential credentials, string organization, string projectName, int buildId, object body)
		{
			var result = await $"https://dev.azure.com/{organization}/{projectName}/_apis/build/builds/{buildId}?api-version=6.0"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.PatchJsonAsync(body)
				.ReceiveJson<Build>()
				.ConfigureAwait(false);

			return result;
		}


		public async Task<PolicyConfiguration> CreatePolicyConfiguration(string organization, string project, NetworkCredential credentials, object policy)
		{
			return await try_catch_server_exception(async () =>
			{
				var result = await $"https://dev.azure.com/{organization}/{project}/_apis/policy/configurations?api-version=6.0"
					.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
					.WithBasicAuth(credentials.UserName, credentials.Password)
					.WithHeader("Accept", "application/json")
					.PostJsonAsync(policy)
					.ReceiveJson<PolicyConfiguration>()
					.ConfigureAwait(false);

				return result;
			});
		}

		public async Task<PolicyConfiguration> UpdatePolicyConfiguration(string organization, string project, NetworkCredential credentials, PolicyConfiguration policy)
		{
			return await try_catch_server_exception(async () =>
			{
				var result = await $"https://dev.azure.com/{organization}/{project}/_apis/policy/configurations/{policy.Id}?api-version=6.0"
					.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
					.WithBasicAuth(credentials.UserName, credentials.Password)
					.WithHeader("Accept", "application/json")
					.PutJsonAsync(policy)
					.ReceiveJson<PolicyConfiguration>()
					.ConfigureAwait(false);

				return result;
			});
		}


		public async Task<GitPullRequest> CreatePullRequestAsync(string organization, string projectName, string repositoryId, NetworkCredential credentials, object body)
		{
			var result = await $"https://dev.azure.com/{organization}/{projectName}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=6.0"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.PostJsonAsync(body)
				.ReceiveJson<GitPullRequest>()
				.ConfigureAwait(false);

			return result;
		}

		private async Task<GitPullRequest> UpdatePullRequestAsync(string organization, string projectName, string repositoryId, NetworkCredential credentials, int pullRequestId, object body)
		{
			var result = await $"https://dev.azure.com/{organization}/{projectName}/_apis/git/repositories/{repositoryId}/pullrequests/{pullRequestId}?api-version=6.0"
				.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
				.WithBasicAuth(credentials.UserName, credentials.Password)
				.WithHeader("Accept", "application/json")
				.PatchJsonAsync(body)
				.ReceiveJson<GitPullRequest>()
				.ConfigureAwait(false);

			return result;
		}

		void PrintJsonMessage(object msg)
		{
            string jsonText = JsonConvert.SerializeObject(msg, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

			Console.WriteLine($">> {jsonText}");
        }

		/// <summary>
		/// Runs Azure devops pipeline task
		/// </summary>
		/// <param name="branchName">branch name, null if default one</param>
		/// <param name="args">parameters to be passed to build</param>
		/// <returns></returns>
		public async Task<RunPipelineResult> RunTask(string organization, int pipelineId, string projectName, NetworkCredential credentials, 
			string branchName, params string[] args)
		{
			if (args.Length % 2 != 0)
			{
				throw new ArgumentException("RunTask: Must provide even count of arguments (key-value pairs)");
			}

			Dictionary<string, string> d = new Dictionary<string, string>();
			for (int i = 0; i < args.Length; i += 2)
			{
				d.Add(args[i], args[i + 1]);
			}

			RunPipeline runpipeline = new RunPipeline() { TemplateParameters = d };
			if(!string.IsNullOrEmpty(branchName))
			{
				// see also https://stackoverflow.com/questions/66445303/how-do-you-specify-the-sourcebranch-for-a-run-of-pipelines-via-the-rest-api
				RunResourcesParameters rparams = new RunResourcesParameters();
				runpipeline.Resources = rparams;
				rparams.Repositories = new Dictionary<string, RepositoryResourceParameters>();
				rparams.Repositories.Add("self", new RepositoryResourceParameters() { RefName = branchName.MakeRefSpec() });
			}

			// PrintJsonMessage(runpipeline);

			int pipelineVersion = 1;

			return await try_catch_server_exception(async () =>
				{
					var result = await $"https://dev.azure.com/{organization}/{projectName}/_apis/pipelines/{pipelineId}/runs?pipelineVersion={pipelineVersion}&api-version=6.0"
						.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
						.WithBasicAuth(credentials.UserName, credentials.Password)
						.WithHeader("Accept", "application/json")
						.PostJsonAsync(runpipeline)
						.ReceiveJson<RunPipelineResult>()
						.ConfigureAwait(false);

					return result;
				});
		}


		public async Task<GitPullRequest> AutoCompletePullRequestAsync(string organization, string projectName, string repositoryId, NetworkCredential credentials, int pullRequestId, IdentityRef autoCompleteSetByIdentityRef)
		{
			var body = new
			{
				autoCompleteSetBy = autoCompleteSetByIdentityRef
			};
			var result = await UpdatePullRequestAsync(organization, projectName, repositoryId, credentials, pullRequestId, body);
			return result;
		}

		// Generally UpdatePullRequestAsync cannot be used to update pull request in context of which we are running on build machine.
		// You will get "Call failed with status code 400 (Bad Request)"

		public async Task<GitPullRequest> UpdatePullRequestAsync(string organization, string projectName, string repositoryId, 
			NetworkCredential credentials, int pullRequestId, IdentityRef autoCompleteSetByIdentityRef, GitPullRequestCompletionOptions completionOptions)
		{
			var body = new
			{
				autoCompleteSetBy = autoCompleteSetByIdentityRef,
				completionOptions = completionOptions
			};

			var result = await UpdatePullRequestAsync(organization, projectName, repositoryId, credentials, pullRequestId, body);
			return result;
		}

		public async Task ApprovePullRequestAsync(NetworkCredential credentials, string organization, string project, string repositoryId, GitPullRequest pr)
		{
			foreach (var reviewer in pr.Reviewers)
			{
				string url = $"https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryId}/pullRequests/{pr.PullRequestId}/reviewers/{reviewer.Id}?api-version=6.0";

				var body = new
				{
					vote = Votes.Approved,
					isRequired = true
				};

				await url
					.ConfigureRequest(settings => settings.JsonSerializer = s_serializer)
					.WithBasicAuth(credentials.UserName, credentials.Password)
					.WithHeader("Accept", "application/json")
					.PutJsonAsync(body)
					.ReceiveJson<IdentityRefWithVote>()
					.ConfigureAwait(false);
			}
		}

		public async Task<GitPullRequest> AbandonPullRequestAsync(string organization, string projectName, string repositoryId, NetworkCredential credentials, int pullRequestId)
		{
			var body = new
			{
				status = PullRequestStatus.Abandoned.ToString().ToLowerInvariant()
			};
			var result = await UpdatePullRequestAsync(organization, projectName, repositoryId, credentials, pullRequestId, body);
			return result;
		}

		public async Task<GitPullRequest> DeletePullRequestSourceBranchAsync(string organization, string projectName, string repositoryId, NetworkCredential credentials, int pullRequestId)
		{
			var body = new
			{
				completionOptions = new GitPullRequestCompletionOptions
				{
					DeleteSourceBranch = true
				}
			};
			var result = await UpdatePullRequestAsync(organization, projectName, repositoryId, credentials, pullRequestId, body);
			return result;
		}
	}
}

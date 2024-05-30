using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Conductor.AzureDevOps.Api.Extensions;
using Conductor.AzureDevOps.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Conductor.AzureDevOps.Api
{
	public class AzureDevOpsApiWrapper
	{
		private static readonly AzureDevOpsApi s_api = new AzureDevOpsApi();

		public async Task<IEnumerable<AzureDevOpsUser>> ListUserEntitlementsAsync(string organization, NetworkCredential credentials)
		{
			var result = await s_api.ListUserEntitlementsAsync(organization, credentials);
			return result
				.Select(x => new AzureDevOpsUser(new Guid(x.Id), x.User.DisplayName, x.User.MailAddress));
		}

		public async Task<IEnumerable<AzureDevOpsProject>> ListProjectsAsync(string organization, NetworkCredential credentials)
		{
			var result = await s_api.ListProjectsAsync(organization, credentials);
			return result
				.Select(x => new AzureDevOpsProject(x.Id, x.Name));
		}

		public async Task<IEnumerable<AzureDevOpsRepository>> ListRepositoriesAsync(string organization, string projectName, NetworkCredential credentials)
		{
			var result = await s_api.ListRepositoriesAsync(organization, projectName, credentials);
			return result
				.Select(x => new AzureDevOpsRepository(x.Name, x.DefaultBranch, x.RemoteUrl));
		}

		public async Task<IEnumerable<PolicyConfiguration>> ListBuildPolicyConfigurations(NetworkCredential credentials, string organization, string project, string repositoryName, string branchName)
		{ 
            var repos = await s_api.ListRepositoriesAsync(organization, project, credentials);
			var repo = repos.FirstOrDefault(x => x.Name.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));
			if (repo == null)
			{
				throw new ArgumentException($"Repository '{repositoryName}' not found in organization {organization} / project {project}");
			}

			var query = new PolicyConfigurationsQuery()
			{ 
				RepositoryId = repo.Id, RefName = branchName.MakeRefSpec(), PolicyType = PolicyTypeRef.BuildPolicyType 
			};
			
			return await s_api.ListPolicyConfigurations(credentials, organization, project, query);
		}

		public async Task<string> GetRepositoryDefaultBranchAsync(string organization, string projectName, NetworkCredential credentials, string repositoryName)
		{
			var repos = await ListRepositoriesAsync(organization, projectName, credentials);
			return repos
				.FirstOrDefault(x => x.Name.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase))
				?.DefaultBranch;
		}

		public object MakePullRequest(string sourceBranchName, IEnumerable<object> reviewerIds, string targetBranchName = "main")
		{
			return new
			{
				sourceRefName = sourceBranchName.MakeRefSpec(),
				targetRefName = targetBranchName.MakeRefSpec(),
				title = sourceBranchName,
				description = sourceBranchName,
				reviewers = reviewerIds
			};
		}

		public string SerializePullRequest(string sourceBranchName, IEnumerable<object> reviewerIds, string targetBranchName = "main")
		{
			return JsonConvert.SerializeObject(MakePullRequest(sourceBranchName, reviewerIds, targetBranchName), new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				NullValueHandling = NullValueHandling.Ignore
			});
		}

		public async Task<IEnumerable<object>> GetReviewerIdsByEmailAsync(string organization, NetworkCredential credentials, params string[] emailAddresses)
		{
			var results = (await s_api.ListUserEntitlementsAsync(organization, credentials))
				.Where(x => emailAddresses.Any(email => x.User.MailAddress.Equals(email, StringComparison.InvariantCultureIgnoreCase)))
				.Select(x => new
				{
					id = $"{x.Id}"
				});

			return results;
		}

		public GitPullRequest CreatePullRequest(NetworkCredential credentials, string organization, string projectName, string repositoryName,
			string sourceBranch, string targetBranch, string description, bool autoComplete, bool approved, IdentityRef creator, params string[] reviewerIds)
		{
			return CreatePullRequestAsync(credentials, organization, projectName, repositoryName, sourceBranch, targetBranch, description, autoComplete, approved, creator, reviewerIds).Result;
		}

		public async Task<GitPullRequest> CreatePullRequestAsync(NetworkCredential credentials, string organization, string projectName, string repositoryName, 
			string sourceBranch, string targetBranch, string description, bool autoComplete, bool approved, IdentityRef creator, params string[] reviewerIds)
		{
			var completionOptions = new GitPullRequestCompletionOptions()
			{
				DeleteSourceBranch = true,
				MergeStrategy = GitPullRequestMergeStrategy.Rebase
			};

			GitPullRequest preq = new GitPullRequest()
			{
				Title = $"Merge {sourceBranch} into {targetBranch}",
				SourceRefName = sourceBranch.MakeRefSpec(),
				TargetRefName = targetBranch.MakeRefSpec(),
				Reviewers = reviewerIds.Select(x => new IdentityRefWithVote() { Id = x, IsRequired = true }).ToArray(),
				CompletionOptions = completionOptions,
				Description = description,
				CreatedBy = creator
			};

			if (creator != null)
			{
				// Even thus we assign this field, it will be ignored. See:
				// https://github.com/MicrosoftDocs/vsts-rest-api-specs/issues/403
				// https://developercommunity.visualstudio.com/t/changed-createdby-with-git-create-pull-request-api/459964
				preq.CreatedBy = creator;
			}

			return await AzureDevOpsApi.try_catch_server_exception(async () =>
			{
				var repository = (await s_api.ListRepositoriesAsync(organization, projectName, credentials))
					.FirstOrDefault(x => x.Name.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));
				if (repository is null)
				{
					return default;
				}

				var result = await s_api.CreatePullRequestAsync(organization, projectName, repository.Id, credentials, preq);
				if (autoComplete)
				{
					result = await s_api.UpdatePullRequestAsync(organization, projectName, 
						repository.Id, credentials, result.PullRequestId, result.CreatedBy, completionOptions);
				}

				if (approved)
				{
					await s_api.ApprovePullRequestAsync(credentials, organization, projectName, repository.Id, result);
				}

				return result;
			});
		}

		public async Task<GitPullRequest> AbandonPullRequestAsync(string organization, string projectName, NetworkCredential credentials, string repositoryName, int pullRequestId)
		{
			var repository = (await s_api.ListRepositoriesAsync(organization, projectName, credentials))
				.FirstOrDefault(x => x.Name.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));
			if (repository is null)
			{
				return default;
			}

			var result = await s_api.AbandonPullRequestAsync(organization, projectName, repository.Id, credentials, pullRequestId);
			return result;
		}
	}
}

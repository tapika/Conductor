# Overview

Here are stored what is considered as model of Azure Devops structures / classes.

There are serialized as such using REST protocol.

For example [Pull Requests - Get Pull Requests](https://docs.microsoft.com/en-us/rest/api/azure/devops/git/pull-requests/get-pull-requests?view=azure-devops-rest-6.0&tabs=HTTP) documentation specifies 
`GitPullRequest` - same named class is available in here as well.

First time when you compase new class and you don't know how it should be encoded -
you can use just `object` for it - like this:

	public class PolicyConfiguration
	{
		...
		public object Settings { get; set; }
		...
	}

If it's own structure - you can use also `Dictionary<string, object>` - until you replace it with class.

	public class PolicyConfiguration
	{
		...
		public Dictionary<string, object> Settings { get; set; }
		...
	}

=> 

	public class PolicyConfiguration
	{
		...
		public PolicyConfigurationSettings Settings { get; set; }
		...
	}

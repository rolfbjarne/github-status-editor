using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

using Mono.Options;

class MainClass {
	static JavaScriptSerializer js = new JavaScriptSerializer ();
	public async static Task<int> Main (string [] args)
	{
		var show_help = false;
		var hash = string.Empty;
		var repository = string.Empty;
		var authorization = string.Empty;
		var new_status = new GitHubStatus ();
		var set_state = string.Empty;
		var message = string.Empty;

		var os = new OptionSet {
			{ "h|help|?", "Show help", (v) => show_help = true },
			{ "hash=", "The hash whose statuses to view/edit", (v) => hash = v },
			{ "repository=", "The repository to access (use owner/repo notation, for instance xamarin/xamarin-macios)", (v) => repository = v },
			{ "add=", "Add a status with the specified state (error, failure, pending or success)", (v) => new_status.State = v },
			{ "target-url=", "The target url of the new/edited status", (v) => new_status.Target_Url = v },
			{ "description=", "The description of the new/edited status", (v) => new_status.Description = v },
			{ "context=", "The context of the new status", (v) => new_status.Context = v },
			{ "authorization=", "The personal access token to use to authorize with GitHub", (v) => authorization = v },
			{ "set=", "Sets statuses to the specified state. If --context is passed, only statuses with the specified context will be changed.", (v) => set_state = v },
			{ "message=", "A message that will be appended to the tracking comment on the commit. Prepend with @ to specify a filename.", (v) => message = v },
		};

		var others = os.Parse (args);
		if (others.Count > 0) {
			Console.WriteLine ("Unexpected argument: {0}", others [0]);
			return 1;
		}

		if (show_help || string.IsNullOrEmpty (hash) || string.IsNullOrEmpty (repository)) {
			Console.WriteLine ("github-status-editor [OPTIONS]");
			os.WriteOptionDescriptions (Console.Out);
			return 0;
		}

		var hc = new HttpClient ();
		hc.DefaultRequestHeaders.Add ("User-Agent", "xcurl/0.1");
		hc.DefaultRequestHeaders.Add ("Accept", "*/*");
		if (!string.IsNullOrEmpty (authorization))
			hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue ("token", authorization);

		var rv = await ReportStatuses (hc, repository, hash);

		if (message.StartsWith ("@", StringComparison.Ordinal))
			message = File.ReadAllText (message.Substring (1));

		string comment = string.Empty;
		if (!string.IsNullOrEmpty (message)) {
			if (!string.IsNullOrEmpty (set_state)) {
				comment = $"Setting state to `{set_state}` ";
				if (!string.IsNullOrEmpty (new_status.Context)) {
					comment += $"where context is `{new_status.Context}`";
				} else {
					comment += "for all statuses";
				}
				comment += ".";
			} else if (new_status.State != null) {
				comment = $"Adding new state '{new_status.State}' with context '{new_status.Context}', target url '{new_status.Target_Url}' and description '{new_status.Description}'";
			}
			comment += "\n\n" + message;
		}


		if (!string.IsNullOrEmpty (set_state)) {
			Console.Write ("Changing all statuses");
			if (!string.IsNullOrEmpty (new_status.Context))
				Console.Write ($" with Context=\"{new_status.Context}\"");
			Console.WriteLine ($" to \"{set_state}\".");
			foreach (var ghs in rv.Statuses) {
				if (ghs.State == set_state) {
					// Status has the expected state
					Console.WriteLine ($"    Status with Context=\"{ghs.Context}\" Description=\"{ghs.Description}\" Target Url=\"{ghs.Target_Url}\" already has the expected state (\"{set_state}\")");
					continue;
				}
				if (!string.IsNullOrEmpty (new_status.Context) && ghs.Context != new_status.Context) {
					// Context doesn't match
					Console.WriteLine ($"    Status with Context=\"{ghs.Context}\" Description=\"{ghs.Description}\" Target Url=\"{ghs.Target_Url}\" does not match context \"{new_status.Context}\", so status won't be updated.");
					continue;
				}
				ghs.State = set_state;
				var logDescription = string.Empty;
				if (!string.IsNullOrEmpty (new_status.Description)) {
					ghs.Description = new_status.Description;
					logDescription = $" and to Description=\"{ghs.Description}\"";
				}
				if (!string.IsNullOrEmpty (new_status.Target_Url)) {
					ghs.Target_Url = new_status.Target_Url;
					logDescription += $" and to Target Url=\"{ghs.Target_Url}\"";
				}
				Console.WriteLine ($"    Setting status with Context=\"{ghs.Context}\" Description=\"{ghs.Description}\" Target Url=\"{ghs.Target_Url}\" to state=\"{set_state}\"{logDescription}");
				var rvS = await AddStatus (hc, ghs, repository, hash, authorization, comment);
				if (rvS != 0)
					return rvS;
			}

			await ReportStatuses (hc, repository, hash);
			return 0;
		}

		if (new_status.State != null)
			return await AddStatus (hc, new_status, repository, hash, authorization, comment);

		return 0;
	}

	static string GetEmojii (string state)
	{
		switch (state) {
		case "failure":
		case "error":
			return "❌";
		case "success":
			return "✅";
		case "pending":
			return "⚠️ ";
		default:
			return "❔";
		}
	}

	async static Task<GitHubStatuses> ReportStatuses (HttpClient hc, string repository, string hash)
	{
		string statuses = null;
		try {
			var url = $"https://api.github.com/repos/{repository}/commits/{hash}/status";
			//Console.WriteLine ($"Downloading: {url}");
			statuses = await hc.GetStringAsync (url);
		} catch (WebException e) {
			DumpWebException (e);
			return null;
		}

		var js = new JavaScriptSerializer ();
		var rv = js.Deserialize<GitHubStatuses> (statuses);

		Console.WriteLine ($"Commit {hash} has {rv.Statuses?.Length ?? 0} statuses, whose combined value is \"{rv.State}\".");
		if (rv.Statuses != null) {
			for (int i = 0; i < rv.Statuses.Length; i++) {
				var s = rv.Statuses [i];
				Console.WriteLine ($"    #{i + 1}: {GetEmojii (s.State)} State=\"{s.State}\" Context=\"{s.Context}\" Description=\"{s.Description}\" Target Url=\"{s.Target_Url}\"");
			}
		}
		return rv;
	}

	static bool commented;
	async static Task<int> AddStatus (HttpClient wc, GitHubStatus status, string repository, string hash, string authorization, string comment)
	{
		try {
			if (string.IsNullOrEmpty (comment)) {
				Console.WriteLine ($"{GetEmojii ("error")} A message is required when adding/switching status(es).");
				return 1;
			}

			if (!commented) {
				var rv = await AddComment (wc, repository, hash, comment);
				if (rv != 0)
					return rv;
				commented = true;
			}

			var url = $"https://api.github.com/repos/{repository}/statuses/{hash}";
			//Console.WriteLine ($"Adding status using url: {url}");
			var data = $@"
{{
""state"": {js.Serialize (status.State)},
""target_url"": {js.Serialize (status.Target_Url)},
""description"": {js.Serialize (status.Description)},
""context"": {js.Serialize (status.Context)}
}}
";
			var content = new StringContent (data);
			var task = wc.PostAsync (url, content);
			var create_status = await task;

			//Console.WriteLine ($"Creation result: {wc}");
			//foreach (var h in create_status.RequestMessage.Headers)
			//	Console.WriteLine ($"{h.Key}={h.Value}");
			//Console.WriteLine ($"Status creation result: {create_status}");
			return 0;
		} catch (WebException e) {
			DumpWebException (e);
			return 1;
		}
	}

	async static Task<int> AddComment (HttpClient wc, string repository, string hash, string comment)
	{
		try {
			var url = $"https://api.github.com/repos/{repository}/commits/{hash}/comments";
			var data = $@"
{{
""body"": {js.Serialize (comment)}
}}
";
			var content = new StringContent (data);
			var task = wc.PostAsync (url, content);
			var create_status = await task;
			if (!create_status.IsSuccessStatusCode) {
				Console.WriteLine ($"{GetEmojii ("error")} Failed to add comment: {await create_status.Content.ReadAsStringAsync ()}");
				return 1;
			}
			return 0;
		} catch (WebException e) {
			DumpWebException (e);
			return 1;
		}
	}

	static void DumpWebException (WebException e)
	{
		Console.WriteLine (e);
		Console.WriteLine (e.GetType ());
		var response = (HttpWebResponse)e.Response;
		Console.WriteLine ("Status Code : {0}", response.StatusCode);
		Console.WriteLine ("Status Description : {0}", response.StatusDescription);
		foreach (string header in response.Headers.Keys)
			Console.WriteLine ($"{header}={response.Headers [header]}");
		using (var reader = new StreamReader (response.GetResponseStream ())) {
			Console.WriteLine (reader.ReadToEnd ());
		}
	}
}

#pragma warning disable 0649
class GitHubStatus {
	public string Url;
	public string Id;
	public string Node_Id;
	public string State;
	public string Description;
	public string Target_Url;
	public string Context;
	public string Created_At;
	public string Updated_At;
}

class GitHubStatuses {
	public string State;
	public GitHubStatus [] Statuses;
}
#pragma warning restore 0649
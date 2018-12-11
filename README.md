# GitHub Status Editor

This is a small command-line tool that can list and edit/add GitHub statuses.

It needs a Personal Access Token from GitHub, which can be created
[here](https://github.com/settings/tokens). The token (probably) needs
`Full control of private repositories` (it might be enough with
`Access public repositories`, depending on the repository).

## Getting Started

In order to use the tool clone the repo and run `make all` at the root of the repo.

### Update your path

If you want to be able to call the tool from anywhere in your terminal you have two options:

1. Adding the tool to your path temporarily by executing

``` bash
export PATH=$PATH:path/to/github-status-editor
```

2. Adding the tool to your path permanently by updating your `~/.bashrc`, `~/.zshrc` or equivalent. Simply find the existing `export PATH` there and add `path/to/github-status-editor` at the end.

## Examples

### List the statuses for a commit

```
$ github-status-editor --repository xamarin/xamarin-macios --hash 4fd5e4001144ca3a2b991f2e77c9783963db185
Commit 4fd5e4001144ca3a2b991f2e77c9783963db1851 has 9 statuses, whose combined value is "pending".
    #1: ⚠️  State="pending" Context="Build" Description="Build triggered for merge commit." Target Url=""
    #2: ✅ State="success" Context="license/cla" Description="All CLA requirements met." Target Url="https://cla.dotnetfoundation.org/xamarin/xamarin-macios?pullRequest=4312"
    #3: ✅ State="success" Context="PKG-Xamarin.iOS" Description="xamarin.ios-11.13.0.23.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/PR-4312/4fd5e4001144ca3a2b991f2e77c9783963db1851/5/package/xamarin.ios-11.13.0.23.pkg"
    #4: ✅ State="success" Context="PKG-Xamarin.Mac" Description="xamarin.mac-4.5.0.396.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/PR-4312/4fd5e4001144ca3a2b991f2e77c9783963db1851/5/package/xamarin.mac-4.5.0.396.pkg"
    #5: ✅ State="success" Context="manifest" Description="manifest" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/PR-4312/4fd5e4001144ca3a2b991f2e77c9783963db1851/5/package/manifest"
    #6: ✅ State="success" Context="Jenkins: Artifacts" Description="artifacts.json" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/PR-4312/4fd5e4001144ca3a2b991f2e77c9783963db1851/5/package/artifacts.json"
    #7: ✅ State="success" Context="bundle.zip" Description="bundle.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/PR-4312/4fd5e4001144ca3a2b991f2e77c9783963db1851/5/package/bundle.zip"
    #8: ✅ State="success" Context="msbuild.zip" Description="msbuild.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/PR-4312/4fd5e4001144ca3a2b991f2e77c9783963db1851/5/package/msbuild.zip"
    #9: ✅ State="success" Context="continuous-integration/jenkins/pr-head" Description="This commit looks good" Target Url="http://xamarin-jenkins.guest.corp.microsoft.com:8080/job/macios/job/PR-4312/5/display/redirect"
```

### Switch all statuses for a commit to 'success'

```
$ github-status-editor --repository=rolfbjarne/testApp --hash=8b4774fd5a2f36ceeaa2fa2529349dd480986077 --set=success --authorization=<GitHub PAT token> --message "hello galaxy"
Commit 8b4774fd5a2f36ceeaa2fa2529349dd480986077 has 3 statuses, whose combined value is "failure".
    #1: ❌ State="error" Context="add-context-failure" Description="add-description" Target Url=""
    #2: ❌ State="error" Context="add-context" Description="add-description" Target Url=""
    #3: ❌ State="error" Context="add-context2" Description="add-description" Target Url=""
Changing all statuses to "success".
    Setting status with Context="add-context-failure" Description="add-description" Target Url="" to state="success"
    Setting status with Context="add-context" Description="add-description" Target Url="" to state="success"
    Setting status with Context="add-context2" Description="add-description" Target Url="" to state="success"
Commit 8b4774fd5a2f36ceeaa2fa2529349dd480986077 has 3 statuses, whose combined value is "success".
    #1: ✅ State="success" Context="add-context-failure" Description="add-description" Target Url=""
    #2: ✅ State="success" Context="add-context" Description="add-description" Target Url=""
    #3: ✅ State="success" Context="add-context2" Description="add-description" Target Url=""
```

### Switch a specific context to 'failure'

```
$ ./github-status-editor --repository=rolfbjarne/testApp --hash=8b4774fd5a2f36ceeaa2fa2529349dd480986077 --set=failure --authorization=<GitHub PAT token> --context=add-context-failure --message "hello universe"
Commit 8b4774fd5a2f36ceeaa2fa2529349dd480986077 has 3 statuses, whose combined value is "success".
    #1: ✅ State="success" Context="add-context-failure" Description="add-description" Target Url=""
    #2: ✅ State="success" Context="add-context" Description="add-description" Target Url=""
    #3: ✅ State="success" Context="add-context2" Description="add-description" Target Url=""
Changing all statuses with Context="add-context-failure" to "failure".
    Setting status with Context="add-context-failure" Description="add-description" Target Url="" to state="failure"
    Status with Context="add-context" Description="add-description" Target Url="" does not match context "add-context-failure", so status won't be updated.
    Status with Context="add-context2" Description="add-description" Target Url="" does not match context "add-context-failure", so status won't be updated.
Commit 8b4774fd5a2f36ceeaa2fa2529349dd480986077 has 3 statuses, whose combined value is "failure".
    #1: ✅ State="success" Context="add-context" Description="add-description" Target Url=""
    #2: ✅ State="success" Context="add-context2" Description="add-description" Target Url=""
    #3: ❌ State="failure" Context="add-context-failure" Description="add-description" Target Url=""
```

### Switch a specific context to 'success', providing a file as the message

```
$ echo "No blocking issues found" > message.txt
$ ./github-status-editor --repository=xamarin/xamarin-macios --hash=807efadf3838d4bb28ff1fe8a1b4561b074de8ec --set=success --authorization=<GitHub PAT token> --context=continuous-integration/jenkins/branch --message @message.txt
Commit 807efadf3838d4bb28ff1fe8a1b4561b074de8ec has 7 statuses, whose combined value is "failure".
    #1: ✅ State="success" Context="PKG-Xamarin.iOS" Description="xamarin.ios-11.99.3.585.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/xamarin.ios-11.99.3.585.pkg"
    #2: ✅ State="success" Context="PKG-Xamarin.Mac" Description="xamarin.mac-4.99.3.705.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/xamarin.mac-4.99.3.705.pkg"
    #3: ✅ State="success" Context="manifest" Description="manifest" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/manifest"
    #4: ✅ State="success" Context="Jenkins: Artifacts" Description="artifacts.json" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/artifacts.json"
    #5: ✅ State="success" Context="bundle.zip" Description="bundle.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/bundle.zip"
    #6: ✅ State="success" Context="msbuild.zip" Description="msbuild.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/msbuild.zip"
    #7: ❌ State="error" Context="continuous-integration/jenkins/branch" Description="This commit cannot be built" Target Url="https://jenkins.internalx.com/job/macios/job/xcode10/170/display/redirect"
Changing all statuses with Context="continuous-integration/jenkins/branch" to "success".
    Status with Context="PKG-Xamarin.iOS" Description="xamarin.ios-11.99.3.585.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/xamarin.ios-11.99.3.585.pkg" already has the expected state ("success")
    Status with Context="PKG-Xamarin.Mac" Description="xamarin.mac-4.99.3.705.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/xamarin.mac-4.99.3.705.pkg" already has the expected state ("success")
    Status with Context="manifest" Description="manifest" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/manifest" already has the expected state ("success")
    Status with Context="Jenkins: Artifacts" Description="artifacts.json" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/artifacts.json" already has the expected state ("success")
    Status with Context="bundle.zip" Description="bundle.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/bundle.zip" already has the expected state ("success")
    Status with Context="msbuild.zip" Description="msbuild.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/msbuild.zip" already has the expected state ("success")
    Setting status with Context="continuous-integration/jenkins/branch" Description="This commit cannot be built" Target Url="https://jenkins.internalx.com/job/macios/job/xcode10/170/display/redirect" to state="success"
Commit 807efadf3838d4bb28ff1fe8a1b4561b074de8ec has 7 statuses, whose combined value is "success".
    #1: ✅ State="success" Context="PKG-Xamarin.iOS" Description="xamarin.ios-11.99.3.585.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/xamarin.ios-11.99.3.585.pkg"
    #2: ✅ State="success" Context="PKG-Xamarin.Mac" Description="xamarin.mac-4.99.3.705.pkg" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/xamarin.mac-4.99.3.705.pkg"
    #3: ✅ State="success" Context="manifest" Description="manifest" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/manifest"
    #4: ✅ State="success" Context="Jenkins: Artifacts" Description="artifacts.json" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/artifacts.json"
    #5: ✅ State="success" Context="bundle.zip" Description="bundle.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/bundle.zip"
    #6: ✅ State="success" Context="msbuild.zip" Description="msbuild.zip" Target Url="https://bosstoragemirror.blob.core.windows.net/wrench/jenkins/xcode10/807efadf3838d4bb28ff1fe8a1b4561b074de8ec/170/package/msbuild.zip"
    #7: ✅ State="success" Context="continuous-integration/jenkins/branch" Description="This commit cannot be built" Target Url="https://jenkins.internalx.com/job/macios/job/xcode10/170/display/redirect"
```

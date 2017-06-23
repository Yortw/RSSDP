
Thank you for considering contributing to this project! Help is always appreciated, though it's best to check in via an issue before starting any major work.

Following these guidelines helps to communicate that you respect the time of the developers managing and developing this open source project. 
In return, they should reciprocate that respect in addressing your issue, assessing changes, and helping you finalize your pull requests. Following the 
guidelines helps everybody, including you, as it can reduce the time to get issues resovled and prevent work from being undertaken that might not be accepted later.


# Issues
* Please check if there is an existing issue (open or closed with an appropriate solution) before opening a new one.
* Please include as much detail as possible, including the .Net platform you're using (Desktop framework, Xamarin iOS/Android, Net Standard etc),
your version of visual studio, the version of the project or nuget package you're using, exact error mesages/numbers and if possible sample code or 
a link to a project that reproduces the issue.
* We endeavour to respond to issues quickly, but the project is unpaid and run on the personal time of our contributors (of whom there are few, possibly only one). 
We have day jobs, as well as lives, so while we will respond as soon as possible, it might take several days or a week. If no response after a week, please 
feel free to ping the thread/post a second request.
* Please open one issue per problem/request/idea. Combining multiple concepts/reports into a single issue makes it hard to track, read, and close appropriately.

# Pull Requests and Code
* Always a good idea to raise an issue for the work before you start, in case the work doesn't fit with the project goals, there has already
been some design work/thoughts about the work you're doing etc.
* Please try to follow the coding style already used in the project. We don't want to be anal-retentive about every character of whitespace, 
but it helps all contributors/maintainers if the code is relatively consistent. Following the naming/cashing/indentation/bracing styles that are already used in the code.
If the project includes and .editorconfig, please enable support for that in your editor if possible.
* If the project has a code analysis ruleset, please ensure your run code analysis before submitting and fix any issues. CA issues may be suppressed, but must be 
with good reason/thought - using the "justifcation" property on the suppression attribute in code is an excellent way of explaining why the suppression is sensible, though
a comment is also good.
* If the project has xml documentation of public types/members, please ensure your document any new members/update documentation for exsiting members. Please ensure the documentation 
goes beyond what is obvious from the member name (where appropriate), i.e which arguments can be null, empty string, negative or zero values, does the method ever return null or empty values,
[is it thread-safe](https://blogs.msdn.microsoft.com/ericlippert/2009/10/19/what-is-this-thing-you-call-thread-safe/) (and if yes, in what context/usage scenario).
* If the project has associated unit/integration tests, please ensure all changed or new code is suitably tested. We don't demand 100% code coverage, but we want at least the 
'happy path' and the 'obvious unhappy paths' validated.
* Please make changes in a feature or fix branch, not the main branch.
* If the code supports multiple platforms, please ensure your change works on all platforms. If there is a reason why any given platform cannot be supported, or why you cannot develop or test 
for that platform, please raise an issue to discuss. Other's may be able to do or help with the work/testing you cannot. If there's a good/obvious reason why new code/changes don't work on a particular 
platform then it may be ok to support it on only some platforms, but agreement must be reached first.
We endeavour to respond to pull requests quickly, but the project is unpaid and run on the personal time of our contributors (of whom there are few, possibly only one). 
We have day jobs, as well as lives, so while we will respond as soon as possible, it might take several days or a week before we can even start looking. We will need to review the code 
before deciding whether or not to merge it in it's current state (if at all), and will then need time to merge, test and prepare a release. Again, we'll do this as quickly as we can, but it 
is more important that it is done well, and we cannot make promises about timeframes. 
* Thanks! I look forward to merging your awesomesauce.

# Wiki and Documentation
* We'd love to have **great** documentation. We're not experts at it. If your can help, please do so - again, raising an issue with an offer of help/suggestion is a great way to get 
started. If the project as a wiki that's probably the best place to start, but alternate forms of documentation can be considered. 

# DebtRatchet [![Join the chat at https://gitter.im/keyboardDrummer/DebtRatchet](https://badges.gitter.im/keyboardDrummer/DebtRatchet.svg)](https://gitter.im/keyboardDrummer/DebtRatchet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Build status](https://ci.appveyor.com/api/projects/status/nd57ig4flxg9b71u?svg=true)](https://ci.appveyor.com/project/keyboardDrummer/debtratchet)

DebtRatchet is a tool that alerts you whenever technical debt increases, enabling you to systematically reduce debt in legacy codebases. It has been used in a large enterprise codebase succesfully, where it greatly increased the pace at which debt was reduced. 

DebtRatchet is implemented as a diagnostic for the .NET Roslyn compiler. To use it, simply install the [NuGet package](https://www.nuget.org/packages/DebtRatchet/1.0.0) into your project. DebtRachet runs when you compile, but also while you code. Note that using DebtRatchet requires VS 2015 update 3.

DebtRatchet uses some rules to determine what it considers debt. These rules are kept simple so that a programmer is never confused why code contains debt. Also, these rules can be configured, which should be done defensively to prevent false positives. The rules used are these:
* Method has too many parameters
* Method is too long
* Type has too many fields
* Type is too long

###### Related work
- [git-ratchet](https://gowalker.org/github.com/iangrunert/git-ratchet) is a tool that allows you to track metrics and apply a ratchet to them.
- [Quality](https://github.com/apiology/quality) is debt ratchet tool for Ruby.
- [git-code-debt](https://github.com/Yelp/git-code-debt) A dashboard for monitoring code debt in a git repository.

## Demo
Here follows a screenshot showing the plugin in action. Inside the interface are four scenario's all related to the rule 'Method has too many parameters'.

![Screenshot](http://i.imgur.com/12ye5JG.png)

## Workflow

For the plugin to report only increases in technical debt, and not go off on existing technical debt, it must be allowed to annotate your codebase in places where technical debt occurs. This is a one-time operation performed when you start using DebtRatchet. It is important to also tighten the ratchet every once in a while. This means updating the debt annotations after you've reduced technical debt. 


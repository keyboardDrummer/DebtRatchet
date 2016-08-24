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
The screenshot below shows DebtRatchet in action. The code contains an interface with four methods. Two of these methods are fine, but DebtRatchet finds the other two methods have increased in technical debt, resulting in an error. 

In the example you will see the attribute ```MethodHasDebt``` above two methods. This indicates already existing debt, allowing us to differentiate between new and old debt. 

![Screenshot](http://i.imgur.com/gD5iUz6.png)

You can see that hovering the mouse over a method with an error from DebtRatchet shows a tooltip. From the tooltip you can generate the MethodHasDebt attribute to quickly eliminate the error, in case you don't want to refactor your code at this time.

## How-to

- Install the [NuGet package](https://www.nuget.org/packages/DebtRatchet/1.0.0) into your solution.
- Restart Visual Studio to see that it now reports errors on methods with technical debt.
- Run the tool AttributeUpdater.exe which is included in the NuGet package on your solution, to add the attributes ```MethodHasDebt``` and ```TypeHasDebt``` to your code, which records where technical debt is already present.
- Program away while DebtRatchet makes sure you don't increase your debt.
- Once you've reduced technical debt in your code a bit, run AttributeUpdater.exe on your solution again. This will tighten the parameters on the debt attributes, so that you can't regress into more technical debt again. This tightening is where DebtRatchet gets its name from.
- That's it!

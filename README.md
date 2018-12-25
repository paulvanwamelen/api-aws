# Abstract Play API Server

This AWS serverless stack contains the core API server. It's the only public API.  

## Contact

The [main website](https://www.abstractplay.com) houses the development blog and wiki.

## Change log

25 Dec 2018:

* Had to give up on the standalone games/AI server structure for now. [See development blog for details.](https://www.abstractplay.com)
  * `games-aws` repository has been archived.
  * Game code and tests have been moved into the API.
  * Games are now created synchronously by the GraphQL mutator.
* Games are now created properly when challenges are filled.
* You can now make moves in games, and games end and archive correctly. This includes resigning.
* There will be no "Undo" feature. Instead, you can now preview your move before committing to it.
* In-game chat is working.
* The in-game console is functioning. You can freeze/thaw the clocks and agree to end a game in a draw.
* Players can rank the games relative to each other. This will lead to a global rating of the various games. ([I wrote and published the ranking code.](https://github.com/Perlkonig/Condorcet))
* Players can add tags to games.
* Players can send direct messages to each other.

28 Aug 2018:

* Upgraded to NetCore 2.1 and language version 7.1.
* Found a workaround for EFCore issue!!

29 Jul 2018:

* Moved to a [GraphQL](https://graphql.org/) structure. This provides many benefits, including reduced latency and network transfer volume.
* There is a public endpoint and an authenticated endpoint.
* Many types are now exposed, including user data, games data and metadata, and challenges.
* Slowly adding the mutators. (You can now create and edit profiles and issue challenges.)
* There are also some backend functions now working, too (e.g., game pinging and status updates).
* Currently blocked on known issue with latest version of Entity Framework Core ([issue #12749](https://github.com/aspnet/EntityFrameworkCore/issues/12749)). Will have to continue with other work until it gets resolved or I find a workaround.

10 Jun 2018:

* Figured out my database issues. Profiles can now be correctly fetched.  

03 Jun 2018:

* Moved to [Serverless framework](https://serverless.com) to manage deployment. The system within Visual Studio wouldn't let me configure API gateway in the way I needed to.
* Profile creation is working as expected.
* Profile fetching is currently broken for reasons I haven't had time to ascertain yet.

22 May 2018:

* AWS database up and running.
* The lambdas now talk to the database. Very exciting!
* Profile creation is pretty much working (in the test environment, at least).

05 May 2018:

* Initial commit of the new code. Nothing here yet.

## Deploy

* Make sure you have `dotnet` and `serverless` installed.
* Clone the repo.
* Run `npm install` to install the plugins.
* Configure `serverless` with your own credentials.
* Create the `apsecrets.yml` file with the entries you see in `serverless.yml` or enter your information directly.
* Run `serverless deploy`.

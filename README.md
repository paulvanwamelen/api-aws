# Abstract Play API Server

This AWS serverless stack contains the core API server. It's the only public API.  

## Contact

The [main website](https://www.abstractplay.com) houses the development blog and wiki.

## Change log

XX XXX XXXX:

* Only the GraphQL endpoints are now behind the VPC, reducing latency for all the others. But it means that most database writes will be asynchronous.
* The query and mutator endpoints are fully functioning now.
* API Gateway doesn't support optional authorization. The headers are either required or not even processed. So for now I'm creating a separate endpoint for authenticated queries, thus allowing "me"-style types.
* Continuing to expand the data available through GraphQL.
* Mutations are working!!
* Refactored GraphQL code.
* Games can now be pinged and metadata updated.
* Game status now updates during pings.
* You can now issue challenges!

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

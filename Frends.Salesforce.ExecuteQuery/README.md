# Frends.Salesforce.ExecuteQuery
FRENDS Task for executing a query to Salesforce

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.Salesforce/actions/workflows/ExecuteQuery_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.Salesforce/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.Salesforce.ExecuteQuery)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.Salesforce/Frends.Salesforce.ExecuteQuery|main)

- [Installing](#installing)
- [Task](#task)
     - [ExecuteQuery](#ExecuteQuery)
- [Building](#building)
- [License](#license)
- [Contributing](#contributing)

# Installing

You can install the task via FRENDS UI Task View or you can find the nuget package from the following nuget feed 'Insert nuget feed here'.

# Task

## ExecuteQuery
Execute query to Salesforce.

### Properties

| Property | Type     | Description                                                                            | Example                             |
|----------|----------|----------------------------------------------------------------------------------------|-------------------------------------|
| Domain   | `string` | Salesforce domain. No need to add the query endpoint, since it is added automatically. | 'https://example.my.salesforce.com' |
| Query    | `string` | Query which will be executed.                                                          | 'Select Name From Customer'         |

### Options

| Property             | Type                                  | Description                                                                                                                | Example               |
|----------------------|---------------------------------------|----------------------------------------------------------------------------------------------------------------------------|-----------------------|
| AuthenticationMethod | enum(AccessToken, OAuth2WithPassword) | Method for authentication.                                                                                                 | AccessToken           |
| AccessToken          | `string`                              | OAuth2 access token (only needed for AccessToken authentication).                                                          | 'BFGEGBERBGÖBABGESRB' |
| ClientID             | `string`                              | OAuth2 Client ID (only needed for OAuth2WithPassword authentication).                                                      | 'bgjlekbglejrbgbal'   |
| ClientSecret         | `string`                              | OAuth2 Client Secret (only needed for OAuth2WithPassword authentication).                                                  | 'gbuigiurgiw'         |
| Username             | `string`                              | Salesforce users username (only needed for OAuth2WithPassword authentication).                                             | 'test@test.fi'        |
| Password             | `string`                              | Salesforce users password (only needed for OAuth2WithPassword authentication).                                             | 'verysecretpassword'  |
| SecurityToken        | `string`                              | Salesforce users security token (only needed for OAuth2WithPassword authentication).                                       | 'bguihgeuirghuiewq'   |
| ReturnAccessToken    | `bool`                                | Option to return OAuth2 access token in tasks results for further use (only needed for OAuth2WithPassword authentication). | true                  |

### Returns

Task returns an object with following properties

| Property            | Type      | Description                                               | Example            |
|---------------------|-----------|-----------------------------------------------------------|--------------------|
| Body                | `object`  | Body of the query response.                               |                    |
| RequestIsSuccessful | `bool`    | Indicates if query request was successful.                | true               |
| ErrorException      | Exception | Exception-object if request threw an exception.           |                    |
| ErrorMessage        | `string`  | Error message if request threw an exception.              |                    |
| Token               | `string`  | OAuth2 access token (only if ReturnAccessToken was true). | 'gneogneogeohoews' |

# Building

Clone a copy of the repo.

`git clone https://github.com/FrendsPlatform/Frends.Salesforce`

Go to the task directory.

`cd Frends.Salesforce/Frends.Salesforce.ExecuteQuery`

Build the solution.

`dotnet build`

Run tests.

`dotnet test`

Create a nuget package.

`dotnet pack --configuration Release`

# License

This project is licensed under the MIT License - see the LICENSE file for details.

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

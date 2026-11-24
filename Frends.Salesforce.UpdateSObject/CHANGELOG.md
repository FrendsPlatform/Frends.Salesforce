# Changelog

## [4.0.0] - 2026-11-19

### Changed

- Task now targets .NET 8.
- **[Breaking Change]** Added new `Options` parameter with `ThrowErrorOnFailure` and `ErrorMessageOnFailure`. When `ThrowErrorOnFailure` is false, errors are returned in the result instead of being thrown.
- **[Breaking Change]** Replaced result properties `RequestIsSuccessful`, `ErrorException` and `ErrorMessage` with `Success` and `Error` (`Message`, `AdditionalInfo`).
- **[Breaking Change]** An unsuccessful response from Salesforce is treated as a failure and handled according to the `Options` settings.
- **[Breaking Change]** Errors are no longer wrapped in a generic `Exception`; the original exception is thrown unless `ErrorMessageOnFailure` is set.
- **[Breaking Change]** Error messages now include the response body returned by Salesforce.

## [3.0.0] - 2026-04-01

### Changed

- [Breaking Change] Move parameters related to Connection to new Connection parameter.
- Remove the empty Options parameter.
- Improve validation of input parameters.

### Added

- Added a new authentication method for authentication by OAuth2 with client credentials

## [2.2.0] - 2025-12-12

### Changed

- Change throwing exception logic

## [2.1.0] - 2025-03-18

### Changed

- Updated packages:
  Newtonsoft.Json 13.0.1 -> 13.0.3
  RestSharp 107.3.0 -> 112.1.0
  System.ComponentModel.Annotations 4.7.0 -> 5.0.0
  coverlet.collector 3.1.0 -> 6.0.4
  dotenv.net 3.2.0 -> 3.2.1
  Microsoft.NET.Test.Sdk 16.7.0 -> 17.13.0
  MSTest.TestAdapter 2.1.2 -> 3.8.3
  MSTest.TestFramework 2.1.2 -> 3.8.3
  nunit 3.12.0 -> 4.3.2
  NUnit3TestAdapter 3.17.0 -> 5.0.0

## [2.0.0] - 2024-08-19

### Added

- Salesforce API version number can now be specified in input.

## [1.1.0] - 2024-08-28

### Changed

- Documentation link now links to a correct page in Frends Tasks.

## [1.0.0] - 2022-05-11

### Added

- Initial implementation of Frends.Salesforce.UpdateSObject

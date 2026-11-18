# Changelog

## [1.2.0] - 2026-11-12

### Changed

- Fields missing from the JSON payload now fall back to the schema default value, or to `null` when the field is nullable, instead of always failing.
- When required fields are missing (no value, no default and not nullable), the error now lists all of them together with the schema used for validation.

## [1.1.0] - 2026-09-30

### Changed

- Updated the task package copyright information.

## [1.0.0] - 2026-04-26

### Added

- Initial implementation

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [5.0.0] - 2026-02-28

### Added

- Added fused model baking utilities and workflow scripts for generating runtime model assets.

### Changed

- Replaced ONNX model assets with fused `.sentis` model files.
- Moved pre/postprocess steps into the fused model graph.
- Updated `jp.keijiro.klak.nnutils` dependency version.
- Updated README documentation.

## [4.0.0] - 2025-05-29

### Changed

- Migrated the backend from Sentis to Unity Inference Engine.
- Updated package dependencies to match the new inference backend.

## [3.0.0] - 2024-09-28

### Changed

- Updated the package to support Unity Sentis.
- Migrated runtime implementation from Barracuda to Sentis.
- Replaced Barracuda-related naming/text across the package.
- Updated the project to Unity 6.

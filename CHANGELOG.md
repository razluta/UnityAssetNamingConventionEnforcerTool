# Changelog

All notable changes to the Unity Asset Naming Convention Enforcer Tool will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-05-31

### Added
- **Initial release** of Unity Asset Naming Convention Enforcer Tool
- **Wizard-based interface** for step-by-step asset renaming process
- **Game Category selection** with predefined categories:
  - CHAR (Characters)
  - WEAP (Weapons)
  - ENV (Environment assets)
  - PROP (Props)
  - GUI (UI elements)
  - AUDIO (Sound effects and music)
  - VFX (Visual effects)
- **Asset Type detection** with automatic inference from file extensions:
  - Prefab, Model, SkinnedModel, Animation, Animator
  - Texture, Font, Sound, Music, Curve, Data
  - Preset, Template, Atlas, Light, AI, Level, Loc
- **Intelligent name processing** that automatically detects and categorizes:
  - Variants (01, 02, a, b, alt)
  - States (idle, active, normal, broken)
  - Size/Quality markers (1K, 2K, low, high, ultra)
- **Real-time name validation** to prevent invalid asset names
- **Name breakdown visualization** showing how the final name is constructed
- **Multi-asset processing** with individual wizard flow for each asset
- **UIToolkit-based interface** with modern, responsive design
- **Menu integration** accessible via `Tools/Razluta/Unity Asset Naming Convention Enforcer Tool`
- **Standardized naming convention**: `[GameCategory]_[AssetType]_[AssetName]_[Variant]_[State]_[Size/Quality]`

### Features
- Automatic removal of existing category/type prefixes to avoid duplication
- Support for camelCase, space-separated, and underscore-separated original names
- Real-time preview of final asset name with editable text field
- Progress tracking for multi-asset operations
- Skip functionality for assets user doesn't want to rename
- Comprehensive validation preventing invalid characters and overly long names

### Technical Details
- **Unity Version**: Requires Unity 2022.3 or higher
- **Package ID**: `com.razluta.unity-asset-naming-convention-enforcer-tool`
- **Assembly Definition**: Editor-only with proper namespace isolation
- **UI Framework**: Built with Unity UIToolkit for modern editor integration
- **Installation**: Available via Unity Package Manager using Git URL

### Documentation
- Complete setup and usage instructions
- Step-by-step installation guide
- Comprehensive examples and naming convention documentation
- GitHub repository with full source code and documentation
# Unity Asset Naming Convention Enforcer Tool

A Unity tool for enforcing consistent asset naming conventions through an automated wizard interface.

## Installation

### Via Git URL (Recommended)
1. Open Unity Package Manager
2. Click "+" → "Add package from git URL"
3. Enter: `https://github.com/razluta/UnityAssetNamingConventionEnforcerTool.git`

### Via Local Package
1. Clone this repository
2. In Unity Package Manager, click "+" → "Add package from disk"
3. Select the `package.json` file from the cloned repository

## Usage

1. Select one or more assets in the Project window
2. Open `Tools/Razluta/Unity Asset Naming Convention Enforcer Tool`
3. Follow the wizard steps:
    - Choose Game Category (CHAR, WEAP, ENV, PROP, GUI, AUDIO, VFX)
    - Choose Asset Type (Prefab, Model, Texture, etc.)
    - Review and edit the generated name
    - Apply the rename

## Naming Convention

Assets follow this format: `[GameCategory]_[AssetType]_[AssetName]_[Variant]_[State]_[Size/Quality]`

### Examples
- `CHAR_SkinnedModel_Knight_Male_Body_01`
- `WEAP_Model_Sword_Steel_Sharp_High`
- `ENV_Texture_Ground_Grass_Dry_2K`
- `GUI_Prefab_Button_Main_Normal_Large`

## Requirements

- Unity 2022.3 or higher
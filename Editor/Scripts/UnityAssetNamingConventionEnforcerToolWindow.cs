using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Razluta.UnityAssetNamingConventionEnforcerTool.Editor
{
    public class UnityAssetNamingConventionEnforcerToolWindow : EditorWindow
    {
        private VisualElement m_RootContainer;
        private VisualElement m_NoSelectionContainer;
        private VisualElement m_WizardContainer;
        private VisualElement m_BatchControls;
        
        // Wizard steps
        private VisualElement m_CategoryStep;
        private VisualElement m_AssetTypeStep;
        private VisualElement m_FinalNameStep;
        
        // Current processing data
        private List<string> m_SelectedAssetPaths;
        private int m_CurrentAssetIndex;
        private AssetNamingData m_CurrentAssetData;
        private BatchProcessingSettings m_BatchSettings;
        
        // UI Elements
        private Label m_CurrentAssetNameLabel;
        private Label m_CurrentAssetPathLabel;
        private Label m_AssetProgressLabel;
        private VisualElement m_AssetPreview;
        private EnumField m_CategoryField;
        private EnumField m_AssetTypeField;
        private TextField m_FinalNameField;
        private Label m_ValidationMessage;
        private VisualElement m_BreakdownContainer;

        [MenuItem("Tools/Razluta/Unity Asset Naming Convention Enforcer Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityAssetNamingConventionEnforcerToolWindow>();
            window.titleContent = new GUIContent("Unity Asset Naming Convention Enforcer Tool");
            window.minSize = new Vector2(400, 600);
        }

        public void CreateGUI()
        {
            // Load UXML
            var visualTree = Resources.Load<VisualTreeAsset>("UnityAssetNamingConventionEnforcerToolWindow");
            if (visualTree != null)
            {
                m_RootContainer = visualTree.Instantiate();
                rootVisualElement.Add(m_RootContainer);
            }
            else
            {
                Debug.LogError("Could not load UnityAssetNamingConventionEnforcerToolWindow.uxml");
                return;
            }

            // Cache UI elements
            CacheUIElements();
            
            // Setup event handlers
            SetupEventHandlers();
            
            // Initialize selection tracking
            Selection.selectionChanged += OnSelectionChanged;
            
            // Initial update
            UpdateUI();
        }

        private void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void CacheUIElements()
        {
            m_NoSelectionContainer = m_RootContainer.Q<VisualElement>("no-selection-container");
            m_WizardContainer = m_RootContainer.Q<VisualElement>("wizard-container");
            m_BatchControls = m_RootContainer.Q<VisualElement>("batch-controls");
            
            m_CategoryStep = m_RootContainer.Q<VisualElement>("category-step");
            m_AssetTypeStep = m_RootContainer.Q<VisualElement>("asset-type-step");
            m_FinalNameStep = m_RootContainer.Q<VisualElement>("final-name-step");
            
            m_CurrentAssetNameLabel = m_RootContainer.Q<Label>("current-asset-name");
            m_CurrentAssetPathLabel = m_RootContainer.Q<Label>("current-asset-path");
            m_AssetProgressLabel = m_RootContainer.Q<Label>("asset-progress");
            m_AssetPreview = m_RootContainer.Q<VisualElement>("asset-preview");
            
            m_CategoryField = m_RootContainer.Q<EnumField>("category-field");
            m_AssetTypeField = m_RootContainer.Q<EnumField>("asset-type-field");
            m_FinalNameField = m_RootContainer.Q<TextField>("final-name-field");
            m_ValidationMessage = m_RootContainer.Q<Label>("validation-message");
            m_BreakdownContainer = m_RootContainer.Q<VisualElement>("breakdown-container");
        }

        private void SetupEventHandlers()
        {
            // Step navigation buttons
            m_RootContainer.Q<Button>("category-next-btn").clicked += OnCategoryNext;
            m_RootContainer.Q<Button>("asset-type-back-btn").clicked += OnAssetTypeBack;
            m_RootContainer.Q<Button>("asset-type-next-btn").clicked += OnAssetTypeNext;
            m_RootContainer.Q<Button>("final-back-btn").clicked += OnFinalBack;
            m_RootContainer.Q<Button>("apply-btn").clicked += OnApplyRename;
            m_RootContainer.Q<Button>("skip-btn").clicked += OnSkipAsset;
            m_RootContainer.Q<Button>("reset-asset-btn").clicked += OnResetCurrentAsset;
            
            // Batch controls
            m_RootContainer.Q<Button>("process-all-btn").clicked += OnProcessAll;
            m_RootContainer.Q<Button>("reset-btn").clicked += OnReset;
            
            // Field change handlers
            m_CategoryField.RegisterValueChangedCallback(OnCategoryChanged);
            m_AssetTypeField.RegisterValueChangedCallback(OnAssetTypeChanged);
            m_FinalNameField.RegisterValueChangedCallback(OnFinalNameChanged);
        }

        private void OnSelectionChanged()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            var selectedAssets = Selection.assetGUIDs;
            
            if (selectedAssets == null || selectedAssets.Length == 0)
            {
                ShowNoSelection();
            }
            else
            {
                m_SelectedAssetPaths = selectedAssets
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => !AssetDatabase.IsValidFolder(path))
                    .ToList();
                
                if (m_SelectedAssetPaths.Count == 0)
                {
                    ShowNoSelection();
                }
                else
                {
                    StartWizard();
                }
            }
        }

        private void ShowNoSelection()
        {
            m_NoSelectionContainer.style.display = DisplayStyle.Flex;
            m_WizardContainer.style.display = DisplayStyle.None;
            m_BatchControls.style.display = DisplayStyle.None;
        }

        private void StartWizard()
        {
            m_CurrentAssetIndex = 0;
            m_BatchSettings = new BatchProcessingSettings();
            ProcessCurrentAsset();
            
            m_NoSelectionContainer.style.display = DisplayStyle.None;
            m_WizardContainer.style.display = DisplayStyle.Flex;
            m_BatchControls.style.display = DisplayStyle.Flex;
            
            // For multiple assets, skip to final step if batch settings are already configured
            if (m_SelectedAssetPaths.Count > 1 && m_BatchSettings.isSet)
            {
                ApplyBatchSettings();
                ShowStep(2);
            }
            else
            {
                ShowStep(0); // Start with category selection for single asset or first asset in batch
            }
        }

        private void ProcessCurrentAsset()
        {
            if (m_CurrentAssetIndex >= m_SelectedAssetPaths.Count)
            {
                OnAllAssetsProcessed();
                return;
            }
            
            string assetPath = m_SelectedAssetPaths[m_CurrentAssetIndex];
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            
            m_CurrentAssetData = new AssetNamingData(assetName, assetPath);
            
            // Try to infer asset type from file extension
            var inferredAssetType = NamingConventionProcessor.InferAssetTypeFromPath(assetPath);
            m_AssetTypeField.value = inferredAssetType;
            
            UpdateCurrentAssetInfo();
        }

        private void UpdateCurrentAssetInfo()
        {
            m_CurrentAssetNameLabel.text = $"Original name: {m_CurrentAssetData.originalName}";
            m_CurrentAssetPathLabel.text = $"Current path: {m_CurrentAssetData.originalPath}";
            m_AssetProgressLabel.text = $"Asset {m_CurrentAssetIndex + 1} of {m_SelectedAssetPaths.Count}";
            
            UpdateAssetPreview();
        }

        private void UpdateAssetPreview()
        {
            // Clear existing preview
            m_AssetPreview.Clear();
            
            // Load the asset
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(m_CurrentAssetData.originalPath);
            if (asset == null) return;
            
            // Create preview based on asset type
            Texture2D previewTexture = null;
            
            // Try to get asset preview
            previewTexture = AssetPreview.GetAssetPreview(asset);
            
            // If no preview available, try to get mini thumbnail
            if (previewTexture == null)
            {
                previewTexture = AssetPreview.GetMiniThumbnail(asset);
            }
            
            // If we have a preview texture, create an Image element
            if (previewTexture != null)
            {
                var previewImage = new UnityEngine.UIElements.Image();
                previewImage.image = previewTexture;
                previewImage.scaleMode = ScaleMode.ScaleToFit;
                previewImage.style.width = Length.Percent(100);
                previewImage.style.height = Length.Percent(100);
                m_AssetPreview.Add(previewImage);
            }
            else
            {
                // If no preview available, show asset type icon or placeholder
                var icon = AssetDatabase.GetCachedIcon(m_CurrentAssetData.originalPath);
                if (icon != null)
                {
                    var iconImage = new UnityEngine.UIElements.Image();
                    iconImage.image = icon;
                    iconImage.scaleMode = ScaleMode.ScaleToFit;
                    iconImage.style.width = Length.Percent(100);
                    iconImage.style.height = Length.Percent(100);
                    m_AssetPreview.Add(iconImage);
                }
                else
                {
                    // Show placeholder text
                    var placeholderLabel = new Label("No Preview");
                    placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    placeholderLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    placeholderLabel.style.fontSize = 14;
                    placeholderLabel.style.height = Length.Percent(100);
                    m_AssetPreview.Add(placeholderLabel);
                }
            }
        }

        private void ShowStep(int stepIndex)
        {
            m_CategoryStep.style.display = stepIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            m_AssetTypeStep.style.display = stepIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            m_FinalNameStep.style.display = stepIndex == 2 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnCategoryNext()
        {
            m_CurrentAssetData.gameCategory = (GameCategory)m_CategoryField.value;
            ShowStep(1);
        }

        private void OnAssetTypeBack()
        {
            ShowStep(0);
        }

        private void OnAssetTypeNext()
        {
            m_CurrentAssetData.assetType = (AssetType)m_AssetTypeField.value;
            
            // Store batch settings if processing multiple assets
            if (m_SelectedAssetPaths.Count > 1)
            {
                m_BatchSettings.gameCategory = m_CurrentAssetData.gameCategory;
                m_BatchSettings.assetType = m_CurrentAssetData.assetType;
                m_BatchSettings.isSet = true;
            }
            
            GenerateNamePreview();
            ShowStep(2);
        }

        private void OnFinalBack()
        {
            // Only allow going back if we're not in batch mode or if batch settings aren't set
            if (m_SelectedAssetPaths.Count == 1 || !m_BatchSettings.isSet)
            {
                ShowStep(1);
            }
        }

        private void OnResetCurrentAsset()
        {
            // Reset this specific asset to go through the full wizard
            if (m_SelectedAssetPaths.Count > 1)
            {
                // Temporarily disable batch settings for this asset
                var tempBatchSettings = m_BatchSettings.isSet;
                m_BatchSettings.isSet = false;
                
                ShowStep(0);
                
                // Don't restore batch settings - let user go through the process
            }
            else
            {
                ShowStep(0);
            }
        }

        private void OnCategoryChanged(ChangeEvent<System.Enum> evt)
        {
            // Update preview if we're past this step
            if (m_FinalNameStep.style.display == DisplayStyle.Flex)
            {
                m_CurrentAssetData.gameCategory = (GameCategory)evt.newValue;
                GenerateNamePreview();
            }
        }

        private void OnAssetTypeChanged(ChangeEvent<System.Enum> evt)
        {
            // Update preview if we're past this step
            if (m_FinalNameStep.style.display == DisplayStyle.Flex)
            {
                m_CurrentAssetData.assetType = (AssetType)evt.newValue;
                GenerateNamePreview();
            }
        }

        private void OnFinalNameChanged(ChangeEvent<string> evt)
        {
            ValidateFinalName(evt.newValue);
        }

        private void GenerateNamePreview()
        {
            m_CurrentAssetData = NamingConventionProcessor.ProcessAssetName(
                m_CurrentAssetData.originalName,
                m_CurrentAssetData.originalPath,
                m_CurrentAssetData.gameCategory,
                m_CurrentAssetData.assetType
            );
            
            m_FinalNameField.value = m_CurrentAssetData.finalName;
            UpdateNameBreakdown();
            ValidateFinalName(m_CurrentAssetData.finalName);
        }

        private void UpdateNameBreakdown()
        {
            m_BreakdownContainer.Clear();
            
            AddBreakdownPart("Game Category", m_CurrentAssetData.gameCategory.ToString());
            AddBreakdownPart("Asset Type", m_CurrentAssetData.assetType.ToString());
            
            if (!string.IsNullOrEmpty(m_CurrentAssetData.assetName))
                AddBreakdownPart("Asset Name", m_CurrentAssetData.assetName);
            
            if (!string.IsNullOrEmpty(m_CurrentAssetData.variant))
                AddBreakdownPart("Variant", m_CurrentAssetData.variant);
            
            if (!string.IsNullOrEmpty(m_CurrentAssetData.state))
                AddBreakdownPart("State", m_CurrentAssetData.state);
            
            if (!string.IsNullOrEmpty(m_CurrentAssetData.sizeQuality))
                AddBreakdownPart("Size/Quality", m_CurrentAssetData.sizeQuality);
        }

        private void AddBreakdownPart(string label, string value)
        {
            var partLabel = new Label($"{label}: {value}");
            partLabel.AddToClassList("breakdown-part");
            m_BreakdownContainer.Add(partLabel);
        }

        private void ValidateFinalName(string name)
        {
            string validationError = NamingConventionProcessor.ValidateAssetName(name);
            
            if (string.IsNullOrEmpty(validationError))
            {
                m_ValidationMessage.style.display = DisplayStyle.None;
                m_RootContainer.Q<Button>("apply-btn").SetEnabled(true);
            }
            else
            {
                m_ValidationMessage.text = validationError;
                m_ValidationMessage.style.display = DisplayStyle.Flex;
                m_RootContainer.Q<Button>("apply-btn").SetEnabled(false);
            }
        }

        private void OnApplyRename()
        {
            string newName = m_FinalNameField.value;
            string oldPath = m_CurrentAssetData.originalPath;
            string directory = Path.GetDirectoryName(oldPath);
            string extension = Path.GetExtension(oldPath);
            string newPath = Path.Combine(directory, newName + extension);
            
            string error = AssetDatabase.RenameAsset(oldPath, newName);
            
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log($"Successfully renamed '{m_CurrentAssetData.originalName}' to '{newName}'");
                MoveToNextAsset();
            }
            else
            {
                EditorUtility.DisplayDialog("Rename Failed", $"Failed to rename asset: {error}", "OK");
            }
        }

        private void OnSkipAsset()
        {
            Debug.Log($"Skipped renaming '{m_CurrentAssetData.originalName}'");
            MoveToNextAsset();
        }

        private void MoveToNextAsset()
        {
            m_CurrentAssetIndex++;
            
            if (m_CurrentAssetIndex < m_SelectedAssetPaths.Count)
            {
                ProcessCurrentAsset();
                
                // For multiple assets with batch settings, go directly to final step
                if (m_SelectedAssetPaths.Count > 1 && m_BatchSettings.isSet)
                {
                    ApplyBatchSettings();
                    ShowStep(2);
                }
                else
                {
                    ShowStep(0); // Reset to first step for next asset
                }
            }
            else
            {
                OnAllAssetsProcessed();
            }
        }

        private void ApplyBatchSettings()
        {
            if (m_BatchSettings.isSet)
            {
                m_CurrentAssetData.gameCategory = m_BatchSettings.gameCategory;
                m_CurrentAssetData.assetType = m_BatchSettings.assetType;
                
                // Update the UI fields to reflect the batch settings
                m_CategoryField.value = m_BatchSettings.gameCategory;
                m_AssetTypeField.value = m_BatchSettings.assetType;
                
                GenerateNamePreview();
            }
        }

        private void OnAllAssetsProcessed()
        {
            EditorUtility.DisplayDialog("Processing Complete", 
                "All selected assets have been processed!", "OK");
            
            ShowNoSelection();
        }

        private void OnProcessAll()
        {
            if (m_SelectedAssetPaths.Count > 1 && !m_BatchSettings.isSet)
            {
                EditorUtility.DisplayDialog("Batch Processing", 
                    "Please set the category and type for the first asset to establish batch settings for all selected assets.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Batch Processing", 
                    "Individual processing ensures each asset gets proper attention. " +
                    "Continue through the wizard for each asset.", "OK");
            }
        }

        private void OnReset()
        {
            m_BatchSettings = new BatchProcessingSettings();
            Selection.activeObject = null;
            UpdateUI();
        }
    }
}
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Seulitools
{
    public class ImportPanel : EditorWindow
    {
        private const string ImportFolderPrefsKey = "Seulitools.ImportFolder";
        private string searchQuery = "";
        private Texture2D image;

        private bool isEditMode = false;

        private Vector2 scrollPosition;

        private readonly Dictionary<string, string> categoryInputs = new Dictionary<string, string>();
        private readonly List<string> categoryKeys = new List<string>();
        private readonly List<string> matchingFiles = new List<string>();

        private static string jsonFilePath;
        public static Dictionary<string, List<string>> fileCategories;

        [MenuItem("SeuliTools/Import Panel", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportPanel>("Import Panel");
            window.maxSize = new Vector2(650, 900);
        }

        private void OnEnable()
        {
            string destinationPath = GetUserDataPath();
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            LoadFileCategories();
            AssignUncategorizedCategory();
            image = EditorGUIUtility.Load("Packages/seulitools/Runtime/Resources/fish.png") as Texture2D;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Import Panel", EditorStyles.boldLabel);
            GUILayout.Label("Manage and import .unitypackage files", EditorStyles.miniLabel);
            if (image != null)
            {
                GUILayout.Space(8f);
                GUILayout.Label(image, GUILayout.Height(180));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Search", GUILayout.Width(50f));
            searchQuery = GUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.MinWidth(160f));
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60f)))
            {
                searchQuery = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80f)))
            {
                LoadFileCategories();
                AssignUncategorizedCategory();
            }
            if (GUILayout.Button(isEditMode ? "Exit Edit" : "Edit Mode", EditorStyles.toolbarButton, GUILayout.Width(85f)))
            {
                isEditMode = !isEditMode;
            }
            if (GUILayout.Button("Import Folder", EditorStyles.toolbarButton, GUILayout.Width(110f)))
            {
                PromptAndImportFromFolder();
            }
            if (GUILayout.Button("Add Package", EditorStyles.toolbarButton, GUILayout.Width(100f)))
            {
                string filePath = EditorUtility.OpenFilePanel("Select Unity Package", "", "unitypackage");
                if (!string.IsNullOrEmpty(filePath))
                {
                    string destinationPath = GetUserDataPath();
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string targetPath = Path.Combine(destinationPath, fileName + ".unitypackage");
                    if (File.Exists(targetPath))
                    {
                        bool overwrite = EditorUtility.DisplayDialog("Overwrite Package", "A package with the same name already exists. Overwrite it?", "Overwrite", "Cancel");
                        if (!overwrite)
                        {
                            return;
                        }
                    }
                    if (TryCopyFile(filePath, targetPath, "Add Package Failed"))
                    {
                        AssignUncategorizedCategory();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);

            if (isEditMode)
            {
                EditorGUILayout.HelpBox("Edit mode lets you move packages between categories or delete them.", MessageType.Info);
            }
            ListFilesByCategories();
        }

        private void PromptAndImportFromFolder()
        {
            string lastFolder = EditorPrefs.GetString(ImportFolderPrefsKey, string.Empty);
            string folder = EditorUtility.OpenFolderPanel("Select Package Folder", lastFolder, string.Empty);
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            EditorPrefs.SetString(ImportFolderPrefsKey, folder);
            CopyPackagesFromFolder(folder);
        }

        private void CopyPackagesFromFolder(string sourceFolderPath)
        {
            if (!Directory.Exists(sourceFolderPath))
            {
                EditorUtility.DisplayDialog("Import Folder", "Folder not found:\n" + sourceFolderPath, "OK");
                return;
            }

            string destinationPath = GetUserDataPath();
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string[] files = Directory.GetFiles(sourceFolderPath, "*.unitypackage");
            if (files.Length == 0)
            {
                EditorUtility.DisplayDialog("Import Folder", "No .unitypackage files found in the selected folder.", "OK");
                return;
            }

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(destinationPath, fileName);

                if (File.Exists(destinationFilePath))
                {
                    bool overwrite = EditorUtility.DisplayDialog(
                        "Overwrite Package",
                        "A package with the same name already exists. Overwrite it?\n\n" + fileName,
                        "Overwrite",
                        "Skip"
                    );

                    if (!overwrite)
                    {
                        continue;
                    }
                }

                TryCopyFile(filePath, destinationFilePath, "Import Package Failed");
            }

            AssignUncategorizedCategory();
            AssetDatabase.Refresh();
        }

        private void ListFilesByCategories()
        {
            int fileAmount = 0;
            GUILayout.Space(10);

            string destinationPath = GetUserDataPath();

            if (fileCategories != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                categoryKeys.Clear();
                foreach (var key in fileCategories.Keys)
                {
                    categoryKeys.Add(key);
                }

                string query = searchQuery;
                bool hasQuery = !string.IsNullOrEmpty(query);

                foreach (var categoryName in categoryKeys)
                {
                    if (!fileCategories.TryGetValue(categoryName, out List<string> files))
                    {
                        continue;
                    }

                    matchingFiles.Clear();
                    for (int i = 0; i < files.Count; i++)
                    {
                        string filePath = files[i];
                        if (!hasQuery || filePath.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matchingFiles.Add(filePath);
                        }
                    }

                    if (matchingFiles.Count == 0)
                        continue;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(categoryName, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(matchingFiles.Count.ToString(), EditorStyles.miniLabel, GUILayout.Width(32f));
                    EditorGUILayout.EndHorizontal();

                    bool isDark = false;
                    foreach (var filePath in matchingFiles)
                    {
                        fileAmount++;

                        if (isEditMode)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(filePath, EditorStyles.label);

                            string currentValue = categoryInputs.TryGetValue(filePath, out string value) ? value : "";
                            string newValue = EditorGUILayout.TextField(currentValue);
                            if (!string.Equals(currentValue, newValue, StringComparison.Ordinal))
                            {
                                categoryInputs[filePath] = newValue;
                            }
                            if (GUILayout.Button("Save", GUILayout.Width(70f)))
                            {
                                MoveFileToCategory(filePath, categoryName, newValue);
                            }

                            if (GUILayout.Button("Delete", GUILayout.Width(60f)))
                            {
                                bool confirm = EditorUtility.DisplayDialog("Delete Package", "Delete this package from the list?", "Delete", "Cancel");
                                if (confirm)
                                {
                                    string fullPath = Path.Combine(destinationPath, filePath);
                                    if (File.Exists(fullPath))
                                    {
                                        TryDeleteFile(fullPath, "Delete Package Failed");
                                    }
                                    LoadFileCategories();
                                    AssignUncategorizedCategory();
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            string fileName = Path.GetFileNameWithoutExtension(filePath);
                            if (GUILayout.Button(fileName))
                            {
                                string fullPath = Path.Combine(destinationPath, filePath);
                                if (File.Exists(fullPath))
                                {
                                    AssetDatabase.ImportPackage(fullPath, true);
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog("File Missing", "The package file could not be found.", "OK");
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            isDark = !isDark;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();
            }
            if (fileAmount == 0)
            {
                EditorGUILayout.HelpBox("No packages found. Add a package or refresh.", MessageType.Info);
            }
            GUILayout.Label("Fetched Files: " + fileAmount, EditorStyles.miniLabel);
        }

        private void LoadFileCategories()
        {
            string destinationPath = GetUserDataPath();
            jsonFilePath = Path.Combine(destinationPath, "categories.json");

            categoryInputs.Clear();

            if (!string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath))
            {
                try
                {
                    string json = File.ReadAllText(jsonFilePath);
                    fileCategories = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                }
                catch
                {
                    fileCategories = new Dictionary<string, List<string>>();
                }

                if (fileCategories == null)
                {
                    fileCategories = new Dictionary<string, List<string>>();
                }

                var categoriesToRemove = new List<string>();

                foreach (var category in fileCategories)
                {
                    var filesToRemove = new List<string>();

                    foreach (var filePath in category.Value)
                    {
                        string fullPath = Path.Combine(destinationPath, filePath);

                        if (!File.Exists(fullPath))
                        {
                            filesToRemove.Add(filePath);
                        }
                    }

                    foreach (var filePath in filesToRemove)
                    {
                        category.Value.Remove(filePath);
                    }

                    if (category.Value.Count == 0)
                    {
                        categoriesToRemove.Add(category.Key);
                    }
                }

                foreach (var categoryToRemove in categoriesToRemove)
                {
                    fileCategories.Remove(categoryToRemove);
                }

                SaveCategoriesToJson();
            }
            else
            {
                fileCategories = new Dictionary<string, List<string>>();
            }
        }

        private void AssignUncategorizedCategory()
        {
            string destinationPath = GetUserDataPath();
            if (!Directory.Exists(destinationPath))
            {
                return;
            }

            if (fileCategories == null)
            {
                fileCategories = new Dictionary<string, List<string>>();
            }

            if (!fileCategories.ContainsKey("Uncategorized"))
            {
                fileCategories["Uncategorized"] = new List<string>();
            }

            string[] unityPackages = Directory.GetFiles(destinationPath, "*.unitypackage");

            HashSet<string> assignedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var category in fileCategories)
            {
                foreach (var filePath in category.Value)
                {
                    assignedFiles.Add(filePath);
                }
            }

            foreach (string packagePath in unityPackages)
            {
                string fileName = Path.GetFileName(packagePath);

                if (!assignedFiles.Contains(fileName))
                {
                    fileCategories["Uncategorized"].Add(fileName);
                }
            }

            SaveCategoriesToJson();
        }

        private void MoveFileToCategory(string filePath, string currentCategory, string newCategory)
        {
            if (string.IsNullOrWhiteSpace(newCategory))
            {
                EditorUtility.DisplayDialog("Invalid Category", "Category name cannot be empty.", "OK");
                return;
            }

            newCategory = newCategory.Trim();

            if (fileCategories.ContainsKey(currentCategory))
            {
                fileCategories[currentCategory].Remove(filePath);

                if (fileCategories[currentCategory].Count == 0)
                {
                    fileCategories.Remove(currentCategory);
                }
            }

            if (!fileCategories.ContainsKey(newCategory))
            {
                fileCategories[newCategory] = new List<string>();
            }
            if (!fileCategories[newCategory].Contains(filePath))
            {
                fileCategories[newCategory].Add(filePath);
            }

            SaveCategoriesToJson();
        }

        public static void SaveCategoriesToJson()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                string destinationPath = GetUserDataPath();
                jsonFilePath = Path.Combine(destinationPath, "categories.json");
            }
            string json = JsonConvert.SerializeObject(fileCategories, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
        }

        private static string GetUserDataPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "Seulitools");
        }

        private static bool TryCopyFile(string sourcePath, string destinationPath, string title)
        {
            try
            {
                File.Copy(sourcePath, destinationPath, true);
                return true;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(title, "File operation failed:\n" + ex.Message, "OK");
                return false;
            }
        }

        private static bool TryDeleteFile(string filePath, string title)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(title, "File operation failed:\n" + ex.Message, "OK");
                return false;
            }
        }

    }
}
#endif
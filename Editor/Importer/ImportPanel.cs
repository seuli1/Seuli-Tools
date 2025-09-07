#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace Seulitools
{
    public class ImportPanel : EditorWindow
    {
        private string searchQuery = "";
        private Texture2D image;

        private bool isEditMode = false;

        private Vector2 scrollPosition;

        private static string jsonFilePath;
        public static Dictionary<string, List<string>> fileCategories;

        [MenuItem("SeuliTools/Import Panel")]
        public static void ShowWindow()
        {
            var window = GetWindow<ImportPanel>("Import Panel");
            window.maxSize = new Vector2(650, 900);
        }

        private void OnEnable()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string destinationPath = Path.Combine(appDataPath, "Seulitools");
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
            MoveFilesToFolder(Path.Combine(Directory.GetCurrentDirectory(), "Packages/seulitools/Editor/Importer/Imports"), destinationPath);
            LoadFileCategories();
            AssignUncategorizedCategory();
            image = EditorGUIUtility.Load("Packages/seulitools/Runtime/Resources/fish.png") as Texture2D;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(image, GUILayout.Height(255));
            GUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(10);
            searchQuery = GUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (GUILayout.Button("X", EditorStyles.toolbarButton))
            {
                searchQuery = "";
                GUI.FocusControl(null);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LoadFileCategories();
                AssignUncategorizedCategory();
            }
            if (GUILayout.Button(isEditMode ? "Exit Edit Mode" : "Enter Edit Mode", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                isEditMode = !isEditMode;
            }
            if (GUILayout.Button("Add Package", EditorStyles.toolbarButton, GUILayout.Width(90)))
            {
                string filePath = EditorUtility.OpenFilePanel("Select Unity Package", "", "unitypackage");
                if (!string.IsNullOrEmpty(filePath))
                {
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string destinationPath = Path.Combine(appDataPath, "Seulitools");
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    File.Copy(filePath, Path.Combine(destinationPath, fileName + ".unitypackage"), true);
                    AssignUncategorizedCategory();
                }
            }
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            ListFilesByCategories();
        }

        private void MoveFilesToFolder(string sourceFolderPath, string destinationFolderPath)
        {
            if (!Directory.Exists(sourceFolderPath)) return;
            if (!Directory.Exists(destinationFolderPath)) Directory.CreateDirectory(destinationFolderPath);

            string[] files = Directory.GetFiles(sourceFolderPath);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);

                string destinationFilePath = Path.Combine(destinationFolderPath, fileName);

                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                }

                File.Move(filePath, destinationFilePath);
            }

            Directory.Delete(sourceFolderPath, true);
            AssetDatabase.Refresh();
        }


        string[] okok = new string[99999];

        private void ListFilesByCategories()
        {
            int fileAmount = 0;
            GUILayout.Space(10);

            if (fileCategories != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                var categoriesCopy = new Dictionary<string, List<string>>(fileCategories);

                foreach (var category in categoriesCopy)
                {
                    string categoryName = category.Key;

                    var matchingFiles = category.Value
                        .Where(filePath => string.IsNullOrEmpty(searchQuery) || filePath.ToLower().Contains(searchQuery.ToLower()))
                        .ToList();

                    if (matchingFiles.Count == 0)
                        continue;

                    GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f);
                    GUILayout.BeginVertical(GUI.skin.box);

                    EditorGUILayout.LabelField(categoryName, EditorStyles.boldLabel);

                    bool isDark = false;
                    foreach (var filePath in matchingFiles)
                    {
                        fileAmount++;
                        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string destinationPath = Path.Combine(appDataPath, "Seulitools/");

                        if (isEditMode)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(filePath);

                            EditorGUI.BeginChangeCheck();
                            okok[fileAmount] = EditorGUILayout.TextField(okok[fileAmount]);
                            if (GUILayout.Button("Save", GUILayout.Width(80)))
                            {
                                MoveFileToCategory(filePath, categoryName, okok[fileAmount]);
                            }

                            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                            {
                                File.Delete(destinationPath + filePath);
                                LoadFileCategories();
                                AssignUncategorizedCategory();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUI.backgroundColor = isDark ? new Color(0.75f, 0.75f, 0.75f) : Color.white;
                            isDark = !isDark;

                            string fileName = Path.GetFileNameWithoutExtension(filePath);
                            if (GUILayout.Button(fileName))
                            {
                                AssetDatabase.ImportPackage(destinationPath + filePath, true);
                            }
                        }
                    }
                    EditorGUILayout.Space();
                    GUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.Label("Fetched Files: " + fileAmount);
        }

        private void LoadFileCategories()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            jsonFilePath = Path.Combine(appDataPath, "Seulitools/categories.json");
            string destinationPath = Path.Combine(appDataPath, "Seulitools");

            if (!string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                fileCategories = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

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
            }
            else
            {
                fileCategories = new Dictionary<string, List<string>>();
            }
        }

        private void AssignUncategorizedCategory()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string destinationPath = Path.Combine(appDataPath, "Seulitools");

            if (!fileCategories.ContainsKey("Uncategorized"))
            {
                fileCategories["Uncategorized"] = new List<string>();
            }

            string[] unityPackages = Directory.GetFiles(destinationPath, "*.unitypackage");

            foreach (string packagePath in unityPackages)
            {
                string fileName = Path.GetFileName(packagePath);

                bool isAssigned = false;
                foreach (var category in fileCategories)
                {
                    if (category.Value.Contains(fileName))
                    {
                        isAssigned = true;
                        break;
                    }
                }

                if (!isAssigned)
                {
                    fileCategories["Uncategorized"].Add(fileName);
                }
            }

            SaveCategoriesToJson();
        }

        private void MoveFileToCategory(string filePath, string currentCategory, string newCategory)
        {
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
            fileCategories[newCategory].Add(filePath);

            SaveCategoriesToJson();
        }

        public static void SaveCategoriesToJson()
        {
            string json = JsonConvert.SerializeObject(fileCategories, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);
        }

    }
}
#endif
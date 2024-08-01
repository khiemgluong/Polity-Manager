using System;
using UnityEngine;
using System.Collections.Generic;

namespace KhiemLuong
{
    public class PolityManager : MonoBehaviour
    {
        public static PolityManager PM { get; private set; }
        [Tooltip("The largest, most important organizational unit in your game.")]
        public Polity[] polities;
        public PolityRelation[,] polityRelationMatrix;
        [SerializeField] string polityRelationMatrixString;
        public enum PolityRelation
        {
            Neutral,
            Allies,
            Enemies
        }

        [Serializable]
        class PolityRelationMatrixWrapper
        { public List<PolityRelation> relations = new(); public int rows, columns; }

        [SerializeField]
        [Tooltip("Set to true to persist this Singleton across scenes")] bool dontDestroyOnLoad;
        /* --------------------------------- EVENTS --------------------------------- */
        public static Action OnRelationChange, OnFactionChange;
        void Awake()
        {
            if (PM != null && PM != this) Destroy(gameObject);
            else
            {
                PM = this;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
        }

        void OnValidate() { ValidatePolityRelationMatrix(); SerializePolityRelationMatrix(); }

        [ContextMenu("Reset Polity Relation Matrix")]
        void ResetPolityRelationMatrix()
        {
            int size = polities.Length;
            polityRelationMatrix = new PolityRelation[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    polityRelationMatrix[i, j] = PolityRelation.Neutral;
            SerializePolityRelationMatrix();
            ValidatePolityRelationMatrix();
        }
        void ValidatePolityRelationMatrix()
        {
            LoadPolityRelationMatrix();
            if (polityRelationMatrix == null ||
                polityRelationMatrix.GetLength(0) != polities.Length ||
                polityRelationMatrix.GetLength(1) != polities.Length)
            {
                // Create a temporary matrix to hold existing data
                PolityRelation[,] tempMatrix = new PolityRelation[polities.Length, polities.Length];
                if (polityRelationMatrix != null)
                {
                    int minRows = Mathf.Min(polityRelationMatrix.GetLength(0), polities.Length);
                    int minCols = Mathf.Min(polityRelationMatrix.GetLength(1), polities.Length);

                    for (int i = 0; i < minRows; i++)
                        for (int j = 0; j < minCols; j++)
                            tempMatrix[i, j] = polityRelationMatrix[i, j];
                }
                // Replace the old matrix with the new matrix of appropriate size
                polityRelationMatrix = tempMatrix;
                CheckForDuplicatePolityNames();
            }
        }

        [ContextMenu("Find Duplicate Polity Names")]
        void CheckForDuplicatePolityNames()
        {
            Dictionary<string, int> nameIndex = new();
            for (int i = 0; i < polities.Length; i++)
            {
                if (nameIndex.ContainsKey(polities[i].name))
                    Debug.LogWarning($"Duplicate name found: {polities[i].name} at {i}");
                else nameIndex[polities[i].name] = i;
            }
        }
        [ContextMenu("Load Polity Relation Matrix")]
        // Unity can't serialize & deserialize matrices, so this is a custom approach around it.
        void LoadPolityRelationMatrix() =>
            polityRelationMatrix = DeserializePolityRelationMatrixMatrix();

        /* -------------------------------------------------------------------------- */
        /*                             PUBLIC API METHODS                             */
        /* -------------------------------------------------------------------------- */

        /* ------------------------------- SERIALIZERS ------------------------------ */
        public string SerializePolityRelationMatrix(PolityRelation[,] polityRelationMatrix)
        {
            PolityRelationMatrixWrapper wrapper = new()
            {
                rows = polityRelationMatrix.GetLength(0),
                columns = polityRelationMatrix.GetLength(1)
            };

            for (int i = 0; i < wrapper.rows; i++)
                for (int j = 0; j < wrapper.columns; j++)
                    wrapper.relations.Add(polityRelationMatrix[i, j]);
            polityRelationMatrixString = JsonUtility.ToJson(wrapper);
            return polityRelationMatrixString;
        }

        public string SerializePolityRelationMatrix() =>
            SerializePolityRelationMatrix(polityRelationMatrix);

        public PolityRelation[,] DeserializePolityRelationMatrixMatrix(string json)
        {
            if (json.Equals("")) return null;
            PolityRelationMatrixWrapper wrapper = JsonUtility.FromJson<PolityRelationMatrixWrapper>(json);
            PolityRelation[,] matrix = new PolityRelation[wrapper.rows, wrapper.columns];
            int index = 0;
            for (int i = 0; i < wrapper.rows; i++)
                for (int j = 0; j < wrapper.columns; j++)
                    matrix[i, j] = wrapper.relations[index++];
            return matrix;
        }
        public PolityRelation[,] DeserializePolityRelationMatrixMatrix() =>
            DeserializePolityRelationMatrixMatrix(polityRelationMatrixString);

        /* --------------------------------- GETTERS -------------------------------- */
        /// <summary>
        /// Gets the current PolityRelation from one PolityMember to another.
        /// </summary>
        /// <returns>The PolityRelation enum as Neutral, Allies, or Enemies.</returns>
        public PolityRelation GetPolityRelation(PolityMember polityMember, PolityMember theirPolityMember) =>
               GetPolityRelation(polityMember.polityName, theirPolityMember.polityName);
        public PolityRelation GetPolityRelation(string yourPolityName, string theirPolityName)
        {
            int yourIndex = Array.FindIndex(polities, p => p.name == yourPolityName);
            int theirIndex = Array.FindIndex(polities, p => p.name == theirPolityName);
            if (yourIndex == -1 || theirIndex == -1)
            { Debug.LogError("One or both polity names not found."); return default; }

            PolityRelation relation = polityRelationMatrix[yourIndex, theirIndex];
            Debug.Log($"Relationship w/ {yourPolityName} & {theirPolityName}: {relation} ({yourIndex},{theirIndex})");
            return relation;
        }

        /// <summary>
        /// Gets the emblem texture of the polity, or its class and faction if those properties have been provided.
        /// </summary>
        public Texture2D GetPolityEmblem(PolityStruct _struct)
        {
            Texture2D emblem;
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return null; }
            foreach (var polity in polities)
                if (_struct.polityName.Equals(polity.name))
                {
                    emblem = polity.emblem;
                    if (!string.IsNullOrEmpty(_struct.className))
                    {
                        foreach (var polityClass in polity.classes)
                            if (_struct.className.Equals(polityClass.name))
                            {
                                emblem = polityClass.emblem;
                                if (!string.IsNullOrEmpty(_struct.factionName))
                                {
                                    foreach (var faction in polityClass.factions)
                                        if (_struct.factionName.Equals(faction.name))
                                            return faction.emblem;
                                    Debug.LogError("No Faction Found");
                                    return emblem;
                                }
                                return emblem;
                            }
                        Debug.LogError("No Class Found");
                        return emblem;
                    }
                    return emblem;
                }
            Debug.LogError("No Polity Found"); return null;
        }

        public PolityMember GetPolityLeader(PolityStruct _struct)
        {
            PolityMember leader;
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return null; }
            foreach (var polity in polities)
                if (_struct.polityName.Equals(polity.name))
                {
                    leader = polity.leader;
                    if (!string.IsNullOrEmpty(_struct.className))
                    {
                        foreach (var polityClass in polity.classes)
                            if (_struct.className.Equals(polityClass.name))
                            {
                                leader = polityClass.leader;
                                if (!string.IsNullOrEmpty(_struct.factionName))
                                {
                                    foreach (var faction in polityClass.factions)
                                        if (_struct.factionName.Equals(faction.name))
                                            return faction.leader;
                                    Debug.LogError("No Faction Found");
                                    return leader;
                                }
                                return leader;
                            }
                        Debug.LogError("No Class Found");
                        return leader;
                    }
                    return leader;
                }
            Debug.LogError("No Polity Found"); return null;
        }

        /* --------------------------------- SETTERS -------------------------------- */
        /// <summary>
        /// Sets a new relation of one polity to another by their name, to FactionRelation
        /// </summary>
        /// <param name="theirPolityName">The string of the polity name that is selected.</param>
        /// <param name="newRelation">The new relation to set; Neutral, Allies or Enemies</param>
        public void ModifyPolityRelation(string thisPolityName, string theirPolityName, PolityRelation newRelation)
        {
            // string yourPolityName = polityMember.polityName;
            int thisIndex = Array.FindIndex(polities, p => p.name == thisPolityName);
            int theirIndex = Array.FindIndex(polities, p => p.name == theirPolityName);
            if (thisIndex == -1 || theirIndex == -1)
            {
                Debug.LogError("One or both polity names not found.");
                return;
            }
            polityRelationMatrix[thisIndex, theirIndex] = newRelation;
            polityRelationMatrix[theirIndex, thisIndex] = newRelation;
            OnRelationChange?.Invoke();
            Debug.Log($"Set relation between {thisPolityName} & {theirPolityName} to {newRelation}");
        }

        public void ModifyPolityRelation(PolityMember polityMember, string theirPolityName, PolityRelation newRelation)
            => ModifyPolityRelation(polityMember.polityName, theirPolityName, newRelation);

        /// <summary>
        /// Adds a faction to a polity, requiring a matching polityName and className to work.
        /// </summary>
        public Faction AddFactionToPolity(PolityStruct _struct, Texture2D emblem, PolityMember leader)
        {
            Faction newFaction = new(_struct.factionName, emblem, leader);
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return null; }

            foreach (var polity in polities)
                if (_struct.polityName.Equals(polity.name))
                    if (!string.IsNullOrEmpty(_struct.className))
                    {
                        foreach (var polityClass in polity.classes)
                            if (_struct.className.Equals(polityClass.name))
                                if (!string.IsNullOrEmpty(_struct.factionName))
                                {
                                    bool factionExists = false;
                                    foreach (var faction in polityClass.factions)
                                        if (_struct.factionName.Equals(faction.name))
                                        {
                                            Debug.LogWarning("Faction already exists");
                                            factionExists = true; break;
                                        }
                                    if (!factionExists)
                                    {
                                        polityClass.factions.Add(newFaction);
                                        OnFactionChange?.Invoke();
                                        return newFaction;
                                    }
                                    Debug.LogError("No Faction Found");
                                }
                        Debug.LogError("No Class Found");
                    }
            return null;
        }
        public void AddFactionToPolity(PolityStruct _struct) => AddFactionToPolity(_struct, null, null);

        /// <summary>
        /// Remove a faction of a polity, if the PolityStruct polityName, className and factionName all match.
        /// </summary>
        public void RemoveFactionFromPolity(PolityStruct _struct)
        {
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return; }

            foreach (var polity in polities)
                if (_struct.polityName.Equals(polity.name))
                {
                    if (string.IsNullOrEmpty(_struct.className))
                    { Debug.LogError("No Class Name Provided"); return; }

                    foreach (var polityClass in polity.classes)
                        if (_struct.className.Equals(polityClass.name))
                        {
                            if (string.IsNullOrEmpty(_struct.factionName))
                            { Debug.LogError("No Faction Name Provided"); return; }
                            for (int i = 0; i < polityClass.factions.Count; i++)
                                if (_struct.factionName.Equals(polityClass.factions[i].name))
                                {
                                    polityClass.factions.RemoveAt(i);
                                    Debug.Log("Faction found and removed");
                                    OnFactionChange?.Invoke(); return;
                                }
                            return;
                        }
                    Debug.LogError("No Class Found"); return;
                }
        }

        /* -------------------------------------------------------------------------- */
        /*                                POLITYSTRUCT                                */
        /* -------------------------------------------------------------------------- */
        /// <summary>
        /// This struct declares a specific polity's name, class and faction.
        /// </summary>
        public struct PolityStruct
        {
            /// <summary>
            /// The selected polity name.
            /// </summary>
            public string polityName;
            public bool isPolityLeader;
            /// <summary>
            /// The selected class within the polityName.
            /// </summary>
            public string className;
            public bool isClassLeader;
            /// <summary>
            /// The selected faction within the className.
            /// </summary>
            public string factionName;
            public bool isFactionLeader;

        }

        /* -------------------------------------------------------------------------- */
        /*                             SERIALIZED CLASSES                             */
        /* -------------------------------------------------------------------------- */
        /// <summary>
        /// Represents the largest & most important political unit such as a government body or main team.
        /// </summary>
        [Serializable]
        public class Polity : PolityBase
        {
            [Tooltip("A social class, government branch, organization, or any large collective corp.")]
            public Class[] classes;
        }
        /// <summary>
        /// Could represent a social class, government branch, organization, or any large collective corp.
        /// </summary>
        [Serializable]
        public class Class : PolityBase
        {
            [Tooltip("A small temporary political unit, such as a bandit squad or impromptu team.")]
            public List<Faction> factions;
        }

        /// <summary>
        /// Could represent a temporary political unit, which can be added and removed at runtime.
        /// </summary>
        [Serializable]
        public class Faction : PolityBase
        {
            public Faction(string _name, Texture2D _emblem, PolityMember _leader)
            {
                name = _name;
                emblem = _emblem;
                leader = _leader;
            }
        }

        [SerializeField]
        public abstract class PolityBase
        {
            [Tooltip("The name of the polity unit.")]
            public string name;
            /// <summary>
            /// Can represent a standard, vexillum, ensign, coat of arms or a team color.
            /// </summary>
            [Tooltip("A standard, vexillum, ensign, coat of arms or a team color.")]
            // [JsonIgnore]
            public Texture2D emblem;
            /// <summary>
            /// The leader of this specific unit, e.g. an emperor, queen or manager.
            /// </summary>
            [Tooltip("The leader of this unit, e.g. an emperor, queen or manager.")]
            // [JsonIgnore]
            public PolityMember leader;
        }
    }
}

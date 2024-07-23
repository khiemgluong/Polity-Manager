using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KhiemLuong
{
    public class PolityManager : MonoBehaviour
    {
        public static PolityManager PM { get; private set; }
        [Tooltip("The largest, most important political unit such as a government body or main team.")]
        public Polity[] polities;
        [Serializable]
        public enum PolityRelation
        {
            Neutral,
            Allies,
            Enemies
        }
        public PolityRelation[,] relationships;
        [SerializeField] string serializedRelationships;
        [SerializeField]
        [Tooltip("Set to true to persist this Singleton across scenes")] bool dontDestroyOnLoad;
        /* --------------------------------- EVENTS --------------------------------- */
        public static Action OnRelationChange;
        public static Action OnFactionChange;

        void Awake()
        {
            if (PM != null && PM != this) Destroy(gameObject);
            else
            {
                PM = this;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            DeserializePolityMatrix();
        }

        [ContextMenu("Reset Polity Matrix")]
        void ResetPolityMatrix()
        {
            int size = polities.Length;
            relationships = new PolityRelation[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    relationships[i, j] = PolityRelation.Neutral;
            SerializePolityMatrix();
        }

        /* -------------------------------------------------------------------------- */
        /*                             PUBLIC API METHODS                             */
        /* -------------------------------------------------------------------------- */

        /* --------------------------------- GETTERS -------------------------------- */
        public PolityRelation[,] DeserializePolityMatrix(string _serializedRelationships)
        {
            if (!string.IsNullOrEmpty(_serializedRelationships))
                relationships = JsonConvert.DeserializeObject<PolityRelation[,]>(_serializedRelationships);
            return relationships;
        }
        public PolityRelation[,] DeserializePolityMatrix() => DeserializePolityMatrix(serializedRelationships);
        /// <summary>
        /// Deserializes a string represent the Polity[] array, it does not overwrite the actual polities variable
        /// </summary>
        /// <param name="serializedPolities">The string serialized from SerializePolities()</param>
        /// <returns>The Polity[] object and its children, except Texture2D emblem and PolityMember leader.</returns>
        public Polity[] DeserializePolities(string serializedPolities)
        {
            Polity[] _polities = null; if (!string.IsNullOrEmpty(serializedPolities))
                _polities = JsonConvert.DeserializeObject<Polity[]>(serializedPolities);
            return _polities;
        }
        public Polity[] DeserializePolities() => DeserializePolities(SerializePolities());

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

            PolityRelation relation = relationships[yourIndex, theirIndex];
            Debug.Log($"Relationship between {yourPolityName} & {theirPolityName} is {relation} at {yourIndex},{theirIndex}");
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
        public string SerializePolityMatrix()
        {
            serializedRelationships = JsonConvert.SerializeObject(relationships);
            return serializedRelationships;
        }
        public string SerializePolities()
        {
            string serializedPolities = JsonConvert.SerializeObject(polities);
            Debug.Log("serialized polities: " + serializedPolities);
            return serializedPolities;
        }
        /// <summary>
        /// Sets a new relation of one polity to another by their name, to FactionRelation
        /// </summary>
        /// <param name="theirPolityName">The string of the polity name that is selected, retrieved from polityName in PolityMember.</param>
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
            relationships[thisIndex, theirIndex] = newRelation;
            relationships[theirIndex, thisIndex] = newRelation;
            SerializePolityMatrix();
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
            [JsonIgnore]
            public Texture2D emblem;
            /// <summary>
            /// The leader of this specific unit, e.g. an emperor, queen or manager.
            /// </summary>
            [Tooltip("The leader of this unit, e.g. an emperor, queen or manager.")]
            [JsonIgnore]
            public PolityMember leader;
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
    }
}

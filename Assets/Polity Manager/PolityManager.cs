using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.UI;

namespace KhiemLuong
{
    public class PolityManager : MonoBehaviour
    {
        public static PolityManager PM { get; private set; }
        public PolityMember playerPolity;
        public Polity[] polities;
        public static Action OnPolityRelationChange;
        [Serializable]
        public enum PolityRelation
        {
            Neutral,
            Allies,
            Enemies
        }
        public PolityRelation[,] relationships;
        [SerializeField] string serializedRelationships;

        void Awake()
        {
            if (PM != null && PM != this)
                Destroy(gameObject);
            else PM = this;
            DeserializePolityMatrix();
        }

        [ContextMenu("Load Polity Matrix")]
        void LoadSerializedPolityMatrix() => DeserializePolityMatrix();

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
        public void DeserializePolityMatrix(string _serializedRelationships)
        {
            if (!string.IsNullOrEmpty(_serializedRelationships))
                relationships = JsonConvert.DeserializeObject<PolityRelation[,]>(_serializedRelationships);
        }
        public void DeserializePolityMatrix() => DeserializePolityMatrix(serializedRelationships);

        /* --------------------------------- Getters -------------------------------- */
        /// <summary>
        /// Gets the current PolityRelation from one PolityMember to another.
        /// </summary>
        /// <returns>The PolityRelation enum as Neutral, Allies, or Enemies.</returns>
        public PolityRelation ComparePolityRelation(PolityMember polityMember, PolityMember otherPolityMember) =>
               ComparePolityRelation(polityMember, otherPolityMember.polityName);
        public PolityRelation ComparePolityRelation(PolityMember polityMember, string theirPolityName)
        {
            string yourPolityName = polityMember.polityName;
            int yourIndex = Array.FindIndex(polities, p => p.name == yourPolityName);
            int theirIndex = Array.FindIndex(polities, p => p.name == theirPolityName);
            if (yourIndex == -1 || theirIndex == -1)
            {
                Debug.LogError("One or both polity names not found.");
                return default;
            }

            PolityRelation relation = relationships[yourIndex, theirIndex];
            Debug.Log($"Relationship between {yourPolityName} & {theirPolityName} is {relation} at {yourIndex},{theirIndex}");
            return relation;
        }
        public string SerializePolityMatrix()
        {
            serializedRelationships = JsonConvert.SerializeObject(relationships);
            return serializedRelationships;
        }

        /// <summary>
        /// Gets the emblem texture of the polity, or its class and faction if the properties for those have been provided.
        /// </summary>
        public Texture GetPolityEmblem(PolityStruct _struct)
        {
            Texture emblem;
            if (!string.IsNullOrEmpty(_struct.polityName))
            {
                foreach (var polity in PM.polities)
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
                Debug.LogError("No Polity Found");
                return null;
            }
            else { Debug.LogError("No Polity Name Provided"); return null; }
        }

        // public PolityMember[] GetAllMembersFromPolity(PolityStruct _struct){

        // }
        /* --------------------------------- Setters -------------------------------- */
        /// <summary>
        /// Sets a new relation of one polity to another by their name, to FactionRelation
        /// </summary>
        /// <param name="theirPolityName">The string of the polity name that is selected, retrieved from polityName in PolityMember.</param>
        /// <param name="factionRelation">The new relation to set; Neutral, Allies or Enemies</param>
        public void ModifyPolityRelation(PolityMember polityMember, string theirPolityName, PolityRelation factionRelation)
        {
            string yourPolityName = polityMember.polityName;
            int memberIndex = Array.FindIndex(polities, p => p.name == yourPolityName);
            int theirIndex = Array.FindIndex(polities, p => p.name == theirPolityName);
            if (memberIndex == -1 || theirIndex == -1)
            {
                Debug.LogError("One or both polity names not found.");
                return;
            }
            relationships[memberIndex, theirIndex] = factionRelation;
            relationships[theirIndex, memberIndex] = factionRelation;
            SerializePolityMatrix();
            Debug.Log($"Modified relation between {polityMember.polityName} & {theirPolityName} to {factionRelation}");
        }


        public void AddFactionToPolity(ref PolityStruct @struct)
        {

        }
        public void RemoveFactionFromPolity(ref PolityStruct @struct)
        {

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
            [Tooltip("A social class, government branch, corporation, or any large collective corp.")]
            public Class[] classes;
        }
        /// <summary>
        /// Could represent a social class, government branch, corporation, or any large collective corp.
        /// </summary>
        [Serializable]
        public class Class : PolityBase
        {
            [Tooltip("A temporary political unit, such as a roving bandit squad or impromptu team.")]
            public List<Faction> factions;
        }

        /// <summary>
        /// Could represent a temporary political unit, such as a roving bandit squad or impromptu team.
        /// </summary>
        [Serializable]
        public class Faction : PolityBase
        {
        }

        [SerializeField]
        public abstract class PolityBase
        {
            [Tooltip("The name of the political unit.")]
            public string name;
            /// <summary>
            /// Can represent a standard, vexillum, ensign, coat of arms or a team color.
            /// </summary>
            [Tooltip("A standard, vexillum, ensign, coat of arms or a team color.")]
            public Texture emblem;
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
            /// <summary>
            /// The selected class within the polityName.
            /// </summary>
            public string className;
            /// <summary>
            /// The selected faction within the className.
            /// </summary>
            public string factionName;
        }
    }
}
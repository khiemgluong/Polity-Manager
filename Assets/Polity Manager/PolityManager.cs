using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace KhiemLuong
{
    public class PolityManager : MonoBehaviour
    {
        public static PolityManager Singleton { get; private set; }
        public PolityMember playerPolity;
        public Polity[] polities;
        public PolityRelation[,] relationships;
        public static Action OnPolityRelationChange;
        [Serializable]
        public enum PolityRelation
        {
            Neutral,
            Allies,
            Enemies
        }

        [SerializeField] string serializedRelationships;
        public bool DisableInteractivity { get; private set; }

        void Awake()
        {
            if (Singleton != null && Singleton != this)
                Destroy(gameObject);
            else
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            DeserializeMatrix();
            DisableInteractivity = true;
        }

        public void SerializeMatrix() =>
            serializedRelationships = JsonConvert.SerializeObject(relationships);

        [ContextMenu("Refresh Relation Matrix")]
        public void DeserializeMatrix()
        {
            if (!string.IsNullOrEmpty(serializedRelationships))
                relationships = JsonConvert.DeserializeObject<PolityRelation[,]>(serializedRelationships);
        }

        [ContextMenu("Reset Relation Matrix")]
        void ResetRelationMatrix()
        {
            int size = polities.Length;
            relationships = new PolityRelation[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    relationships[i, j] = PolityRelation.Neutral;
            SerializeMatrix();
        }

        public void ModifyPolityRelation(PolityMember polityMember, string otherPolityName, PolityRelation factionRelation)
        {
            int memberIndex = -1, otherIndex = -1;
            if (polityMember.polityName.Equals(otherPolityName))
            { Debug.LogError("Polity names match.", polityMember.gameObject); return; }
            for (int i = 0; i < polities.Length; i++)
            {
                Debug.Log("polityName " + polities[i].name + " otherFactionName " + otherPolityName);
                if (polities[i].name.Equals(polityMember.polityName))
                {
                    memberIndex = i;
                    Debug.LogError("found faction " + polityMember.polityName + " " + i);
                }
                if (polities[i].name.Equals(otherPolityName))
                {
                    otherIndex = i;
                    Debug.LogError("found other faction " + otherPolityName + " " + i);
                }
            }

            if (memberIndex == -1 || otherIndex == -1)
            {
                Debug.LogError("One or both faction names not found in the polities array.");
                return;
            }
            // Modify the relationship at the intersection of these indices in the matrix
            relationships[0, 1] = factionRelation;
            relationships[1, 0] = factionRelation;
            // DeserializeMatrix();
            Debug.LogError($"Modified relation between {polityMember.className} and {otherPolityName} to {factionRelation}");
        }

        public PolityRelation CompareFactionRelation(PolityMember polityMember, PolityMember otherPolityMember)
        {
            // Extract selected faction names from both polity members
            string polityMemberFactionName = polityMember.className;
            string otherPolityMemberFactionName = otherPolityMember.className;

            // Initialize indices to -1 to indicate 'not found'
            int memberIndex = -1;
            int otherMemberIndex = -1;

            // Loop through the polities array to find the indices
            for (int i = 0; i < polities.Length; i++)
            {
                if (polities[i].name == polityMemberFactionName)
                {
                    memberIndex = i;
                }
                if (polities[i].name == otherPolityMemberFactionName)
                {
                    otherMemberIndex = i;
                }

                // Early exit if both indices are found
                if (memberIndex != -1 && otherMemberIndex != -1)
                    break;
            }

            // Check if both indices were found
            if (memberIndex == -1 || otherMemberIndex == -1)
            {
                Debug.LogError("One or both faction names not found in the polities array.");
                return default;
            }

            PolityRelation relation = relationships[memberIndex, otherMemberIndex];
            Debug.Log($"The relationship between {polityMemberFactionName} and {otherPolityMemberFactionName} is {relation}  at index {memberIndex},{otherMemberIndex}");
            return relation;
        }
        public enum PolityType
        {
            Imperial,        // Centralized authority ruled by an emperor.
            Monarchy,        // Ruled by a king or queen.
            Theocracy,       // Governed by religious leadership.
            Republic,        // State in which power is held by the people and their elected representatives.
            Oligarchy,       // Small group of people having control of a country or organization.
            Technocracy,     // Government or control by technical experts.
            Anarchy,         // Absence of any form of political authority.
            Democracy        // Government by the people, directly or through elected representatives.
        }
        [Serializable]
        public class Polity
        {
            public string name;
            public Transform capital;
            public Class[] classes;
        }
        /// <summary>
        /// Could represent a social class, government branch, corporation, or any large collective unit
        /// </summary>
        [Serializable]
        public class Class
        {
            public string name;
            public Faction[] factions;
        }

        /// <summary>
        /// Represents a collection of families or units grouped by a common interest, locality, or purpose.
        /// </summary>
        [Serializable]
        public class Faction
        {
            public string name;
        }

        public interface IPolityStateChangeListener
        {
            void OnPolityStateChanged();
        }
    }
}
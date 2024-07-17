using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

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

        [ContextMenu("Reset Relation Matrix")]
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
        public void DeserializePolityMatrix()
        {
            if (!string.IsNullOrEmpty(serializedRelationships))
                relationships = JsonConvert.DeserializeObject<PolityRelation[,]>(serializedRelationships);
        }


        /* --------------------------------- Getters -------------------------------- */
        /// <summary>
        /// Gets the current PolityRelation from one PolityMember to another
        /// </summary>
        /// <returns>the PolityRelation enum as Neutral, Allies or Enemies</returns>
        public PolityRelation ComparePolityRelation(PolityMember polityMember, PolityMember otherPolityMember)
        {
            string yourPolityName = polityMember.polityName;
            string theirPolityName = otherPolityMember.polityName;

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

        // public PolityMember[] GetAllMembersFromPolity(PolityStruct @struct){

        // }
        /* --------------------------------- Setters -------------------------------- */
        /// <summary>
        /// Sets a new relation of one polity to another by their name, to FactionRelation
        /// </summary>
        /// <param name="theirPolityName">The string of the polity name that is selected</param>
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
            relationships[0, 1] = factionRelation;
            relationships[1, 0] = factionRelation;
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
        public class Polity
        {
            public string name;
            public Transform capital;
            public Class[] classes;
        }
        /// <summary>
        /// Could represent a social class, government branch, corporation, or any large collective corp.
        /// </summary>
        [Serializable]
        public class Class
        {
            public string name;
            public List<Faction> factions;
        }

        /// <summary>
        /// Could represent a tribe, clan or community grouped by a common interest, locality, or purpose.
        /// Referenced as a List, so it is the only political unit which can be added and removed at runtime.
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

        /* -------------------------------------------------------------------------- */
        /*                                POLITYSTRUCT                                */
        /* -------------------------------------------------------------------------- */
        /// <summary>
        /// 
        /// </summary>
        public struct PolityStruct
        {
            string polityName;
            int polityIndex;
            string className;
            int classIndex;
            string factionName;
            int factionIndex;
        }
    }
}
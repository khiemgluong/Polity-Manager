using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    [DisallowMultipleComponent]
    public class PolityMember : MonoBehaviour
    {
        [SerializeField] int selectedPolityIndex, selectedClassIndex, selectedFactionIndex;
        public string polityName, className, factionName;
        [SerializeField] bool isPolityLeader, isClassLeader, isFactionLeader;
        public List<PolityMember> parents;
        public List<PolityMember> partners;
        public List<PolityMember> children;

        /* --------------------------- ENCAPSULATED FIELDS -------------------------- */
        public int SelectedPolityIndex
        { get => selectedPolityIndex; private set => selectedPolityIndex = value; }
        public int SelectedClassIndex
        { get => selectedClassIndex; private set => selectedClassIndex = value; }
        public int SelectedFactionIndex
        { get => selectedFactionIndex; private set => selectedFactionIndex = value; }

        /* --------------------------------- EVENTS --------------------------------- */

        void OnEnable() => OnFactionChange += OnFactionChanged;

        void OnDisable() => OnFactionChange -= OnFactionChanged;

        void Awake() => Cleanup();

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                PolityStruct polityStruct = new()
                {
                    polityName = polityName,
                    className = className,
                    factionName = "Test"
                };
                PM.AddFactionToPolity(polityStruct);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                PolityStruct polityStruct = new()
                {
                    polityName = polityName,
                    className = className,
                    factionName = factionName
                };
                PM.RemoveFactionFromPolity(polityStruct);
            }
        }


        void OnFactionChanged()
        {
            bool isCurrentFactionStillAvailable = false;
            foreach (Faction faction in PM.polities[selectedPolityIndex].classes[selectedClassIndex - 1].factions)
            {
                if (faction.name.Equals(factionName))
                { isCurrentFactionStillAvailable = true; break; }
            }
            if (!isCurrentFactionStillAvailable)
            {
                Debug.Log(factionName + " is removed from factions list");
                selectedFactionIndex = 0; factionName = "";
            }
        }
        [ContextMenu("Cleanup")]
        void Cleanup()
        {
            parents = parents.Where(item => item != null).ToList();
            partners = partners.Where(item => item != null).ToList();
            children = children.Where(item => item != null).ToList();
        }

        [ContextMenu("Check Relationships")]
        void ResetRelationships()
        {
            Cleanup();
            CheckRelationship(parents, member => member.children, "parent");
            CheckRelationship(partners, member => member.partners, "partner");
            CheckRelationship(children, member => member.parents, "child");
        }
        void CheckRelationship(List<PolityMember> yourFamily, Func<PolityMember, List<PolityMember>> theirFamily, string relationshipType)
        {
            if (yourFamily.Any())
            {
                List<PolityMember> toRemove = new List<PolityMember>();
                foreach (PolityMember member in yourFamily)
                    if (!theirFamily(member).Contains(this))
                        toRemove.Add(member);

                foreach (PolityMember nonReciprocal in toRemove)
                {
                    yourFamily.Remove(nonReciprocal);
                    Debug.Log($"Removed non-reciprocal {relationshipType}: {nonReciprocal} from {this}'s {relationshipType} list.");
                }
            }
        }

        /* -------------------------------------------------------------------------- */
        /*                             PUBLIC API METHODS                             */
        /* -------------------------------------------------------------------------- */
        /// <summary>
        /// Changes the current PolityMember's polity, class and faction.
        /// Sets the member's polity based on what parameters were provided.
        /// </summary>
        public void ChangeMemberPolity(ref PolityStruct _struct)
        {
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return; }
            foreach (var polity in PM.polities)
                if (_struct.polityName.Equals(polity.name))
                {
                    polityName = polity.name;
                    selectedPolityIndex = Array.IndexOf(PM.polities, polity);
                    if (!string.IsNullOrEmpty(_struct.className))
                    {
                        foreach (var polityClass in polity.classes)
                            if (_struct.className.Equals(polityClass.name))
                            {
                                className = _struct.className;
                                selectedClassIndex = Array.IndexOf(polity.classes, polityClass) + 1;

                                if (!string.IsNullOrEmpty(_struct.factionName))
                                {
                                    foreach (var faction in polityClass.factions)
                                        if (_struct.factionName.Equals(faction.name))
                                        {
                                            factionName = _struct.factionName;
                                            selectedFactionIndex = polityClass.factions.IndexOf(faction) + 1;
                                            return; // All matches found, end the method
                                        }
                                    Debug.LogError("No Faction Match Found");
                                    selectedFactionIndex = 0;
                                    return;
                                }
                                return;
                            }
                        Debug.LogError("No Class Match Found");
                        selectedClassIndex = 0;
                        return;
                    }
                    return;
                }
            Debug.LogError("No Polity Match Found");
        }
        /* --------------------------------- Getters -------------------------------- */
        public PolityStruct GetMemberPolity()
        {
            PolityStruct polityStruct = new()
            {
                polityName = polityName,
                isPolityLeader = isPolityLeader,
                className = className,
                isClassLeader = isClassLeader,
                factionName = factionName,
                isFactionLeader = isFactionLeader
            }; return polityStruct;
        }
        public FamilyStruct GetMemberFamily()
        {
            FamilyStruct familyStruct = new()
            {
                parents = parents.ToArray(),
                partners = partners.ToArray(),
                children = children.ToArray(),
            }; return familyStruct;
        }

        /* -------------------------------------------------------------------------- */
        /*                                FAMILYSTRUCT                                */
        /* -------------------------------------------------------------------------- */
        /// <summary>
        /// This struct declares a PolityMember's current parents, partners and children array.
        /// </summary>
        public struct FamilyStruct
        {
            public PolityMember[] parents;
            public PolityMember[] partners;
            public PolityMember[] children;
        }
    }
}
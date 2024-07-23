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
        public static Action OnLeaderChange;

        void OnEnable() => OnFactionChange += OnFactionChanged;

        void OnDisable() => OnFactionChange -= OnFactionChanged;

        void Awake() => CleanupFamily();

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

        [ContextMenu("Check Family")]
        void ResetRelationships()
        {
            CleanupFamily();
            CheckRelationship(parents, member => member.children, "parent");
            CheckRelationship(partners, member => member.partners, "partner");
            CheckRelationship(children, member => member.parents, "child");
        }

        [ContextMenu("Delete Family")]
        void DeleteFamily()
        {
            // Remove this member from all partners' lists and vice versa
            foreach (PolityMember partner in new List<PolityMember>(partners))
                partner.partners.Remove(this);
            partners.Clear();
            foreach (PolityMember parent in new List<PolityMember>(parents))
                parent.children.Remove(this);
            parents.Clear();
            foreach (PolityMember child in new List<PolityMember>(children))
                child.parents.Remove(this);
            children.Clear();
        }
        [ContextMenu("Cleanup Family")]
        void CleanupFamily()
        {
            parents = parents.Where(item => item != null).ToList();
            partners = partners.Where(item => item != null).ToList();
            children = children.Where(item => item != null).ToList();
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
        ///Changes the current PolityMember's polity, class and faction based on what parameters were provided.
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
        }

        /// <summary>
        /// Sets this PolityMember to be the leader of a polity, or its class and faction
        /// </summary>
        public void SetAsPolityLeader(PolityStruct _struct) =>
            SetPolityLeader(this, _struct);

        public static void SetPolityLeader(PolityMember newLeader, PolityStruct _struct)
        {
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return; }
            foreach (var polity in PM.polities)
                if (_struct.polityName.Equals(polity.name))
                    if (!string.IsNullOrEmpty(_struct.className))
                    {
                        foreach (var _class in polity.classes)
                            if (_struct.className.Equals(_class.name))
                            {
                                if (!string.IsNullOrEmpty(_struct.factionName))
                                {
                                    foreach (var faction in _class.factions)
                                        if (_struct.factionName.Equals(faction.name))
                                        {
                                            faction.leader = newLeader;
                                            OnLeaderChange?.Invoke(); break;
                                        }
                                    Debug.LogError("No Faction Found");
                                }
                                else
                                {
                                    _class.leader = newLeader;
                                    OnLeaderChange?.Invoke(); break;
                                }
                            }
                        Debug.LogError("No Class Found");
                    }
                    else
                    { polity.leader = newLeader; OnLeaderChange?.Invoke(); break; }
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

        /// <summary>
        /// Gets all PolityMember classes in the scene, then filters out the members which belongs to the polity, class or faction to the PolityStruct
        /// </summary>
        /// <param name="_struct">Gets the PolityMember[] based on the provided struct values (polityName, className, factionName)</param>
        /// <param name="getInactive"></param>
        /// <returns>The array of filtered PolityMember[] belonging to the PolityStruct</returns>
        public static PolityMember[] GetMembersInPolity(PolityStruct _struct) => GetMembersInPolity(_struct, false);

        public static PolityMember[] GetMembersInPolity(PolityStruct _struct, bool getInactive)
        {
            if (string.IsNullOrEmpty(_struct.polityName))
            { Debug.LogError("No Polity Name Provided"); return null; }
            PolityMember[] members;
            if (!getInactive) members = FindObjectsOfType<PolityMember>();
            else members = Resources.FindObjectsOfTypeAll<PolityMember>();

            List<PolityMember> filteredMembers = new();
            if (members.Length > 0)
            {
                int polityI = -1, classI = -1, factionI = -1;
                foreach (var polity in PM.polities)
                    if (_struct.polityName.Equals(polity.name))
                    {
                        polityI = Array.IndexOf(PM.polities, polity);
                        if (!string.IsNullOrEmpty(_struct.className))
                        {
                            foreach (var _class in polity.classes)
                                if (_struct.className.Equals(_class.name))
                                {
                                    classI = Array.IndexOf(polity.classes, _class);
                                    if (!string.IsNullOrEmpty(_struct.factionName))
                                    {
                                        foreach (var faction in _class.factions)
                                            if (_struct.factionName.Equals(faction.name))
                                            {
                                                factionI = _class.factions.IndexOf(faction);
                                                break;
                                            }
                                        if (factionI == -1) Debug.LogError("No Faction Found");
                                        break;
                                    }
                                    break;
                                }
                            if (classI == -1) Debug.LogError("No Class Found");
                        }
                        break;
                    }

                if (polityI != -1)
                    foreach (PolityMember member in members)
                    {
                        bool match = true; if (polityI != -1 && !member.polityName.Equals(PM.polities[polityI].name)) match = false;
                        if (classI != -1 && !member.className.Equals(PM.polities[polityI].classes[classI].name)) match = false;
                        if (factionI != -1 && !member.factionName.Equals(PM.polities[polityI].classes[classI].factions[factionI].name))
                            match = false;
                        if (match) filteredMembers.Add(member);
                    }
                else Debug.LogError("No Polity Found");
            }
            return filteredMembers.ToArray();
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
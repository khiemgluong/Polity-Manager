using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    [DisallowMultipleComponent]
    public class PolityMember : MonoBehaviour
    {
        // public PolityMemberFamily polityMemberFamily;
        public string polityName;
        public bool polityLeader;
        public string factionName;
        public List<PolityMember> parents;
        public List<PolityMember> partners;
        public List<PolityMember> children;
        private void OnEnable()
        {
            OnFactionStateChange += OnFactionStateChanged;
        }

        private void OnDisable()
        {
            OnFactionStateChange -= OnFactionStateChanged;
        }

        void OnFactionStateChanged()
        {
        }
        [ContextMenu("Refresh")]
        void Refresh()
        {
            parents = parents.Where(item => item != null).ToList();
            partners = partners.Where(item => item != null).ToList();
            children = children.Where(item => item != null).ToList();
            if (parents.Any())
            {
                List<PolityMember> toRemove = new List<PolityMember>();  // List to keep track of partners to remove
                foreach (PolityMember parent in parents)
                {
                    if (!parent.parents.Contains(this))
                    {
                        toRemove.Add(parent);  // If not reciprocal, mark for removal
                    }
                }
                foreach (PolityMember nonReciprocal in toRemove)
                {
                    parents.Remove(nonReciprocal);
                    Debug.LogError($"Removed non-reciprocal parents: {nonReciprocal} from {this}'s partners list.");
                }
            }
            if (partners.Any())  // Check if there are any partners
            {
                List<PolityMember> toRemove = new List<PolityMember>();  // List to keep track of partners to remove
                foreach (PolityMember partner in partners)
                    if (!partner.partners.Contains(this))
                        toRemove.Add(partner);  // If not reciprocal, mark for removal

                foreach (PolityMember nonReciprocal in toRemove)
                {
                    partners.Remove(nonReciprocal);
                    Debug.LogError($"Removed non-reciprocal partner: {nonReciprocal} from {this}'s partners list.");
                }
            }
        }

    }

    /// <summary>
    /// This is the smallest polity unit, which refers to a surname that its members will carry
    /// </summary>
    [Serializable]
    public class FamilyObject
    {
        public string surname;
        public List<PolityMember> parents;
        public PolityMember partner;
        public List<PolityMember> children;
    }
}
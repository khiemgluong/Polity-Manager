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
        public string polityName;
        public bool polityLeader;
        public string factionName;
        public List<PolityMember> parents;
        public List<PolityMember> partners;
        public List<PolityMember> children;
        void OnEnable()
        {
            OnPolityStateChange += OnFactionStateChanged;
        }

        void OnDisable()
        {
            OnPolityStateChange -= OnFactionStateChanged;
        }

        void Awake()
        {
            parents = parents.Where(item => item != null).ToList();
            partners = partners.Where(item => item != null).ToList();
            children = children.Where(item => item != null).ToList();
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
            ValidateRelationships(parents, member => member.children, "parent");
            ValidateRelationships(partners, member => member.partners, "partner");
            ValidateRelationships(children, member => member.parents, "child");
        }
        void ValidateRelationships(List<PolityMember> ownList, Func<PolityMember, List<PolityMember>> getOppositeList, string relationshipType)
        {
            if (ownList.Any())
            {
                List<PolityMember> toRemove = new List<PolityMember>();
                foreach (PolityMember member in ownList)
                    if (!getOppositeList(member).Contains(this))
                        toRemove.Add(member);

                foreach (PolityMember nonReciprocal in toRemove)
                {
                    ownList.Remove(nonReciprocal);
                    Debug.Log($"Removed non-reciprocal {relationshipType}: {nonReciprocal} from {this}'s {relationshipType} list.");
                }
            }
        }
    }
}
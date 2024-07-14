using System;
using System.Collections.Generic;
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
        public FamilyObject family;

        public PolityMember father, mother;
        public PolityMember partner;
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
    }

    /// <summary>
    /// This is the smallest polity unit, which refers to a surname that its members will carry
    /// </summary>
    [Serializable]
    public class FamilyObject
    {
        public string surname;
        public PolityMember father, mother;
        public PolityMember partner;
        public List<PolityMember> children;
    }
}
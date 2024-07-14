using System;
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
        public PolityMember children;
        public bool polityLeader;
        public string factionName;
        public FamilyObject family;
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

    [Serializable]
    public class PolityMemberFamily
    {
        public string name;
        public string surname;
        public PolityMember father, mother;
        public PolityMember[] partners;
        public PolityMember[] children;
    }

}
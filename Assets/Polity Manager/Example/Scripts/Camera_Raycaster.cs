using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KhiemLuong
{
    using static KhiemLuong.PolityManager;
    using static KhiemLuong.PolityMember;
    public class CameraRaycaster : MonoBehaviour
    {
        public Image targetImage;
        public RawImage emblem;
        CanvasGroup canvasGroup;
        TextMeshProUGUI memberName, memberPolity, memberClass, memberFaction;
        TextMeshProUGUI parentName, partnerName, childrenName;
        void Start()
        {
            canvasGroup = targetImage.GetComponent<CanvasGroup>();
            Transform t = targetImage.transform;
            memberName = t.Find("Name").GetComponent<TextMeshProUGUI>();
            memberPolity = t.Find("Polity").GetComponent<TextMeshProUGUI>();
            memberClass = t.Find("Class").GetComponent<TextMeshProUGUI>();
            memberFaction = t.Find("Faction").GetComponent<TextMeshProUGUI>();
            /* --------------------------- FamilyStruct texts --------------------------- */
            parentName = t.Find("Parents").GetComponent<TextMeshProUGUI>();
            partnerName = t.Find("Partners").GetComponent<TextMeshProUGUI>();
            childrenName = t.Find("Children").GetComponent<TextMeshProUGUI>();
        }
        void Update()
        {
            if (targetImage == null)
            {
                Debug.LogError("Target Image not assigned in the inspector");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                canvasGroup.alpha = 1;
                if (hit.collider.TryGetComponent<PolityMember>(out var polityMember))
                {
                    memberName.text = polityMember.name;

                    PolityStruct polityStruct = polityMember.GetMemberPolity();
                    memberPolity.text = polityStruct.polityName;
                    Debug.LogError("Class " + polityStruct.className);
                    if (polityStruct.className.Equals("None"))
                        memberClass.enabled = false;
                    else
                    {
                        memberClass.enabled = true;
                        memberClass.text = polityStruct.className;
                    }
                    if (polityStruct.factionName.Equals("None"))
                        memberFaction.enabled = false;
                    else
                    {
                        memberFaction.enabled = true;
                        memberFaction.text = polityStruct.factionName;
                    }
                    /* --------------------------- FamilyStruct texts --------------------------- */

                    FamilyStruct familyStruct = polityMember.GetMemberFamily();
                    if (familyStruct.parents.Length == 0)
                        parentName.enabled = false;
                    else
                    {
                        parentName.enabled = true;
                        if (familyStruct.parents.Length > 1)
                            parentName.text = "Parents: " + familyStruct.parents.Length;
                        else parentName.text = "Parent: " + familyStruct.parents[0].name;
                    }
                    if (familyStruct.partners.Length == 0)
                        partnerName.enabled = false;
                    else
                    {
                        partnerName.enabled = true;
                        if (familyStruct.partners.Length > 1)
                            partnerName.text = "Partners: " + familyStruct.partners.Length;
                        else partnerName.text = "Partner: " + familyStruct.partners[0].name;
                    }
                    if (familyStruct.children.Length == 0)
                        childrenName.enabled = false;
                    else
                    {
                        childrenName.enabled = true;
                        if (familyStruct.children.Length > 1)
                            childrenName.text = "Children: " + familyStruct.children.Length;
                        else childrenName.text = "Child: " + familyStruct.children[0].name;
                    }

                }
                else
                {
                    memberName.text = "";
                    memberPolity.text = "";
                    memberFaction.text = "";

                    parentName.text = "";
                    partnerName.text = "";
                }

            }
            else canvasGroup.alpha = 0;

            Vector2 mousePosition = Input.mousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)targetImage.canvas.transform, mousePosition, targetImage.canvas.worldCamera, out Vector2 canvasPosition);
            // Set the target image position
            targetImage.rectTransform.anchoredPosition = canvasPosition;
        }
    }
}